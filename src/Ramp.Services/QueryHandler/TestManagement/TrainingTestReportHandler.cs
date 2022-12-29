using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class TrainingTestReportHandler : ReportingQueryHandler<TrainingTestExportReportQuery>,IQueryHandler<TrainingTestExportReportQuery,IEnumerable<AttachmentModel>>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly IRepository<TestUserAnswer> _testUserAnswerRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<StandardUser> _userRepository;
        public TrainingTestReportHandler(IRepository<TrainingTest> testRepository,
                                         IRepository<TestUserAnswer> testUserAnswerRepository,
                                         IRepository<TestResult> testResultRepository,
                                         IRepository<StandardUser> userRepository)
        {
            _testRepository = testRepository;
            _testUserAnswerRepository = testUserAnswerRepository;
            _testResultRepository = testResultRepository;
            _userRepository = userRepository;
        }
        private ReportSection CreateCoverSection(TrainingTest test,TestResult result = null)
        {
            var section = CreateSection("", PageOrientation.Portrait, result == null);
            section.AddElement(CreateParagraph(test.TestTitle, hudgeFont, Centered));
            if (result == null)
                section.AddElement(CreateInfoGrid(test));
            else
            {
                section.AddElement(CreateInfoGrid(test,result));
            }
            return section;
        }
        private byte[] GetTrophy(TrainingTest test)
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TrophyPicDir"]);
            var temp = Path.Combine(path, "Temp");
            var old = Path.Combine(path, "Old");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!Directory.Exists(old))
                Directory.CreateDirectory(old);
            if (test.TestTrophy != null)
                return test.TestTrophy;
            else if (!string.IsNullOrWhiteSpace(test.TrophyName))
            {
                var trophyPath = Path.Combine(old, Path.GetFileNameWithoutExtension(test.TrophyName) + ".png");
                if (File.Exists(trophyPath))
                {
                    test.TestTrophy = File.ReadAllBytes(trophyPath);
                    _testRepository.SaveChanges();
                    return test.TestTrophy;
                }
            }
            return null;
        }
        private GridBlock CreateInfoGrid(TrainingTest test,TestResult result = null)
        {
            var infoGrid = CreateGrid();
            infoGrid.ColumnWidths.AddRange(new[] { 50, 50 });
            if (result == null)
            {
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Allow Review", test.TestReview ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Assign Marks To Each Question", test.AssignMarksToQuestions ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Disable Question Randomization", test.DisableQuestionRandomization ? "True" : "False");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Duration", $"{test.TestDuration} Minutes");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Expiry Date", test.TestExpiryDate.HasValue ? $"{test.TestExpiryDate.Value.ToShortDateString()}" : "N/A");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Linked Playbook", $"{test.TrainingGuide.Title}");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Maximum Rewrites", test.MaximumNumberOfRewites.HasValue ? $"{test.MaximumNumberOfRewites.Value}" : "N/A");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Pass Mark", $"{Math.Round(test.PassMarks, 2)}%");
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Points", $"{test.PassPoints}");

                var trophy = GetTrophy(test);
                if (trophy != null)
                {
                    var t = GetImage(trophy, 120, 120);
                    t.AddStyle(new BorderElementStyle(BorderStyle.None, 0, Color.Transparent));
                    CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, new ParagraphBlock("Trophy"), t);
                }
            }
            else
            {
                var user = _userRepository.Find(result.TestTakenByUserId);
                CreateTableDataRowWithStyles(infoGrid, new[] { headingFont }, "Username", user != null ? Project.UserViewModelFrom(user)?.FullName : string.Empty);
            }
            return infoGrid;
        }
        private string ProcessHTML(string html)
        {
            var result = html;
            var listOfAnchors = GetAllByTag("a", html);
            listOfAnchors.ForEach((tag) =>
            {
                var href = string.Empty;
                var hrefIndex = tag.IndexOf("href");
                if (hrefIndex > 0)
                {
                    href = tag.Substring(hrefIndex);
                    if (href.IndexOf("\"", 6) > -1)
                        href = href.Substring(6, href.IndexOf("\"", 6) - 6);
                }
                if (!string.IsNullOrWhiteSpace(href))
                    result = result.Replace(tag, $"{tag}({href})");
            });
            return result.Replace("<hr />", "<br>");
        }
        private ReportSection CreateIntroductionSection(TrainingTest test)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Introduction", headingFont, Centered));
            section.AddElement(GetImageFromHtml(ProcessHTML(test.IntroductionContent), 500, new HorizontalAlignmentElementStyle(HorizontalAlignment.Center)));
            section.AddElement(CreateCenteredHorizontalRule());
            return section;
        }
        private ReportSection CreateQuestionSection(TrainingTest test, bool heightLightCorrectAnswer,IEnumerable<TestUserAnswer> userAnswers, out IEnumerable<AttachmentModel> attachmentModels)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Questions", headingFont, Centered));

            var attachmentModelsScoped = new List<AttachmentModel>();
            var questions = test.QuestionList.OrderBy(x => x.TestQuestionNumber).ToList();
            questions.ForEach(delegate (TrainingQuestion question)
            {
                var index = questions.IndexOf(question);
                IEnumerable<AttachmentModel> attachments = new List<AttachmentModel>();
                    section.AddElement(CreateQuestionAnswerBlock(question, index, heightLightCorrectAnswer, userAnswers?.FirstOrDefault(x => x.Answer.TrainingQuestionId == question.Id), out attachments));
                attachmentModelsScoped.AddRange(attachments);
                if (attachments.Any())
                    section.AddElement(CreateParagraph("* See annexure: A, for attachments", largeFont));
                section.AddElement(CreateCenteredHorizontalRule());
            });
            attachmentModels = attachmentModelsScoped;
            return section;
        }
        private GridBlock CreateQuestionAnswerBlock(TrainingQuestion question,int indexOfQuestion,bool heightLightCorrectAnswer,TestUserAnswer userAnswer,out IEnumerable<AttachmentModel> attachments)
        {
            var block = new GridBlock();
            block.ColumnWidths.AddRange(new[] { 5, 80 });
            if (userAnswer != null && question.CorrectAnswer != null)
                CreateTableDataRowWithStyles(block, new[] { new FontElementStyle(new Font(largeFont.Font, FontStyle.Bold)) }, string.Empty, userAnswer.Answer.Id.ToString() == question.CorrectAnswer ? "Correct" : "Incorrect");
            CreateTableDataRowWithStyles(block, new[] { new FontElementStyle(new Font(largeFont.Font, FontStyle.Bold)) }, indexOfQuestion + 1, question.TestQuestion);
            var answers = question.TestAnswerList.OrderBy(x => x.Position).ToList();
            foreach (var answer in answers)
            {
                var answerRow = CreateTableDataRowWithStyles(block, new[] { largeFont }, $"({answers.IndexOf(answer) + 1})", answer.Option != null ? string.Format("{0}       {1}", answer.Option.Trim(), userAnswer != null ? userAnswer.Answer.Id == answer.Id ? "(Your Answer)" : string.Empty : string.Empty) : "<NO ANSWER GIVEN!>");
                if (heightLightCorrectAnswer && question.CorrectAnswer != null && question.CorrectAnswer == answer.Id.ToString())
                    answerRow.LastElement().AddStyle(new BackgroundColorElementStyle(Color.Yellow));
            }
            attachments = GetAttachments(question, indexOfQuestion);
            return block;
        }
        private IEnumerable<AttachmentModel> GetAttachments(TrainingQuestion question, int indexOfQuestion,bool data=false)
        {
            var attachments = new List<AttachmentModel>();
            var parent = indexOfQuestion + 1;
            var reference = $"Question {parent}";
            if (!data)
            {
                if (question.Image != null && question.Image.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Image, question.Image.Upload.Name, question.Image.Upload.Description, question.Image.Upload.Data, reference, parent, maxWidth: 1400, maxHeight: 800, includeData: data));
                if (question.Video != null && question.Video.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Video, question.Video.Upload.Name, question.Video.Upload.Description, question.Video.Upload.Data, reference, parent, maxWidth: 240, maxHeight: 240, includeData: data));
                if (question.Audio != null && question.Audio.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Audio, question.Audio.Upload.Name, question.Audio.Upload.Description, question.Audio.Upload.Data, reference, parent, maxHeight: 120, maxWidth: 120, includeData: data));
            }
            else
            {
                if (question.Image != null && question.Image.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Image, question.Image.Upload.Name, question.Image.Upload.Description, question.Image.Upload.Data, reference, parent, includeData: data));
                if (question.Video != null && question.Video.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Video, question.Video.Upload.Name, question.Video.Upload.Description, question.Video.Upload.Data, reference, parent, includeData: data));
                if (question.Audio != null && question.Audio.Upload != null)
                    attachments.Add(GetAttachmentModel(TrainingDocumentTypeEnum.Audio, question.Audio.Upload.Name, question.Audio.Upload.Description, question.Audio.Upload.Data, reference, parent, includeData: data));
            }
            return attachments;
        }
        private ReportSection CreateAnnexure(IEnumerable<AttachmentModel> attachments)
        {

            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Annexure : A", headingFont));
            attachments.OrderBy(x => x.Parent).GroupBy(x => x.Parent).ToList().ForEach(delegate (IGrouping<int?, AttachmentModel> questionGroup)
            {
                section.AddElement(CreateParagraph(questionGroup.FirstOrDefault().Reference + ":", LeftAligned, headingFont));
                CreateAllImageAttachments(section, questionGroup.Where(x => x.Type == TrainingDocumentTypeEnum.Image));
                CreateAttachmentsBlock(questionGroup.Where(x => x.Type != TrainingDocumentTypeEnum.Image)).ToList().ForEach(x => section.AddElement(x));
            });
            return section;
        }
        public override void BuildReport(ReportDocument document, out string title,out string recepitent, TrainingTestExportReportQuery data)
        {
            var test = _testRepository.Find(data.TestId);
            if (test == null)
                throw new ArgumentNullException($"No test found with id : {data.TestId}");
            var result = _testResultRepository.Find(data.ResultId);
            title = test.TestTitle;
            recepitent = string.Empty;
            data.AddOnrampBranding = true;
            data.HighlightCorrectAnswer = data.ResultId.HasValue ? test.HighlightAnswersOnSummary.HasValue ? test.HighlightAnswersOnSummary.Value : false : data.HighlightCorrectAnswer;
            IEnumerable<AttachmentModel> attachments = new List<AttachmentModel>();

            if (result == null)
            {
                document.AddElement(CreateCoverSection(test));
                document.AddElement(CreateIntroductionSection(test));
                document.AddElement(CreateQuestionSection(test, data.HighlightCorrectAnswer, _testUserAnswerRepository.List.AsQueryable().Where(x => x.Result.Id == data.ResultId), out attachments));
                if (attachments.Any())
                    document.AddElement(CreateAnnexure(attachments));
            }
            else
            {
                document.AddElement(CreateCoverSection(test,result));
                document.AddElement(CreateQuestionSection(test, data.HighlightCorrectAnswer, _testUserAnswerRepository.List.AsQueryable().Where(x => x.Result.Id == data.ResultId), out attachments));
                if (attachments.Any())
                    document.AddElement(CreateAnnexure(attachments));
            }
        }
        IEnumerable<AttachmentModel> IQueryHandler<TrainingTestExportReportQuery, IEnumerable<AttachmentModel>>.ExecuteQuery(TrainingTestExportReportQuery query)
        {

            var test = _testRepository.Find(query.TestId);
            var attachments = new List<AttachmentModel>();
            test.QuestionList.ForEach(x => attachments.AddRange(GetAttachments(x, test.QuestionList.IndexOf(x), true)));
            return attachments;
        }
    }
    
}
