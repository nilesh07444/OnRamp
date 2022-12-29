using Common.Data;
using Common.Query;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.Query.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

using StandardUserGroup = Domain.Customer.Models.Groups.StandardUserGroup;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class UserActivityAndPerformanceUserListReportQueryHandler : ReportingQueryHandler<UserActivityAndPerformanceUserListReportQuery>,
                                                                        IQueryHandler<UserActivityAndPerformanceUserListReportQuery, IEnumerable<UserViewModel>>
    {
        private readonly IQueryHandler<StandardUsersQuery, IEnumerable<UserViewModel>> _userQueryHandler;

        private readonly IRepository<StandardUserGroup> _standardUserGroupRepository;

        public UserActivityAndPerformanceUserListReportQueryHandler(
            IRepository<StandardUserGroup> standardUserGroupRepository, IQueryHandler<StandardUsersQuery, IEnumerable<UserViewModel>> userQueryHandler)
        {
            _userQueryHandler = userQueryHandler;
            _standardUserGroupRepository = standardUserGroupRepository;
        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitent, UserActivityAndPerformanceUserListReportQuery data)
        {
            title = "User Actitvity and Performance Report User Details Report";
            recepitent = string.Empty;
            var users = GetUsers(data);
            var section = CreateSection(title);
            var grid = CreateGrid();

            var fields = new List<Tuple<string, Func<UserViewModel, string>>>();
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
               "Employee Number", model => model.EmployeeNo));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
                "Name", model => model.FullName));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
               "Email", model => model.EmailAddress));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
              "ID Number", model => model.IDNumber));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
             "Gender", model => model.Gender));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
                "Contact Number", model => model.ContactNumber));
            fields.Add(new Tuple<string, Func<UserViewModel, string>>(
               "Report Link", model => $"{data.UriBase}?UserId={model.Id}&FromDate={data.FromDate}&ToDate={data.ToDate}"));

            CreateTableDataRowWithStyles(grid, HeaderStyle, fields.Select(x => x.Item1).ToArray());
            grid.ColumnWidths.AddRange(fields.Select(x => 20).ToArray());

            foreach (var user in users)
            {
                var row = new GridRowBlock();
                foreach (var field in fields)
                {
                    row.AddElement(new GridCellBlock(field.Item2(user)));
                }
                grid.AddElement(row);
            }

            section.AddElement(grid);
            document.AddElement(section);
        }
        private IEnumerable<UserViewModel> GetUsers(UserActivityAndPerformanceUserListReportQuery query)
        {
            //if (query.UserIds != null || query.GroupIds != null)
            //if ((query.UserIds != null && query.UserIds.Any()) || (query.GroupIds != null && query.GroupIds.Any()))
            //{
            var q = new StandardUsersQuery();
            if (query.UserIds != null && query.UserIds.Any())
                q.Ids = query.UserIds;
            //if (query.GroupIds != null && query.GroupIds.Any())
            //	q.GroupIds = query.GroupIds;
            //if (query.Tags != null && query.Tags.Any())
            // q.TagNames = query.Tags;
            var tags = string.Join(",", query.Tags);
            var rr = _userQueryHandler.ExecuteQuery(q).Where(x => !string.IsNullOrWhiteSpace(x.FullName) && tags.Contains(x.TrainingLabels)).ToList();
            List<UserViewModel> userMod=new List<UserViewModel>() ;
            if (query.GroupIds != null && query.GroupIds.Any() && rr.Count > 0)
            {
                foreach (var u in rr)
                {

                    var groupList = _standardUserGroupRepository.List.Where(c => c.UserId.ToString() == u.Id.ToString()).ToList();

                    string name = null;
                    if (groupList.Count > 0)
                    {
                        foreach (var g in groupList)
                        {
                            if (name != null)
                                name = name + "," + g.GroupId;
                            else name = name + g.GroupId;
                        }
                        //var x = String.Join(",", query.GroupId);
                        //if (x.Contains(name))
                        //{
                        //	userIds.Add(u.ToString());
                        //}
                        u.GroupName = name;
                    }
                }
                var gg = String.Join(",", query.GroupIds);
                foreach (var g in query.GroupIds)
                {
                    userMod.AddRange(rr.Where(r => r.GroupName.Contains(g)).ToList());
                }


                //rr = rr.Where(r => !string.IsNullOrEmpty(r.GroupName)? r.GroupName.Contains(gg):false).ToList();
            }
            else { 
                userMod.AddRange(rr.ToList());
            }
            return userMod;
            //} 
            //        return Enumerable.Empty<UserViewModel>();
        }
        IEnumerable<UserViewModel> IQueryHandler<UserActivityAndPerformanceUserListReportQuery, IEnumerable<UserViewModel>>.ExecuteQuery(UserActivityAndPerformanceUserListReportQuery query)
        {
            return GetUsers(query);
        }
    }
}
