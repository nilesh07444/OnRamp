using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Common.Data;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler
{
    public class TestExportReportQueryHandler :
        ReportingQueryHandler<TestExportReportQuery>
    {
        private readonly IRepository<Test> _testRepository;
        private readonly IRepository<Test_Result> _testResultRepository;
        private readonly IRepository<StandardUser> _userRepository;

        public TestExportReportQueryHandler(
            IRepository<Test> testRepository,
            IRepository<Test_Result> testResultRepository,
            IRepository<StandardUser> userRepository)
        {
            _testRepository = testRepository;
            _testResultRepository = testResultRepository;
            _userRepository = userRepository;
        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitent, TestExportReportQuery data)
        {
			var result = _testResultRepository.Find(data.ResultId);
			var test = new Test();
			if(result !=null) {
				test = _testRepository.Find(result.TestId);
			} else {
				result = _testResultRepository.GetAll().Where(x => x.TestId == data.ResultId).FirstOrDefault();
				test = _testRepository.Find(result.TestId);
			}
            if (test == null)
                throw new ArgumentNullException($"No test found with id : {result.TestId}");
            title = test.Title;
            recepitent = data.Recepients;
            data.AddOnrampBranding = true;
            data.HighlightCorrectAnswer = !string.IsNullOrEmpty(data.ResultId) ? test.HighlightAnswersOnSummary.HasValue ? test.HighlightAnswersOnSummary.Value : false : data.HighlightCorrectAnswer;
            IEnumerable<AttachmentModel> attachments = new List<AttachmentModel>();

            document.AddElement(CreateCoverSection(test, result));
            if (result == null)
            {
                document.AddElement(CreateIntroductionSection(test));
            }
            document.AddElement(CreateQuestionSection(test, data.HighlightCorrectAnswer, result.Questions, out attachments));
            if (attachments.Any())
                document.AddElement(CreateAnnexure(attachments));
        }

        private ReportSection CreateAnnexure(IEnumerable<AttachmentModel> attachments)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Annexure : A", headingFont));
            attachments.OrderBy(x => x.Parent).GroupBy(x => x.Parent).ToList().ForEach(delegate (IGrouping<int?, AttachmentModel> questionGroup)
            {
                section.AddElement(CreateParagraph(questionGroup.FirstOrDefault().Parent + ":", LeftAligned, headingFont));
                CreateAllImageAttachments(section, questionGroup.Where(x => x.Type == TrainingDocumentTypeEnum.Image));
                CreateAttachmentsBlock(questionGroup.Where(x => x.Type != TrainingDocumentTypeEnum.Image)).ToList().ForEach(x => section.AddElement(x));
            });
            return section;
        }

        private ReportSection CreateQuestionSection(Test test, bool highlightCorrectAnswer, ICollection<TestQuestion_Result> resultQuestions, out IEnumerable<AttachmentModel> attachmentModels)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Questions", headerFont, Centered));

            var attachmentModelsScoped = new List<AttachmentModel>();
            var questions = test.Questions.Where(q => !q.Deleted).OrderBy(q => q.Number).ToList();
            questions.ForEach(q =>
            {
                var index = questions.IndexOf(q);
                IEnumerable<AttachmentModel> attachments = new List<AttachmentModel>();
                section.AddElement(CreateQuestionAnswerBlock(q, index, highlightCorrectAnswer,
                    resultQuestions.FirstOrDefault(rq => rq.QuestionId == q.Id).Answers
                        .FirstOrDefault(a => a.Selected), out attachments));
                attachmentModelsScoped.AddRange(attachments);
            });
            attachmentModels = attachmentModelsScoped;
            return section;
        }

        private Element CreateQuestionAnswerBlock(TestQuestion question, int index, bool highlightCorrectAnswer, TestQuestionAnswer_Result userAnswer, out IEnumerable<AttachmentModel> attachments)
        {
            var block = new GridBlock();
            block.ColumnWidths.AddRange(new[] {5, 80});
            if (userAnswer != null && !string.IsNullOrEmpty(question.CorrectAnswerId))
                CreateTableDataRowWithStyles(block,
                    new[] {new FontElementStyle(new Font(largeFont.Font, FontStyle.Bold))}, string.Empty,
                    userAnswer.AnswerId == question.CorrectAnswerId ? "Correct" : "Incorrect");
            CreateTableDataRowWithStyles(block, new[] {new FontElementStyle(new Font(largeFont.Font, FontStyle.Bold))},
                index + 1, question.Question);
            var answers = question.Answers.Where(a => !a.Deleted).OrderBy(a => a.Number).ToList();
            foreach (var answer in answers)
            {
                var answerRow = CreateTableDataRowWithStyles(block, new[] {largeFont }, $"({answers.IndexOf(answer) + 1})", answer.Option != null ? string.Format("{0}       {1}", answer.Option.Trim(), userAnswer != null ? userAnswer.Answer.Id == answer.Id ? "(Your Answer)" : string.Empty : string.Empty) : "<NO ANSWER GIVEN!>");
                if (highlightCorrectAnswer && question.CorrectAnswerId != null && question.CorrectAnswerId == answer.Id)
                    answerRow.LastElement().AddStyle(new BackgroundColorElementStyle(Color.Yellow));
            }

            attachments = GetAttachments(question, index);
            return block;
        }

        private ReportSection CreateIntroductionSection(Test test)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Introduction", headingFont, Centered));
            section.AddElement(CreateParagraph(test.IntroductionContent, Centered));
            section.AddElement(CreateCenteredHorizontalRule());
            return section;
        }

        private ReportSection CreateCoverSection(Test test, Test_Result result = null)
        {
            var section = CreateSection("", PageOrientation.Portrait, result == null);
            section.AddElement(CreateParagraph(test.Title, hudgeFont, Centered));
            if (result == null)
                section.AddElement(CreateInfoGrid(test));
            else
            {
                section.AddElement(CreateInfoGrid(test, result));
            }
            return section;
        }

        private Element CreateInfoGrid(Test test, Test_Result result = null)
        {
            var infoGrid = CreateGrid();
            infoGrid.ColumnWidths.AddRange(new []{ 50, 50 });
            if (result == null)
            {
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Allow Review", test.TestReview ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Assign Marks To Each Question", test.AssignMarksToQuestions ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Randomized Questions", test.RandomizeQuestions ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Duration", $"{test.Duration} Minutes");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Maximum Attempts", test.MaximumAttempts);
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Pass Mark", $"{Math.Round(test.PassMarks, 2)}%");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Points", $"{test.Points}");
            }
            else
            {
                var user = _userRepository.Find(result.UserId.AsGuid());
                CreateTableDataRowWithStyles(infoGrid, new[] {headingFont}, "Username",
                    user != null ? Project.UserViewModelFrom(user)?.FullName : string.Empty);
            }

            return infoGrid;
        }

        private IEnumerable<AttachmentModel> GetAttachments(IContentUploads content, int index, bool data = false)
        {
            var attachments = new List<AttachmentModel>();
            var parent = index + 1;
            var reference = $"Question {parent}";

            content.Uploads.ToList().ForEach(upload =>
            {
                if (Path.GetExtension(upload.Name).ToLower().Equals(".mp3"))
                    upload.Type = TrainingDocumentTypeEnum.Audio.ToString();
                if (Enum.TryParse<TrainingDocumentTypeEnum>(upload.Type, true, out var type))
                {
                    if (data)
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,parent : parent,
                            includeData: data));
                    else
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,parent:parent,
                            includeData: data, maxWidth: 240, maxHeight: 240, reference: upload.Order.ToString()));
                }
            });
            content.ContentToolsUploads.ToList().ForEach(upload =>
            {
                if (Path.GetExtension(upload.Name).ToLower().Equals(".mp3"))
                    upload.Type = TrainingDocumentTypeEnum.Audio.ToString();
                if (Enum.TryParse<TrainingDocumentTypeEnum>(upload.Type, true, out var type))
                {
                    if (data)
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data));
                    else
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data, maxWidth: 240, maxHeight: 240, reference: upload.Order.ToString()));
                }
            });

            return attachments;
        }
    }
}