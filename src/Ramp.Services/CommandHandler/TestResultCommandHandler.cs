using Common;
using Common.Command;
using Data.EF.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Test;
using Domain.Enums;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Ramp.Contracts.Events.TestManagement;
using Ramp.Contracts.Events.Account;

namespace Ramp.Services.CommandHandler
{
    public class TestResultCommandHandler : ICommandHandlerAndValidator<CreateTestResultCommand>,
                                            IEventHandler<StandardUserDeletedEvent>
    {
        private readonly IEventPublisher _eventPublisher;
        readonly ITransientRepository<Test> _testRepository;
        readonly ITransientRepository<StandardUser> _userRepository;
        readonly ITransientRepository<Test_Result> _repository;
        public TestResultCommandHandler(
            IEventPublisher eventPublisher,
            ITransientRepository<Test> testRepository,
            ITransientRepository<StandardUser> userRepository,
            ITransientRepository<Test_Result> repository)
        {
            _eventPublisher = eventPublisher;
            _testRepository = testRepository;
            _userRepository = userRepository;
            _repository = repository;
        }

        public CommandResponse Execute(CreateTestResultCommand command)
        {
            var test = _testRepository.Find(command.Id);
            var user = _userRepository.Find(command.UserId.ConvertToGuid());
            var result = new Test_Result
            {
                Id = $"{Guid.NewGuid()}",
                Created = DateTimeOffset.UtcNow,
                TestId = command.Id,
                UserId = command.UserId,
                Questions = command.ContentModels.AsQueryable().Select(Project.TestQuestionResultModel_TestQuestion_Result).ToArray(),
                Test = test,
                TimeTaken = TimeSpan.FromMinutes(test.Duration) - command.TimeLeft,
				IsGloballyAccessed = command.IsGlobalAccessed
			};
            foreach(var r in result.Questions) {
                var question = test.Questions.FirstOrDefault(x => x.Id == r.QuestionId);
                var selectedAnswer = r.Answers.FirstOrDefault(x => x.Selected);
                if (question != null && selectedAnswer != null)
                    r.Correct = question.CorrectAnswerId == selectedAnswer.AnswerId;
                foreach (var a in r.Answers)
                    a.Answer = question.Answers.FirstOrDefault(x => x.Id == a.AnswerId);
                r.Question = question;
            }
            var correctQuestionsIds = result.Questions.Where(x => x.Correct).Select(x => x.QuestionId).ToList();
            result.Total = command.ContentModels.Sum(x => x.Marks);
            result.Score = command.ContentModels.Where(x => correctQuestionsIds.Contains(x.Id)).Sum(x => x.Marks);
            result.Percentage = Math.Round(decimal.Parse(result.Score.ToString()) / decimal.Parse(result.Total.ToString()) * 100, 2);
            result.Passed = result.Percentage >= test.PassMarks;

            if (result.Passed && test.Certificate != null && test.Certificate.Upload != null && test.Certificate.Upload.Data != null) {
				result.Certificate = new Upload {
					Data = CreateCertificate(user, result, test.Certificate.Upload.Data),
					Id = Guid.NewGuid().ToString(),
					Name = $"{user.Id}_{test.Id}_{result.Id}.pdf",
					Type = FileUploadType.Certificate.ToString(),
					ContentType = "pdf"
				};
				result.CertificateThumbnailId = test.Certificate.UploadId;
			}
            _repository.Add(result);
            _repository.SaveChanges();

            if (test.EmailSummary)
            {
                _eventPublisher.Publish(new TestCompletedEvent
                {
                    TestResultModel = Project.Test_Result_TestResultModel.Compile().Invoke(result),
                    CompanyViewModel = command.PortalContext.UserCompany,
                    UserViewModel = Project.UserViewModelFrom(user),
                    Subject = TestCompletedEvent.DefaultSubject
                });
            }

            command.ResultId = result.Id;
            return null;
        }
        private byte[] CreateCertificate(StandardUser user, Test_Result r, byte[] cert)
        {
            byte[] result;
            try
            {
                //make it a bmp first
                System.Drawing.Image image = null;
                using (MemoryStream stream = new MemoryStream(cert))
                {
                    var temp = Bitmap.FromStream(stream, true, true);
                    using (var tempStream = new MemoryStream())
                    {
                        temp.Save(tempStream, ImageFormat.Bmp);
                        image = Bitmap.FromStream(tempStream, true, true);
                    }
                }
                using (var graphics = Graphics.FromImage(image))
                {

                    System.Drawing.Font font = new System.Drawing.Font("Times New Roman", 20.0f);
                    System.Drawing.Font fontlogo = new System.Drawing.Font("Times New Roman", 40.0f);
                    // Create text position
                    var name = user.FirstName.RemoveSpecialCharacters().Contains(" ") ? user.FirstName.RemoveSpecialCharacters() : $"{user.FirstName} {user.LastName}".RemoveSpecialCharacters();
                    int intWidth = (int)graphics.MeasureString(name, font).Width;
                    PointF pointUserName = new PointF(1250 - (intWidth / 2), 1150);
                    // Draw text User Name
                    graphics.DrawString(name, font, Brushes.Black, pointUserName);

                    int intWidthPlaybook = (int)graphics.MeasureString(r.Test.Title.RemoveSpecialCharacters(), font).Width;
                    PointF pointPlaybookName = new PointF(1250 - (intWidthPlaybook / 2), 1420);
                    // Draw text
                    graphics.DrawString(r.Test.Title.RemoveSpecialCharacters(), font, Brushes.Black, pointPlaybookName);

                    string score = string.Format("{0} %", r.Percentage.ToString());
                    int intWidthMarks = (int)graphics.MeasureString(score, font).Width;
                    PointF pointMarks = new PointF(1250 - (intWidthMarks / 2), 1700);
                    // Draw text total marks scored
                    graphics.DrawString(score, font, Brushes.Black, pointMarks);

                    int intWidthDate = (int)graphics.MeasureString(r.Created.ToString("dd-MMMM-yyyy"), font).Width;
                    PointF pointDate = new PointF(1250 - (intWidthDate / 2), 1900);

                    graphics.DrawString(r.Created.ToString("dd-MMMM-yyyy"), font, Brushes.Black, pointDate);
                    var document = new Document(PageSize.A4, 0, 0, 0, 0);
                    var pdfS = new MemoryStream();

                    PdfWriter.GetInstance(document, pdfS);
                    document.Open();
                    var images = iTextSharp.text.Image.GetInstance(image, ImageFormat.Bmp);
                    images.ScaleToFit(iTextSharp.text.PageSize.A4);
                    document.Add(images);
                    document.Close();
                    result = pdfS.ToArray();
                    pdfS.Dispose();
                }
                return result;
            }
            catch (DocumentException de)
            {
                //throw de;
                var docmsg = de.Message;
                throw de;
            }
            catch (IOException ex)
            {
                //throw ex;
                var msg = ex.Message;
                throw ex;
            }
        }

        public IEnumerable<IValidationResult> Validate(CreateTestResultCommand command)
        {
            var test = _testRepository.Find(command.Id);
            if (test == null)
                yield return new ValidationResult("Id", $"No Test found with id : {command.Id}");
            var user = _userRepository.Find(command.UserId.ConvertToGuid());
            if (user == null)
                yield return new ValidationResult("UserId", $"No User found with id : {command.UserId}");
        }

        public void Handle(StandardUserDeletedEvent @event)
        {
            if (!string.IsNullOrWhiteSpace(@event.Id))
            {
                _repository.List.AsQueryable().Where(x => x.UserId == @event.Id).ToList().ForEach(x => _repository.Delete(x));
                _repository.SaveChanges();
            }
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<TestQuestionResultModel, TestQuestion_Result>> TestQuestionResultModel_TestQuestion_Result =
            x => new TestQuestion_Result
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = x.Id,
                UnAnswered = x.State.UnAnswered,
                ViewLater = x.State.ViewLater,
                Answers = x.Answers.AsQueryable().Select(TestQuestionAnswerResultModel_TestQuestionAnswer_Result).ToArray()
            };
        public static readonly Expression<Func<TestQuestionAnswerResultModel, TestQuestionAnswer_Result>> TestQuestionAnswerResultModel_TestQuestionAnswer_Result =
            x => new TestQuestionAnswer_Result
            {
                AnswerId = x.Id,
                Id = Guid.NewGuid().ToString(),
                Selected = x.State.Selected
            };
    }
}
