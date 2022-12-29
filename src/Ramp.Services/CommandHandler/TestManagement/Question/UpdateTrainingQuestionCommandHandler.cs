using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using Ramp.Services.Projection;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement.Question
{
    public class UpdateTrainingQuestionCommandHandler : ICommandHandlerBase<UpdateTrainingQuestionCommand>
    {
        private readonly IRepository<TrainingQuestion> _questionRepository;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRepository<TrainingTest> _testRepository;
        public UpdateTrainingQuestionCommandHandler(IRepository<TrainingQuestion> questionRepository,ICommandDispatcher dispatcher,
            IRepository<TrainingTest> testRepository)
        {
            _questionRepository = questionRepository;
            _dispatcher = dispatcher;
            _testRepository = testRepository;
        }
        public CommandResponse Execute(UpdateTrainingQuestionCommand command)
        {
            var qEdit = command.Model;
            var qDomain = _questionRepository.Find(command.Model.TrainingTestQuestionId);

            //direct properties
            qDomain.AnswerWeightage = qEdit.AnswerWeightage;
            qDomain.TestQuestion = qEdit.TestQuestion;
            qDomain.TestQuestionNumber = qEdit.TestQuestionNumber;
            qDomain.CorrectAnswer = qEdit.CorrectAnswerId.ToString();
            _questionRepository.SaveChanges();

            //answers
            qEdit.TestAnswerList.Where(x => !string.IsNullOrWhiteSpace(x.Option) && !x.Option.Equals("[Option]")).ToList().ForEach(x => x.Position = qEdit.TestAnswerList.IndexOf(x));
            var currentAnswers = qDomain.TestAnswerList.Select(x => x.Id).ToList();
            var viewAnswers = qEdit.TestAnswerList.Select(x => x.TestAnswerId).ToList();
            var updateableAnswers = qEdit.TestAnswerList.Where(x => currentAnswers.Contains(x.TestAnswerId)).ToList();
            var removedAnswers = qDomain.TestAnswerList.Where(x => !viewAnswers.Contains(x.Id)).ToList();
            var newAnswers = qEdit.TestAnswerList.Where(x => !currentAnswers.Contains(x.TestAnswerId) && !string.IsNullOrWhiteSpace(x.Option) && !x.Option.Equals("[Option]")).ToList();

            foreach(var q in updateableAnswers)
            {
                var result = _dispatcher.Dispatch(new UpdateTestAnswerCommand { Model = q });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            foreach(var q in removedAnswers)
            {
                var result = _dispatcher.Dispatch(new DeleteTestAnswerCommand { Model = Project.TestAnswerViewModelFrom(q)});
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            foreach(var q in newAnswers)
            {
                var result = _dispatcher.Dispatch(new SaveTestAnswerCommand { Model = q ,QuestionId = q.TrainingQuestionId});
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            if (!string.IsNullOrWhiteSpace(qDomain.CorrectAnswer))
            {
                var id = Guid.Parse(qDomain.CorrectAnswer);
                if (qDomain.TestAnswerList.Any(x => x.Id.Equals(id)))
                    qDomain.TestAnswerList.Single(x => x.Id.Equals(id)).Correct = true;
            }
            else if (qDomain.TestAnswerList.Any(x => x.Correct))
                qDomain.CorrectAnswer = qDomain.TestAnswerList.Single(x => x.Correct).Id.ToString();

            //uploads

            var currentImageUpload = qDomain.Image;
            var editImageUpload = qEdit.ImageContainer;

            if (currentImageUpload == null && editImageUpload != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = editImageUpload , QuestionId = qDomain.Id ,Type = QuestionUploadType.Image});
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            else if (currentImageUpload != null && editImageUpload != null)
            {
                if (currentImageUpload.Upload.Id != editImageUpload.Id)
                {
                    var result = _dispatcher.Dispatch(new UpdateQuestionUploadCommand { Model = editImageUpload, QuestionUploadId = currentImageUpload.Id });
                    if (result.Validation.Any())
                        throw new Exception(result.Validation.First().Message);
                }
            }
            else if (currentImageUpload != null && editImageUpload == null)
            {
                var result = _dispatcher.Dispatch(new DeleteQuestionUploadCommand { QuestionUploadId = currentImageUpload.Id ,Type = QuestionUploadType.Image});
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }

            var currentVideoUpload = qDomain.Video;
            var editVideoUpload = qEdit.VideoContainer;

            if (currentVideoUpload == null && editVideoUpload != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = editVideoUpload, QuestionId = qDomain.Id, Type = QuestionUploadType.Video });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            else if (currentVideoUpload != null && editVideoUpload != null)
            {
                if (currentVideoUpload.Upload.Id != editVideoUpload.Id)
                {
                    var result = _dispatcher.Dispatch(new UpdateQuestionUploadCommand { Model = editVideoUpload, QuestionUploadId = currentVideoUpload.Id });
                    if (result.Validation.Any())
                        throw new Exception(result.Validation.First().Message);
                }
            }
            else if (currentVideoUpload != null && editVideoUpload == null)
            {
                var result = _dispatcher.Dispatch(new DeleteQuestionUploadCommand { QuestionUploadId = currentVideoUpload.Id, Type = QuestionUploadType.Video });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            var currentAudioUpload = qDomain.Audio;
            var editAudioUpload = qEdit.AudioContainer;

            if (currentAudioUpload == null && editAudioUpload != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = editAudioUpload, QuestionId = qDomain.Id, Type = QuestionUploadType.Audio });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            else if (currentAudioUpload != null && editAudioUpload != null)
            {
                if (currentAudioUpload.Upload.Id != editAudioUpload.Id)
                {
                    var result = _dispatcher.Dispatch(new UpdateQuestionUploadCommand { Model = editAudioUpload, QuestionUploadId = currentVideoUpload.Id });
                    if (result.Validation.Any())
                        throw new Exception(result.Validation.First().Message);
                }
            }
            else if (currentAudioUpload != null && editAudioUpload == null)
            {
                var result = _dispatcher.Dispatch(new DeleteQuestionUploadCommand { QuestionUploadId = currentAudioUpload.Id, Type = QuestionUploadType.Video });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            _questionRepository.SaveChanges();
            return null;
        }
    }
}
