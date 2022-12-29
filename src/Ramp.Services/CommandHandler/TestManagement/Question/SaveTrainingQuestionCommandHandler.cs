using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement.Answer;
using Ramp.Contracts.CommandParameter.TestManagement.Question;
using Ramp.Contracts.CommandParameter.TestManagement.QuestionUpload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement.Question
{
    public class SaveTrainingQuestionCommandHandler : ICommandHandlerBase<SaveTrainingQuestionCommand>
    {
        private readonly IRepository<TrainingTest> _testRepository;
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        public SaveTrainingQuestionCommandHandler(IRepository<TrainingTest> testRepository,
            IRepository<TrainingQuestion> questionRepository,
            ICommandDispatcher dispatcher)
        {
            _testRepository = testRepository;
            _dispatcher = dispatcher;
            _questionRepository = questionRepository;
        }
        public CommandResponse Execute(SaveTrainingQuestionCommand command)
        {
            var test = _testRepository.Find(command.TestId);
            var question = new TrainingQuestion
            {
                AnswerWeightage = command.Model.AnswerWeightage,
                Id = command.Model.TrainingTestQuestionId.Equals(Guid.Empty) ? Guid.NewGuid()  : command.Model.TrainingTestQuestionId,
                TestQuestion = command.Model.TestQuestion,
                TestQuestionNumber = command.Model.TestQuestionNumber,
                TrainingTestId = test.Id,
            };
            _questionRepository.Add(question);
            _questionRepository.SaveChanges();
            if(command.Model.TestAnswerList.Count > 0)
            {

                var list = command.Model.TestAnswerList.Where(x => !string.IsNullOrWhiteSpace(x.Option) && x.Option != "[Option]").ToList();
                foreach (var a in list)
                {
                    a.Position = a.Position.HasValue ? a.Position.Value : list.IndexOf(a);
                    var result = _dispatcher.Dispatch(new SaveTestAnswerCommand { Model = a, QuestionId = question.Id });
                    if (result.Validation.Any())
                        throw new Exception(result.Validation.First().Message);
                }
                if (command.Model.TestAnswerList.Any(x => x.Correct))
                   question.CorrectAnswer = command.Model.TestAnswerList.First(x => x.Correct == true).TestAnswerId.ToString();
            }

            if(command.Model.ImageContainer != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = command.Model.ImageContainer, QuestionId = question.Id,Type = QuestionUploadType.Image });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            if(command.Model.VideoContainer != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = command.Model.VideoContainer, QuestionId = question.Id, Type = QuestionUploadType.Video });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);

            }
            if(command.Model.AudioContainer != null)
            {
                var result = _dispatcher.Dispatch(new SaveQuestionUploadCommand { Model = command.Model.AudioContainer, QuestionId = question.Id, Type = QuestionUploadType.Audio });
                if (result.Validation.Any())
                    throw new Exception(result.Validation.First().Message);
            }
            return null;
        }
    }
}
