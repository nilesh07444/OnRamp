using Common.Data;
using Common.Query;
using Domain.Customer.Models.ScheduleReport;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Domain.Customer.Models;
using Ramp.Contracts.ViewModel;
using System.Collections.Generic;
using System.Linq;


namespace Ramp.Services.QueryHandler
{
    public class ScheduleReportQueryHandler :
        IQueryHandler<FetchAllScheduleReportQuery, List<ScheduleReportModel>>,
        IQueryHandler<FetchByIdQuery, ScheduleReportVM>
    {

        private readonly IRepository<ScheduleReportModel> _ScheduleReportList;
        private readonly IRepository<ScheduleReportParameter> _ReportRepository;
        private readonly IRepository<SelectedScheduleReportParameter> _ReportParameterRepository;

        public ScheduleReportQueryHandler(
            IRepository<ScheduleReportModel> ScheduleReportList,
            IRepository<ScheduleReportParameter> ReportRepository,
            IRepository<SelectedScheduleReportParameter> ReportParameterRepository
            )
        {
            _ScheduleReportList = ScheduleReportList;
            _ReportRepository = ReportRepository;
            _ReportParameterRepository = ReportParameterRepository;
        }

        public List<ScheduleReportModel> ExecuteQuery(FetchAllScheduleReportQuery queryParameters)
        {
            var response = new List<ScheduleReportModel>();

            try
            {
                var auto = _ScheduleReportList.List.Where(c => c.IsDeleted == false).Select(x => new ScheduleReportModel
                {
                    Id = x.Id,
                    ScheduleName = x.ScheduleName,
                    DateCreated = x.DateCreated,
                    RecipientsList = x.RecipientsList,
                    Occurences = x.Occurences,
                    ScheduleTime = x.ScheduleTime,
                    ReportAssignedId = x.ReportAssignedId,
                    Status=x.Status                   

                }).ToList();
                var grp = _ReportRepository.List.ToList();
                var docs = _ReportParameterRepository.List.ToList();

                foreach (var a in auto)
                {
                    var data = new ScheduleReportModel();
                    //data.DocumentList = new List<ScheduleReportModel>();
                    List<string> groups = new List<string>();
                    data.Id = a.Id;
                    data.ScheduleName = a.ScheduleName;
                    data.RecipientsList = a.RecipientsList;
                    data.Occurences = a.Occurences;
                    data.ScheduleTime = a.ScheduleTime;
                    data.ReportAssignedId = a.ReportAssignedId;
                    data.DateCreated = a.DateCreated;
                    data.Status = a.Status;
                    //foreach (var g in grp)
                    //{
                    //    if (a.Id.ToString() == g.Id.ToString())
                    //    {
                    //        groups.Add(g.Id.ToString());
                    //    }
                    //}
                    //data.GroupIds = groups.ToArray();
                    //foreach (var d in docs)
                    //{
                    //	if (a.Id.ToString() == d.Id.ToString())
                    //	{
                    //		data.DocumentList.Add(new AutoWorkFlowDocs { Id = d.Id.ToString(), Title = "", Type = d.Type.ToString(), Order = d.Order, AditionalMsg = "" });
                    //	}
                    //}

                    response.Add(data);
                }
            }
            catch (System.Exception ex)
            {

                throw;
            }

            return response;
        }

        public ScheduleReportVM ExecuteQuery(FetchByIdQuery query)
        {
            var data = new ScheduleReportVM();

            try
            {
                var auto = _ScheduleReportList.List.Where(c => c.IsDeleted == false && c.Id == Guid.Parse(query.Id.ToString())).Select(x => new ScheduleReportVM
                {

                    Id = x.Id,
                    ScheduleName = x.ScheduleName,
                    DateCreated = x.DateCreated,
                    RecipientsList = x.RecipientsList,
                    Occurences = x.Occurences,
                    ScheduleTime = x.ScheduleTime,
                    ReportAssignedId = x.ReportAssignedId,
                    Status = x.Status

                }).FirstOrDefault();

                var parametergrp =
                                  (from r in _ReportRepository.List
                                   where r.ReportID == Guid.Parse(query.Id.ToString())
                                   group r by new { r.ParameterType } into g
                                   select new { g.Key.ParameterType, Items = string.Join(",", g.Select(kvp => kvp.ParameterID)) }).ToList();

                var grp = _ReportRepository.List.Where(z => z.ReportID == Guid.Parse(query.Id.ToString()))

                    .ToList();
                var docs = _ReportParameterRepository.List.ToList();


                List<string> groups = new List<string>();
                data.Id = auto.Id;
                data.ScheduleName = auto.ScheduleName;
                data.RecipientsList = auto.RecipientsList;
                data.Occurences = auto.Occurences;
                data.ScheduleTime = auto.ScheduleTime;
                data.ReportAssignedId = auto.ReportAssignedId;
                data.DateCreated = auto.DateCreated;
                data.Status = auto.Status;
                data.Params = new List<Parameters>();
                foreach (var g in parametergrp)
                {
                    data.Params.Add(new Parameters { Name = g.ParameterType, Value = g.Items });
                }
                //data.GroupIds = groups.ToArray();
                //foreach (var d in docs)
                //{
                //	if (data.Id.ToString() == d.WorkFlowId.ToString())
                //	{
                //		data.DocumentList.Add(new AutoWorkFlowDocs { Id = d.Id.ToString(), Title = "", Type = d.Type.ToString(), Order = d.Order, AditionalMsg = "" });
                //	}
                //}

            }
            catch (System.Exception ex)
            {

                throw;
            }

            return data;
        }
    }
}

