using Common.Data;
using Common.Query;
using Common.Report;
using Ramp.Contracts.QueryParameter.Reporting;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;

namespace Ramp.Services.QueryHandler.User
{
    public class CustomerCompanyUserExportQueryHandler : ReportingQueryHandler<CustomerCompanyUserExportQuery>
    {
        private readonly IRepository<Domain.Customer.Models.StandardUser> _repository;
        private readonly IRepository<Domain.Models.RaceCodes> _raceCodeRepository;
        private readonly IRepository<Domain.Customer.Models.CustomerGroup> _groupRepository;
        private readonly IRepository<Domain.Customer.Models.Groups.StandardUserGroup> _standardUserGroupRepository;
        public CustomerCompanyUserExportQueryHandler(IRepository<Domain.Customer.Models.StandardUser> repository,
            IRepository<Domain.Models.RaceCodes> raceCodeRepository,
            IRepository<Domain.Customer.Models.CustomerGroup> groupRepository,
            IRepository<Domain.Customer.Models.Groups.StandardUserGroup> standardUserGroupRepository)
        {
            _repository = repository;
            _raceCodeRepository = raceCodeRepository;
            _groupRepository = groupRepository;
            _standardUserGroupRepository = standardUserGroupRepository;
        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitent, CustomerCompanyUserExportQuery data)
        {
            title = $"{data.PortalContext.UserCompany.CompanyName}-UserExport";
            recepitent = string.Empty;
            var section = CreateSection("", PageOrientation.Landscape);
            section.AddElement(CreateUserGrid(GetUsers()));
            document.AddElement(section);
        }
        private GridBlock CreateUserGrid(IEnumerable<UserViewModel> users)
        {
            var grid = CreateGrid();
            CreateTableHeader(grid,
                new Tuple<string, int>("Full Names", 20),
                new Tuple<string, int>("Email Address", 20),
                new Tuple<string, int>("Employee No", 20),
                new Tuple<string, int>("Gender", 20),
                new Tuple<string, int>("Group", 20),
                new Tuple<string, int>("ID Number", 20),
                new Tuple<string, int>("Is From Self Signup", 20),
                new Tuple<string, int>("Mobile No", 20),
                new Tuple<string, int>("Race", 20),
                new Tuple<string, int>("Status", 20)
                );
            users.ToList().ForEach(u =>
            {
                for (var i = 0; i < u.CustomerRoles.Count(); i++)
                {
                    if (i == 0)
                        CreateTableDataRow(grid, u.FullName, u.EmailAddress, u.EmployeeNo, u.Gender, u.GroupName, u.IDNumber, u.IsFromSelfSignUp, u.MobileNumber, u.RaceCodes.FirstOrDefault()?.Description, u.IsActive ? "Active":"Inactive");
                    else
                        CreateTableDataRow(grid, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                }
            });
            return grid;
        }
        private IEnumerable<UserViewModel> GetUsers()
        {

            var users = new List<UserViewModel>();
            _repository.List.ToList().ForEach(u =>
            {
                var model = Project.UserViewModelFrom(u);
                if (model.CustomerRoles.Any())
                    model.CustomerRoles = model.CustomerRoles.OrderBy(x => x).ToList();
                if (model.RaceCodeId.HasValue)
                {
                    var rc = _raceCodeRepository.Find(model.RaceCodeId);
                    if (rc != null)
                        model.RaceCodes.Add(new RaceCodeViewModel
                        {
                            Code = rc.Code,
                            Description = rc.Description,
                            Id = rc.Id
                        });
                }
                var groupedData = _standardUserGroupRepository.List.Where(z => z.UserId == u.Id).ToList();
                List<string> grpname = new List<string>();
                foreach (var g in groupedData) { 
                  var group = _groupRepository.Find(g.GroupId);
                    if (group != null)
                    {
                        grpname.Add(group.Title);
                    }
                }
                if (grpname.Count()>0)
                {
                        model.GroupName = string.Join(",", grpname);
                }
                users.Add(model);
            });
            return users.OrderBy(x => x.FullName).ToList();
        }
    }
}
