using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Course;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ramp.Services.CommandHandler;
using Ramp.Contracts.Query.Document;
using Common.Query;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Command.Document;

namespace Ramp.Services.CommandHandler.Courses
{
    public class AddOrUpdateCourseCommandHandler :
        ICommandHandlerBase<AddOrUpdateCourseCommand>,
        ICommandHandlerBase<AssignCourseToUsersCommand>,
         ICommandHandlerAndValidator<DeleteByIdCommand<Course>>,
        ICommandHandlerBase<AssignDocumentToCourseCommand>
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<AssignedCourse> _assignedCourseRepository;
        private readonly IRepository<AssociatedDocument> _associatedDcoumentRepository;
        private readonly IRepository<AssignedDocument> _assignedDcoumentRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public AddOrUpdateCourseCommandHandler(
            IRepository<Course> courseRepository,
            IRepository<AssignedCourse> assignedCourseRepository,
            IRepository<AssociatedDocument> associatedDcoumentRepository,
              IRepository<AssignedDocument> assignedDcoumentRepository,
           ICommandDispatcher commandDispatcher,
            IQueryExecutor queryExecutor
            )
        {
            _courseRepository = courseRepository;
            _assignedCourseRepository = assignedCourseRepository;
            _associatedDcoumentRepository = associatedDcoumentRepository;
            _commandDispatcher = commandDispatcher;
            _queryExecutor = queryExecutor;
            _assignedDcoumentRepository = assignedDcoumentRepository;
        }

        public CommandResponse Execute(AddOrUpdateCourseCommand command)
        {
            var response = new CommandResponse();
            try
            {
                if (command.Id == Guid.Empty)
                {
                    //add
                    var data = new Course();

                    data.Id = Guid.NewGuid();
                    data.Title = command.Title;
                    data.Description = command.Description;
                    data.AllocatedAdmins = command.AllocatedAdmins;
                    data.CategoryId = command.CategoryId;
                    data.StartDate = command.StartDate;
                    data.EndDate = command.EndDate;
                    data.GlobalAccessEnabled = command.GlobalAccessEnabled;
                    data.IsActive = true;
                    data.AchievementId = command.AchievementId != null ? Guid.Parse(command.AchievementId) : Guid.Empty;
                    data.Status = command.Status;
                    data.CreatedBy = command.CreatedBy;
                    data.CreatedOn = DateTime.Now;
                    data.ExpiryEnabled = command.ExpiryEnabled;
                    data.ExpiryInDays = command.ExpiryInDays;
                    data.Points = command.Points;
                    //var coverPicture = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                    //{
                    //	ExistingUploadId = command.CoverPicture?.Id,
                    //	ModelId = command.CoverPicture?.Id
                    //});
                    //data.CoverPicture = coverPicture.Name;

                    _courseRepository.Add(data);
                    _courseRepository.SaveChanges();

                    foreach (var doc in command.Documents)
                    {
                        _commandDispatcher.Dispatch(new AssignDocumentToCourseCommand
                        {
                            Id = Guid.Empty,
                            CourseId = data.Id,
                            DocumentId = Guid.Parse(doc.Id),
                            Title = doc.Title,
                            Type = doc.Type,
                            OrderNo = doc.OrderNo

                        });
                        AddDocumentInAssignedDocument(doc, command);
                    }

                    foreach (var user in command.AssignedUsers)
                    {
                        _commandDispatcher.Dispatch(new AssignCourseToUsersCommand
                        {

                            UserId = Guid.Parse(user.Replace(";null", "")),
                            CourseId = data.Id,
                            AssignedBy = command.CreatedBy,
                            //AdditionalMsg = command.AdditionalMsg,
                            AssignedDate = DateTime.Now,
                            Deleted = false,
                            IsRecurring = false
                        });
                    }
            

                }
                else if (command.Id != null)
                {
                    //update
                    var entity = _courseRepository.Find(command.Id);

                    entity.Title = command.Title;
                    entity.Description = command.Description;
                    entity.AllocatedAdmins = command.AllocatedAdmins;
                    entity.CategoryId = command.CategoryId;
                    entity.StartDate = command.StartDate;
                    entity.EndDate = command.EndDate;
                    entity.GlobalAccessEnabled = command.GlobalAccessEnabled;
                    entity.IsActive = true;
                    entity.AchievementId = command.AchievementId != null ? Guid.Parse(command.AchievementId) : Guid.Empty;
                    entity.Status = command.Status;
                    entity.CreatedBy = command.CreatedBy;
                    entity.EditedOn = DateTime.Now;
                    entity.Points = command.Points;
                    var coverPicture = _queryExecutor.Execute<SyncDocumentCoverPictureQuery, Upload>(new SyncDocumentCoverPictureQuery
                    {
                        ExistingUploadId = command.CoverPicture?.Id,
                        ModelId = command.CoverPicture?.Id
                    });
                    entity.CoverPicture = coverPicture != null ? coverPicture.Name : null;
                    _courseRepository.SaveChanges();

                    foreach (var doc in command.Documents)
                    {
                        _commandDispatcher.Dispatch(new AssignDocumentToCourseCommand
                        {
                            Id = command.Id,
                            CourseId = entity.Id,
                            DocumentId = Guid.Parse(doc.Id),
                            Title = doc.Title,
                            Type = doc.Type,
                            OrderNo = doc.OrderNo

                        });
                    }

                    //foreach (var user in command.AssignedUsers)
                    //{
                    //    _commandDispatcher.Dispatch(new AssignCourseToUsersCommand
                    //    {
                    //        Id = command.Id,
                    //        UserId = Guid.Parse(user.Replace(";null", "")),
                    //        CourseId = entity.Id,
                    //        AssignedBy = command.CreatedBy,
                    //        //AdditionalMsg = command.AdditionalMsg,
                    //        AssignedDate = DateTime.Now,
                    //        Deleted = false,
                    //        IsRecurring = false
                    //    });
                    //}
                }
           
            
            
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        private void AddDocumentInAssignedDocument(NewDocument document, AddOrUpdateCourseCommand Coursecommand)
        {

            foreach (var user in Coursecommand.AssignedUsers)
            {
                _assignedDcoumentRepository.Add(new AssignedDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    AssignedBy = Coursecommand.AllocatedAdmins.ToString(),
                    AssignedDate = DateTime.UtcNow,
                    DocumentId = document.Id.ToString(),
                    DocumentType = document.Type,
                    UserId = user.Replace(";null", ""),
                    Deleted = false,
                    IsRecurring = false
                });
                _assignedDcoumentRepository.SaveChanges();
            }

        }
        public CommandResponse Execute(AssignCourseToUsersCommand command)
        {
            if (command.Id == Guid.Empty)
            {
                //add
                var entry = new AssignedCourse();

                entry.Id = Guid.NewGuid();
                entry.UserId = command.UserId;
                entry.CourseId = command.CourseId;
                //entry.DocumentType = command.DocumentType;
                entry.AssignedBy = command.AssignedBy;
                entry.AdditionalMsg = command.AdditionalMsg;
                entry.AssignedDate = DateTime.Now;
                entry.Deleted = false;
                entry.IsRecurring = command.IsRecurring;
                //entry.OrderNumber = command.OrderNumber;

                _assignedCourseRepository.Add(entry);
            }
            else
            {
                //update
                var entry = _assignedCourseRepository.Find(command.Id);

                entry.UserId = command.UserId;
                entry.CourseId = command.CourseId;
                //entry.DocumentType = command.DocumentType;
                entry.AssignedBy = command.AssignedBy;
                entry.AdditionalMsg = command.AdditionalMsg;
                entry.Deleted = false;
                entry.IsRecurring = command.IsRecurring;
                //entry.OrderNumber = command.OrderNumber;
            }

            _assignedCourseRepository.SaveChanges();

            return null;
        }

        public CommandResponse Execute(AssignDocumentToCourseCommand command)
        {
            var entry = new AssociatedDocument();
            var assignedDocumentEntry = new AssignedDocument();

            if (command.Id == Guid.Empty)
            {
                //add
                entry.Id = Guid.NewGuid();
                entry.CourseId = command.CourseId;
                entry.DocumentId = command.DocumentId;
                entry.Type = command.Type;
                entry.Title = command.Title;

                _associatedDcoumentRepository.Add(entry);
            }
            else
            {
                //update
                var enrtyU = _associatedDcoumentRepository.List.Where(d => d.CourseId == command.CourseId && d.DocumentId == command.DocumentId).FirstOrDefault();
                var isNew = (enrtyU == null);
                enrtyU = enrtyU == null ? new AssociatedDocument() : enrtyU;

                enrtyU.CourseId = command.CourseId;
                enrtyU.DocumentId = command.DocumentId;
                enrtyU.Type = command.Type;
                enrtyU.Title = command.Title;
                if (isNew)
                {
                    enrtyU.Id = Guid.NewGuid();
                    _associatedDcoumentRepository.Add(enrtyU);
                }
            }

            _associatedDcoumentRepository.SaveChanges();
           

            return null;
        }
   
        public CommandResponse Execute(DeleteByIdCommand<Course> command)
        {
            var e = _courseRepository.List.Where(z => z.Id == Guid.Parse(command.Id)).FirstOrDefault();
            e.IsDeleted = true;
            _courseRepository.SaveChanges();
            return null;
        }

        public IEnumerable<IValidationResult> Validate(DeleteByIdCommand<Course> command)
        {
            if (string.IsNullOrWhiteSpace(command.Id))
                yield return new ValidationResult(nameof(command.Id), "Id is required");
        }
    }
}
