using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models.Test;
using LinqKit;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler
{
    public class TestResultQueryHandler : IQueryHandler<FetchByIdQuery, TestResultModel>,
     IQueryHandler<TestResultCertificateByUserQuery, List<TestResultModel>>, IQueryHandler<FetchUserQuery, List<Test_Result>>
    {
        readonly ITransientRepository<Test_Result> _repository;
        readonly ITransientRepository<Test> _testRepository;
        public TestResultQueryHandler(ITransientRepository<Test_Result> repository,ITransientRepository<Test> testRepository)
        {
            _repository = repository;
            _testRepository = testRepository;
        }
		public List<TestResultModel> ExecuteQuery(TestResultCertificateByUserQuery query) {
			if (string.IsNullOrEmpty(query.UserId.ToString()))
				return null;
			var entry = _repository.List.AsQueryable().Where(c=>c.UserId==query.UserId && c.CertificateId!=null).ToList();
			if (entry == null)
				return null;
			
			var model = entry.Select(Project.Test_Result_TestResultModel.Compile()).ToList();
			return model;
		}

		public TestResultModel ExecuteQuery(FetchByIdQuery query)
        {
            if (string.IsNullOrEmpty(query.Id.ToString()))
                return null;
            var entry = _repository.Find(query.Id.ToString());
            if (entry == null)
                return null;
            if (entry.Test == null)
                entry.Test = _testRepository.Find(entry.TestId);
            var model = Project.Test_Result_TestResultModel.Compile().Invoke(entry);
            return model;
        }


        public List<Test_Result> ExecuteQuery(FetchUserQuery query)
        {
            if (string.IsNullOrEmpty(query.UserId.ToString()))
                return null;
            var entry = _repository.List.AsQueryable().Where(c => c.UserId == query.UserId.ToString()).ToList();
            if (entry == null)
                return null;
            return entry;
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<Test_Result, TestResultModel>> Test_Result_TestResultModel =
            x => new TestResultModel
            {
                CreatedBy = x.Test.CreatedBy,
                CreatedOn = x.Test.CreatedOn,
                LastEditedBy = x.Test.LastEditedBy,
                Deleted = x.Deleted,
                Description = x.Test.Description,
                Id = x.Id.ToString(),
                DocumentStatus = x.Test.DocumentStatus,
                LastEditDate = x.Test.LastEditDate,
                Points = x.Test.Points,
                PreviewMode = x.Test.PreviewMode,
                Printable = x.Test.Printable,
                ReferenceId = x.Test.ReferenceId,
                Title = x.Test.Title,
                TrainingLabels =x.Test.TrainingLabels,
                CoverPicture = x.Test.CoverPicture != null ? Upload_UploadResultViewModel.Invoke(x.Test.CoverPicture) : null,
                CoverPictureId = x.Test.CoverPictureId,
                Category = x.Test.Category != null ? Category_CategoryViewModelShort.Invoke(x.Test.Category) : null,
                CategoryId = x.Test.CategoryId,
                AssignMarksToQuestions = x.Test.AssignMarksToQuestions,
                Duration = x.Test.Duration,
                EmailSummary = x.Test.EmailSummary,
                HighlightAnswersOnSummary = x.Test.HighlightAnswersOnSummary,
                IntroductionContent = x.Test.IntroductionContent,
                PassMarks = x.Test.PassMarks,
                RandomizeQuestions = x.Test.RandomizeQuestions,
                TestExpiresNumberDaysFromAssignment = x.Test.TestExpiresNumberDaysFromAssignment,
                NotificationInteval = x.Test.NotificationInteval,
                NotificationIntevalDaysBeforeExpiry = x.Test.NotificationIntevalDaysBeforeExpiry,
                TestReview = x.Test.TestReview,
                DocumentType = DocumentType.Test,
                Certificate = x.Certificate != null ? Upload_UploadResultViewModel.Invoke(x.Certificate) : null,
                ContentModels = x.Questions.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Question.Number).Select(TestQuestion_Result_TestQuestionResultModel).ToArray(),
                CertificateId = x.CertificateId,
                NumberOfRightAnswers = x.Questions.Count(q => q.Correct),
                NumberOfWrongAnswers = x.Questions.Count(q => !q.Correct && !q.UnAnswered),
                NumberOfUnattemptedQuestions = x.Questions.Count(q => q.UnAnswered),
                Passed = x.Passed,
                Percentage = x.Percentage,
                Score = x.Score,
                Total = x.Total,
				CertificateThumbnailId=x.CertificateThumbnailId
            };
        public static readonly Expression<Func<TestQuestion_Result, TestQuestionResultModel>> TestQuestion_Result_TestQuestionResultModel =
            x => new TestQuestionResultModel
            {
                Marks = x.Question.AnswerWeightage,
                Deleted = x.Deleted,
                Id = x.QuestionId,
                Number = x.Question.Number,
                Question = x.Question.Question,
                CorrectAnswerId = x.Question.CorrectAnswerId,
                Attachments = x.Question.Uploads.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Order).Select(Upload_UploadResultViewModel).ToArray(),
                ContentToolsUploads = x.Question.ContentToolsUploads.AsQueryable().Where(e => !e.Deleted).Select(Upload_UploadFromContentToolsResultModel).ToArray(),
                Answers = x.Answers.AsQueryable().Where(e => !e.Deleted).OrderBy(e => e.Answer.Number).Select(TestQuestionAnswer_Result_TestQuestionAnswerResultModel).ToArray(),
                State = new TestQuestionStateModel { UnAnswered = x.UnAnswered, ViewLater = x.ViewLater },
                Correct = x.Correct
            };
        public static readonly Expression<Func<TestQuestionAnswer_Result, TestQuestionAnswerResultModel>> TestQuestionAnswer_Result_TestQuestionAnswerResultModel =
        x => new TestQuestionAnswerResultModel
        {
            Deleted = x.Deleted,
            Id = x.AnswerId,
            Number = x.Answer.Number,
            Option = x.Answer.Option,
            State = new TestQuestionAnswerStateModel { Selected = x.Selected },
        };
    }
}
