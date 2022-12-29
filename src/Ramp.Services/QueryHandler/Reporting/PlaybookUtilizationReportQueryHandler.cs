using Common.Data;
using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
using Ramp.Contracts.QueryParameter.Reporting;
using Common;
using Ramp.Services.Projection;
using VirtuaCon.Reporting;
using Common.Query;
using VirtuaCon.Reporting.Styles;
using System.Drawing;

namespace Ramp.Services.QueryHandler.Reporting
{
    public class PlaybookUtilizationReportQueryHandler : ReportingQueryHandler<PlaybookUtilizationReportQuery>, IQueryHandler<PlaybookUtilizationReportQuery, PlaybookUtilizationReportModel>
    {
        private readonly IRepository<StandardUserCorrespondanceLog> _correspondanceRepository;
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<TrainingTestUsageStats> _testStatsRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IQueryExecutor _executor;
        public PlaybookUtilizationReportQueryHandler(IRepository<StandardUserCorrespondanceLog> correspondanceRepository,
                                                  IRepository<TrainingGuide> guideRepository,
                                                  IRepository<TrainingTestUsageStats> testStatsRepository,
                                                  IRepository<TestResult> testResultRepository,
                                                  IQueryExecutor executor)
        {
            _correspondanceRepository = correspondanceRepository;
            _guideRepository = guideRepository;
            _testStatsRepository = testStatsRepository;
            _testResultRepository = testResultRepository;
            _executor = executor;
        }
        PlaybookUtilizationReportModel IQueryHandler<PlaybookUtilizationReportQuery, PlaybookUtilizationReportModel>.ExecuteQuery(PlaybookUtilizationReportQuery query)
        {
            if (!query.GuideId.HasValue)
                return new PlaybookUtilizationReportModel();
            if (query.EffectiveDateFrom.HasValue)
                query.EffectiveDateFrom = query.EffectiveDateFrom.Value.AtBeginningOfDay();
            if (query.EffectiveDateTo.HasValue)
                query.EffectiveDateTo = query.EffectiveDateTo.Value.AtEndOfDay();
            var guide = _guideRepository.Find(query.GuideId);
            if (guide == null)
                throw new ArgumentException();
            var tests = guide.TestVersion.Versions.Select(x => new { TestId = x.Id, Version = x.Version, Title = x.TestTitle, Created = x.CreateDate });

            var correspondace = new List<StandardUserCorrespondanceLog>();
            if (query.EffectiveDateTo.HasValue)
                tests = tests.Where(x => x.Created <= query.EffectiveDateTo.Value);
            if (query.EffectiveDateFrom.HasValue)
                tests = tests.Where(x => x.Created >= query.EffectiveDateFrom);

            tests.Select(x => x.TestId).ToList().ForEach(delegate (Guid id)
            {
                var match = $"TakeTrainingTest?trainingTestId={id}";
                var c = _correspondanceRepository.List.AsQueryable().Where(x => x.CorrespondenceType == StandardUserCorrespondenceEnum.Email && x.Content.Contains(match));
                if (query.EffectiveDateTo.HasValue)
                    c = c.Where(x => x.CorrespondenceDate <= query.EffectiveDateTo.Value);
                if (query.EffectiveDateFrom.HasValue)
                    c = c.Where(x => x.CorrespondenceDate >= query.EffectiveDateFrom.Value);
                if (query.UserId.HasValue)
                    c = c.Where(x => x.UserId == query.UserId.Value);
                correspondace.AddRange(c.ToList());
            });
            var result = new PlaybookUtilizationReportModel();
            result.GuideTitle = guide.Title;
            correspondace = correspondace.Where(x => x.User != null && x.User.Group != null).ToList();
            if (query.GroupId.HasValue)
                correspondace = correspondace.Where(x => x.User.Group.Id == query.GroupId.Value).ToList();
            var userAssigned = correspondace.GroupBy(x => x.User).ToList();
            userAssigned.ToList().ForEach(delegate (IGrouping<StandardUser, StandardUserCorrespondanceLog> user_logs)
            {
                var entry = result.Entries.FirstOrDefault(x => x.GroupName == user_logs.Key.Group.Title);
                if (entry == null)
                {
                    entry = new PlaybookUtilizationReportModel.Entry { GroupName = user_logs.Key.Group.Title };
                    (result.Entries as List<PlaybookUtilizationReportModel.Entry>).Add(entry);
                }
                user_logs.ToList().ForEach(delegate (StandardUserCorrespondanceLog log) {
                    var userProjection = Project.UserViewModelFrom(user_logs.Key);
                    var testId = log.Content.Substring(log.Content.IndexOf("TakeTrainingTest?trainingTestId="));
                    testId = testId.Substring("TakeTrainingTest?trainingTestId=".Length, $"{Guid.Empty}".Length);
                    Guid id = Guid.Empty;
                    if (Guid.TryParse(testId, out id))
                    {
                        var entryDetails = new PlaybookUtilizationReportModel.Entry.EntryDetail
                        {
                            FullName = userProjection.FullName,
                            EmployeeNumber = userProjection.EmployeeNo,
                            IDNumber = userProjection.IDNumber,
                            Interacted = HasInteracted(user_logs.Key.Id, id, query.EffectiveDateTo, query.EffectiveDateFrom),
                            TestVersion = GetVersion(guide.TestVersion, id)
                        };
                        entryDetails.YetToInteract = !entryDetails.Interacted;
                        bool passed = false;
                        var testResult = HasTestResult(user_logs.Key.Id, id, query.EffectiveDateTo, query.EffectiveDateFrom, out passed);
                        if (passed)
                            entryDetails.PassedTest = testResult;
                        else
                        {
                            entryDetails.FailedTest = testResult;
                        }
                        (entry.Details as List<PlaybookUtilizationReportModel.Entry.EntryDetail>).Add(entryDetails);
                    }
                });
                entry.Details = entry.Details.OrderBy(x => x.FullName).ThenBy(x => x.TestVersion).ToList();
            });
            result.Entries = result.Entries.OrderBy(x => x.GroupName).ToList();
            return result;
        }
        private bool HasInteracted(Guid userId, Guid testId, DateTime? effectiveDateTo, DateTime? effectiveDateFrom)
        {
            var stats = _testStatsRepository.List.AsQueryable().Where(x => !x.Unassigned && x.UserId == userId && x.TrainingTestId == testId);
            if (effectiveDateFrom.HasValue)
                stats = stats.Where(x => x.DateTaken >= effectiveDateFrom.Value);
            if (effectiveDateTo.HasValue)
                stats = stats.Where(x => x.DateTaken <= effectiveDateTo.Value);
            return stats.Any();
        }
        private decimal HasTestResult(Guid userId, Guid testId, DateTime? effectiveDateTo, DateTime? effectiveDateFrom, out bool passed)
        {
            var r = _testResultRepository.List.AsQueryable().Where(x => x.TrainingTestId == testId && x.TestTakenByUserId == userId);
            if (effectiveDateTo.HasValue)
                r = r.Where(x => x.TestDate <= effectiveDateTo.Value);
            if (effectiveDateFrom.HasValue)
                r = r.Where(x => x.TestDate >= effectiveDateFrom.Value);
            if (r.Any())
            {
                var p = r.FirstOrDefault(x => x.TestResultStatus);
                passed = p != null;
                if (p != null)
                    return Math.Round(decimal.Parse(p.TestScore.ToString()) / decimal.Parse(p.Total.ToString()) * 100, 2);
                else
                {
                    var results = new List<decimal>();
                    r.ToList().ForEach(x => results.Add(decimal.Parse(x.TestScore.ToString()) / decimal.Parse(x.Total.ToString()) * 100));
                    return Math.Round(results.Average(), 2);
                }
            }
            passed = false;
            return 0M;
        }
        private int GetVersion(TestVersion versions, Guid testId)
        {
            var t = versions.Versions.FirstOrDefault(x => x.Id == testId);
            if (t != null && t.Version.HasValue)
                return t.Version.Value;
            return -1;
        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitent, PlaybookUtilizationReportQuery data)
        {
            var model = _executor.Execute<PlaybookUtilizationReportQuery, PlaybookUtilizationReportModel>(data);
            title = model.GuideTitle;
            recepitent = string.Empty;
            var section = CreateSection("", PageOrientation.Portrait);
            IDictionary<string, GridBlock> groups;
            section.AddElement(GenerateFlat(model.GuideTitle, model.Entries, out groups));
            document.AddElement(section);
            if (data.Excel)
            {
                groups.Keys.ToList().ForEach(delegate (string group)
                {
                    section = CreateSection(group);
                    section.AddElement(groups[group]);
                    document.AddElement(section);
                });
            }
        }
        private GridBlock GenerateFlat(string guideTitle, IEnumerable<PlaybookUtilizationReportModel.Entry> entries, out IDictionary<string, GridBlock> groups)
        {
            var grid = CreateGrid();
            var columunHeadingsWithWidths = new List<Tuple<string, int>>()
            {
                new Tuple<string, int>("Full Name",20),
                new Tuple<string, int>("Employee Number",10),
                new Tuple<string, int>("ID Number",20),
                new Tuple<string, int>("Version",10),
                new Tuple<string, int>("Interacted",10),
                new Tuple<string, int>("Yet To Interact",10),
                new Tuple<string, int>("Passed",10),
                new Tuple<string, int>("Failed",10)
            };
            InsertHeader(grid, guideTitle, columunHeadingsWithWidths.ToArray());
            var groupsScoped = new Dictionary<string, GridBlock>();

            entries.GroupBy(x => x.GroupName).ToList().ForEach(delegate (IGrouping<string, PlaybookUtilizationReportModel.Entry> group)
            {
                var headingRow = CreateGroupHeadingRow(group.Key, 8);
                grid.AddElement(headingRow);
                if (!groupsScoped.ContainsKey(group.Key))
                {
                    var scopedGrid = new GridBlock();
                    InsertHeader(scopedGrid, guideTitle, columunHeadingsWithWidths.ToArray());
                    groupsScoped.Add(group.Key, scopedGrid);
                    groupsScoped[group.Key].AddElement(headingRow);
                }
                group.SelectMany(x => x.Details).ToList().ForEach(delegate (PlaybookUtilizationReportModel.Entry.EntryDetail detail)
                {
                    var row = CreateTableDataRowWithStyles(grid, new[] { LeftAligned as ElementStyle, largeFont },
                             detail.FullName,
                             detail.EmployeeNumber,
                             detail.IDNumber,
                             detail.TestVersion.ToString(),
                             detail.Interacted ? "Yes" : "No",
                             detail.YetToInteract ? "Yes" : "No",
                             detail.PassedTest.HasValue ? detail.PassedTest.Value.ToString() : "No",
                             detail.FailedTest.HasValue && !detail.YetToInteract ? detail.FailedTest.Value.ToString() : "No");
                    groupsScoped[group.Key].AddElement(row);
                });
            });
            groups = groupsScoped;
            return grid;
        }
        private void InsertHeader(GridBlock grid, string guideTitle, params Tuple<string, int>[] columunHeadingsWithWidths)
        {
            CreateHeader(guideTitle, columunHeadingsWithWidths.Count(), columunHeadingsWithWidths.Select(x => x.Item1).ToArray()).ToList().ForEach(delegate (GridRowBlock row) {
                grid.AddElement(row);
            });
            grid.ColumnWidths.AddRange(columunHeadingsWithWidths.Select(x => x.Item2).ToArray());
        }
        private GridRowBlock CreateGroupHeadingRow(string title, int colspan)
        {
            var headingRow = new GridRowBlock();
            var dataCell = new GridCellBlock($"Group Name : {title}");
            dataCell.ColumnSpan = colspan;
            ApplyStyles(headingRow, true, new ForeColorElementStyle(Color.Black), new BackgroundColorElementStyle(Color.LightGray), largeFontBold, Centered);
            headingRow.AddElement(dataCell);
            return headingRow;
        }
        private IEnumerable<GridRowBlock> CreateHeader(string guideTitle, int colspan, params string[] headings)
        {
            var rows = new List<GridRowBlock>();
            rows.Add(CreateGuidHeading(guideTitle, colspan));
            rows.Add(CreateTableColumnHeadings(headings));
            return rows;
        }
        private GridRowBlock CreateGuidHeading(string title, int colspan)
        {
            var row = new GridRowBlock();
            ApplyStyles(row, true, new ForeColorElementStyle(Color.Black), new BackgroundColorElementStyle(Color.DarkGray), largeFontBold, Centered);
            var cell = new GridCellBlock($"Playbook Name : {title}");
            cell.ColumnSpan = colspan;
            row.AddElement(cell);
            return row;
        }
        private GridRowBlock CreateTableColumnHeadings(params string[] headings)
        {
            var row = new GridRowBlock();
            ApplyStyles(row, true, new ForeColorElementStyle(Color.Black), new BackgroundColorElementStyle(Color.Gray), largeFontBold, Centered);
            headings.ToList().ForEach(x => row.AddCell(x));
            return row;
        }
        private void ApplyStyles(GridRowBlock row, bool defaultChildStyles = true, params ElementStyle[] styles)
        {
            styles.ToList().ForEach(delegate (ElementStyle style) {
                if (defaultChildStyles)
                    row.AddDefaultChildStyle(style);
                else
                    row.AddStyle(style);
            });
        }

    }
}
