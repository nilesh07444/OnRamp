using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class SaveDraftVersionOfTrainingTestCommandHandler :
        CommandHandlerBase<SaveDraftVersionOfTrainingTestCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<TrainingQuestion> _trainingQuestionRepository;
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly IRepository<TrainingQuestion> _questionRepository;
        private readonly IRepository<TestAnswer> _answerRepository;
        private readonly IRepository<FileUploads> _uploadRepository;
        public SaveDraftVersionOfTrainingTestCommandHandler(IRepository<TrainingTest> trainingTestRepository,
            IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<TrainingQuestion> trainingQuestionRepository,
            IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
            IRepository<TrainingQuestion> questionRepository,
            IRepository<TestAnswer> answerRepository,
            IRepository<FileUploads> uploadRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _trainingGuideRepository = trainingGuideRepository;
            _trainingQuestionRepository = trainingQuestionRepository;
            _questionUploadRepository = questionUploadRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _uploadRepository = uploadRepository;
        }

        public override CommandResponse Execute(SaveDraftVersionOfTrainingTestCommand command)
        {
            if (command.TrainingTestViewModel.ActivePublishDate.HasValue)
            {
                var originalTest = _trainingTestRepository.Find(command.TrainingTestViewModel.TrainingTestId);
                originalTest.DraftEditDate = DateTime.Now;
                _trainingTestRepository.SaveChanges();
            }
            new CommandDispatcher().Dispatch(new UpdateTrainingTestCommand
            {
                model = command.TrainingTestViewModel

            });
            return null;
        }
    }
}