using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using Domain.Enums;
using Domain.Models;
using LINQtoCSV;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Domain.Enums.GenderEnum;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class SaveCsvCustomerCompanyUserCommandHandler : ICommandHandlerBase<SaveCsvCustomerCompanyUserCommand>
    {
        private readonly IRepository<StandardUser> _standardUserRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RaceCodes> _raceCodeRepository;
		private readonly IRepository<CustomerGroup> _groupRepository;

		public SaveCsvCustomerCompanyUserCommandHandler(IRepository<StandardUser> standardUserRepository,
            IRepository<User> userRepository, IRepository<RaceCodes> raceCodeRepository, IRepository<CustomerGroup> groupRepository)
        {
            _standardUserRepository = standardUserRepository;
            _userRepository = userRepository;
            _raceCodeRepository = raceCodeRepository;
			_groupRepository = groupRepository;
		}

        public CommandResponse Execute(SaveCsvCustomerCompanyUserCommand command)
        {
            var resellerEmails = _userRepository.List.Where(u => u.CompanyId.Equals(command.PortalContext.UserCompany.ProvisionalAccountLink)).Select(x => x.EmailAddress);

            var path = command.CsvFilePath;
            var filePath = Path.Combine(path,
                command.CsvHttpPostedFile.FileName);
            var descriptor = new CsvFileDescription { SeparatorChar = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator[0], FirstLineHasColumnNames = true };
            var context = new CsvContext();
            var users = context.Read<CSVMasks.User>(filePath, descriptor);
            if (command.UserRoles.Contains(Contracts.Security.Role.Admin) ||
                        command.UserRoles.Contains(Contracts.Security.Role.Reseller) ||
                        command.UserRoles.Contains(Contracts.Security.Role.UserAdmin) ||
                        command.UserRoles.Contains(Contracts.Security.Role.CustomerAdmin))
            {
                foreach (var u in users)
                {
					#region
					//below code added by neeraj
					if (u.Group != null && u.Group != "") {
						var userGroups = u.Group.ToLower();
						//var userGroups = u.Group.Split(',');

						var temp = _groupRepository.List.Select(n => n.Title).ToList();
						var allGroups = String.Join(",", temp);

						var groupCheck = allGroups.ToLower().Contains(userGroups);

						//if (groupCheck == false) throw new InvalidDataException("group does not exist");
						if (groupCheck == false) return null;
					}
					#endregion

					if (!string.IsNullOrWhiteSpace(u.MobileNumber) && u.MobileNumber.Length == 9)
                        u.MobileNumber = string.Join(string.Empty, "0", u.MobileNumber);

                    if (!_standardUserRepository.List.Any(x => x.EmailAddress.Equals(u.EmailAddress.TrimAllCastToLowerInvariant()))
                        && !resellerEmails.Contains(u.EmailAddress.TrimAllCastToLowerInvariant())
                        && !u.EmailAddress.TrimAllCastToLowerInvariant().Equals("admin@onramp.co.za"))
                    {
                        var customerCompanyUserByCustomerAdminCommandCsv = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
                        {
                            FirstName = u.FullName.Trim().Replace("\"", string.Empty),
                            IDNumber = u.IDNumber,
                            EmailAddress = u.EmailAddress.TrimAllCastToLowerInvariant(),
                            Password = u.Password,
                            MobileNumber = u.MobileNumber,
                            GroupName = u.Group,
                            UserId = command.UserId,
                            CompanyId = command.CompanyId,
                            LastName = command.LastName,
                            ParentUserId = command.ParentUserId,
                            EmployeeNo = u.EmployeeNo,
                        };
                        if (!string.IsNullOrWhiteSpace(u.Gender))
                        {
                            if (u.Gender.Equals("m", StringComparison.InvariantCultureIgnoreCase))
                                u.Gender = Gender.Male.ToString();
                            if (u.Gender.Equals("f", StringComparison.InvariantCultureIgnoreCase))
                                u.Gender = Gender.Female.ToString();
                            customerCompanyUserByCustomerAdminCommandCsv.Gender = Enum.Parse(typeof(Gender), u.Gender, true).ToString();
                        }
						else
                        {
                            customerCompanyUserByCustomerAdminCommandCsv.Gender = Gender.NotSpecified.ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(u.RaceCode))
                        {
                            var raceCodeEnitity = _raceCodeRepository.List.SingleOrDefault(
                                x => x.Code.TrimAllCastToLowerInvariant().Equals(u.RaceCode.TrimAllCastToLowerInvariant())
                            || x.Description.TrimAllCastToLowerInvariant().Equals(u.RaceCode.TrimAllCastToLowerInvariant()));

                            customerCompanyUserByCustomerAdminCommandCsv.RaceCodeId = raceCodeEnitity.Id;
                        }

                        customerCompanyUserByCustomerAdminCommandCsv.Roles.Add(Contracts.Security.Role.StandardUser);
                        new CommandDispatcher().Dispatch(customerCompanyUserByCustomerAdminCommandCsv);
                    }
                }
            }
            return null;
        }
    }
}