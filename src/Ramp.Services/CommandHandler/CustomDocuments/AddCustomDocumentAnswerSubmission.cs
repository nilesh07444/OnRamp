using Common;
using Common.Command;
using Common.Data;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.DocumentTrack;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Enums;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.CommandParameter.CustomDocument;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtuaCon;

namespace Ramp.Services.CommandHandler.CustomDocuments
{
    public class AddCustomDocumentAnswerSubmission : CommandHandlerBase<CustomDocumentAnswerSubmission>
    {
        private readonly IRepository<CustomDocumentAnswerSubmission> _customDocumentanswerRepository;

        public AddCustomDocumentAnswerSubmission(IRepository<CustomDocumentAnswerSubmission> customDocumentanswerRepository)
        {
            _customDocumentanswerRepository = customDocumentanswerRepository;
        }

        public override CommandResponse Execute(CustomDocumentAnswerSubmission command)
        {
            var response = new CommandResponse();
            try
            {
                if (command.TestQuestionID != Guid.Empty && !string.IsNullOrEmpty(command.TestSelectedAnswer))
                {
                    //var isNewEntry = _customDocumentanswerRepository.GetAll();
                    var entryCustomDocumentanswer = _customDocumentanswerRepository.GetAll().Where(z => z.CustomDocumentID == Guid.Parse(command.CustomDocumentID.ToString()) && z.TestQuestionID == command.TestQuestionID && z.StandarduserID == command.StandarduserID).FirstOrDefault();
                    var isNewEntry = entryCustomDocumentanswer == null;

                    if (!isNewEntry)
                    {
                        entryCustomDocumentanswer.CreatedOn = DateTime.Now;
                        entryCustomDocumentanswer.TestSelectedAnswer = command.TestSelectedAnswer;
                    }
                    else
                    {
                        _customDocumentanswerRepository.Add(command);
                    }
                    _customDocumentanswerRepository.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.Id = command.Id.ToString();
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
    }
}
