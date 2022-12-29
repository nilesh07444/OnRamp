using Common.Command;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ramp.Contracts.CommandParameter.CustomerManagement
{
    public class SaveCsvCustomerCompanyUserCommand : ICommand
    {
        public SaveCsvCustomerCompanyUserCommand()
        {
            UserRoles = new List<string>();
        }

        public HttpPostedFileBase CsvHttpPostedFile { get; set; }
        public List<string> UserRoles { get; set; }
        public string CsvFilePath { get; set; }
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string LastName { get; set; }
        public Guid ParentUserId { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
    }
}