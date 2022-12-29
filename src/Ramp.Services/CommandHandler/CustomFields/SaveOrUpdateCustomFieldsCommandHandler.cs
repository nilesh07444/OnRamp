using Common.Command;
using Common.Data;
using Domain.Customer.Models.Custom_Fields;
using Ramp.Contracts.CommandParameter.CustomFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomFields
{
    class SaveOrUpdateCustomFieldsCommandHandler : CommandHandlerBase<SaveOrUpdateCustomFieldCommand>
    {
        private readonly IRepository<Fields> _field;
        public SaveOrUpdateCustomFieldsCommandHandler(IRepository<Fields> field)
        {
            _field = field;
        }
        public override CommandResponse Execute(SaveOrUpdateCustomFieldCommand command)
        {
            var response = new CommandResponse();
            response.ErrorMessage = null;
            response.Id = command.Id.ToString();
            response.Validation = null;

            var data = new Fields();

            try
            {
                if (command.Id == Guid.Empty)
                {
                    data.Id = Guid.NewGuid();
                    data.UserId = command.UserId;
                    data.FieldName = command.FieldName;
                    data.FieldType = command.Type;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    data.DateCreated = DateTime.Now;

                    _field.Add(data);
                    _field.SaveChanges();

                    response.Id = data.Id.ToString();
                }
                else if (command.Id != Guid.Empty)
                {
                    var entity = _field.Find(command.Id);

                    entity.Id = Guid.NewGuid();
                    entity.UserId = command.UserId;
                    entity.FieldName = command.FieldName;
                    entity.FieldType = command.Type;
                    entity.IsActive = true;
                    entity.IsDeleted = false;
                    entity.DateCreated = DateTime.Now;
                    _field.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
    }
}
