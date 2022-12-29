using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.CommandParameter.GuideManagement;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace Ramp.Services.CommandHandler.GuideManagement
{
    public class SaveGuideAndChaptersCommandHandler :
        CommandHandlerBase<SaveGuideAndChaptersCommand>
    {
        private readonly IRepository<TrainingGuide> _trainingGuideRepository;
        private readonly IRepository<Domain.Customer.Models.Categories> _categoryRepository;
        private readonly IRepository<StandardUser> _standardUserRepository;
        public const string DefaultPlaybookTitle = "[PlayBook]";

        public SaveGuideAndChaptersCommandHandler(IRepository<TrainingGuide> trainingGuideRepository,
            IRepository<Domain.Customer.Models.Categories> categoryRepository,
            IRepository<StandardUser> standardUserRepository)
        {
            _trainingGuideRepository = trainingGuideRepository;
            _categoryRepository = categoryRepository;
            _standardUserRepository = standardUserRepository;
        }

        public override CommandResponse Execute(SaveGuideAndChaptersCommand command)
        {
            var chapterIndex = 1;
            var category = _categoryRepository.List.AsQueryable().First(f => !f.Id.Equals(Guid.Empty));
            if (category.TrainingGuides == null)
                category.TrainingGuides = new List<TrainingGuide>();
            string lastRefId =
                LastReferenceIdForGuide.GetGuideReference(
                    new QueryExecutor().Execute<LastReferenceIdFromGuideQueryParameter, string>(
                        new LastReferenceIdFromGuideQueryParameter()));

            var trainingGuide = new TrainingGuide
            {
                Id = Guid.NewGuid(),
                CreatedBy = command.CreatedBy,
                Title = DefaultPlaybookTitle,
                IsActive = false,
                CreatedOn = DateTime.Now,
                ReferenceId = lastRefId
            };
            trainingGuide.Collaborators.Add(_standardUserRepository.Find(command.CreatedBy));
            category.TrainingGuides.Add(trainingGuide);
            for (var i = 0; i < 2; i++)
            {
                var traningGuideChapter = new TraningGuideChapter
                {
                    Id = Guid.NewGuid(),
                    ChapterNumber = chapterIndex,
                    ChapterName = "[Chapter " + chapterIndex + "]"
                };

                trainingGuide.ChapterList.Add(traningGuideChapter);
                chapterIndex++;
            }

            _categoryRepository.SaveChanges();
            _trainingGuideRepository.SaveChanges();

            return null;
        }
    }
}