using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.TestManagement
{
    public class MigrateTestsToDbCommandHandler : ICommandHandlerBase<MigrateTestsToDbCommand>
    {
        private readonly IRepository<TrainingTest> _trainingTestRepository;
        private readonly IRepository<Domain.Customer.Models.QuestionUpload> _questionUploadRepository;
        private readonly IRepository<TrainingQuestion> _trainingQuestionRepository;

        public MigrateTestsToDbCommandHandler(
            IRepository<TrainingTest> trainingTestRepository,
            IRepository<Domain.Customer.Models.QuestionUpload> questionUploadRepository,
            IRepository<TrainingQuestion> trainingQuestionRepository)
        {
            _trainingTestRepository = trainingTestRepository;
            _questionUploadRepository = questionUploadRepository;
            _trainingQuestionRepository = trainingQuestionRepository;
        }

        public CommandResponse Execute(MigrateTestsToDbCommand command)
        {
            foreach (var test in _trainingTestRepository.List.ToList())
            {
                foreach (var question in test.QuestionList)
                {
                    var saveLocationManager = new RampLocationManager(command.PathToSaveUploadedFiles, command.CompanyName,
                                                                         test.TrainingGuide.Id, test.Id);
                    if (question.ImageName != null)
                    {
                        if (Directory.Exists(saveLocationManager.TestRoot))
                        {
                            string[] fileEntries = Directory.GetFiles(saveLocationManager.TestRoot);

                            foreach (string fileEntry in fileEntries)
                            {
                                if (Path.GetFileName(fileEntry).Equals(question.ImageName))
                                {
                                    string sourceFilePath = Path.Combine(saveLocationManager.TestRoot, Path.GetFileName(fileEntry));

                                    if (File.Exists(sourceFilePath))
                                    {
                                        question.Image = new Domain.Customer.Models.QuestionUpload
                                        {
                                            Id = Guid.NewGuid(),
                                            DocumentFileContent = Utility.Convertor.BytesFromFilePath(sourceFilePath),
                                            DocumentName = question.ImageName,
                                            DocumentType = TrainingDocumentTypeEnum.Image,
                                            TrainingQuestion = question
                                        };
                                        _questionUploadRepository.Add(question.Image);
                                        //string fileExtension = Path.GetExtension(sourceFilePath);
                                        //string fileNameToSave = trainingQuestion.Id.ToString() + fileExtension;
                                        //string filePath = Path.Combine(saveLocationManager.TestRoot, fileNameToSave);
                                        //File.Copy(sourceFilePath, filePath, true);
                                        //trainingQuestion.ImageName = fileNameToSave;
                                        File.Delete(sourceFilePath);
                                    }
                                }
                            }
                        }
                    }
                    if (question.VideoName != null)
                    {
                        if (Directory.Exists(saveLocationManager.TestRoot))
                        {
                            string[] fileEntries = Directory.GetFiles(saveLocationManager.TestRoot);

                            foreach (string fileEntry in fileEntries)
                            {
                                if (Path.GetFileName(fileEntry).Equals(question.VideoName))
                                {
                                    string sourceFilePath = Path.Combine(saveLocationManager.TestRoot, Path.GetFileName(fileEntry));

                                    if (File.Exists(sourceFilePath))
                                    {
                                        question.Video = new Domain.Customer.Models.QuestionUpload
                                        {
                                            Id = Guid.NewGuid(),
                                            DocumentFileContent = Utility.Convertor.BytesFromFilePath(sourceFilePath),
                                            DocumentName = question.VideoName,
                                            DocumentType = TrainingDocumentTypeEnum.Video,
                                            TrainingQuestion = question
                                        };
                                        _questionUploadRepository.Add(question.Video);
                                        //string fileExtension = Path.GetExtension(sourceFilePath);
                                        //string fileNameToSave = trainingQuestion.Id.ToString() + fileExtension;
                                        //string filePath = Path.Combine(saveLocationManager.TestRoot, fileNameToSave);
                                        //File.Copy(sourceFilePath, filePath, true);
                                        //trainingQuestion.ImageName = fileNameToSave;
                                        File.Delete(sourceFilePath);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var answer in question.TestAnswerList)
                    {
                        if (!string.IsNullOrWhiteSpace(answer.Option))
                        {
                            if (answer.Option.Equals(question.CorrectAnswer))
                            {
                                answer.Correct = true;
                                question.CorrectAnswer = answer.Id.ToString();
                            }
                        }
                    }
                }
                _trainingQuestionRepository.SaveChanges();
            }

            return null;
        }
    }
}