using Common.Query;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Customer.Models;
using Ramp.Services.Projection;
using Common;
using Domain.Models;
using VirtuaCon.Reporting;
using Data.EF.Customer;
using Common.Collections;

namespace Ramp.Services.QueryHandler {

	public class TrainingActivityQueryHandler : ReportingQueryHandler<TrainingActivityListQuery>,
												IQueryHandler<TrainingActivityListQuery, IPaged<TrainingActivityListModel>>,
												IQueryHandler<FetchByIdQuery, TrainingActivityModel>,
												IQueryHandler<FetchAllTrainingActivityLogQuery, List<TrainingActivityModel>>,
												IQueryHandler<FilterTrainingActivityLogQuery, List<TrainingActivityModel>>,
												IQueryHandler<FetchAllKnownBursaryTypesQuery, IEnumerable<string>>,
												IQueryHandler<TrainingActivityListQuery, TrainingActivityReportModel> {
		private readonly ITransientReadRepository<StandardUserTrainingActivityLog> _repository;
		private readonly ITransientReadRepository<Label> _labelRepository;
		private readonly IQueryExecutor _executor;

		public TrainingActivityQueryHandler(ITransientReadRepository<StandardUserTrainingActivityLog> repository, ITransientReadRepository<Label> labelRepository, IQueryExecutor executor) {
			_repository = repository;
			_labelRepository = labelRepository;
			_executor = executor;
		}

		public List<TrainingActivityModel> ExecuteQuery(FilterTrainingActivityLogQuery query) {
			var r = _repository.List.AsQueryable();

			var models = r.ToList().AsQueryable().Select(Project.ToTrainingActivityModel).ToList();
			var trainingList = new List<TrainingActivityModel>();

			if (!string.IsNullOrEmpty(query.TrainingLables)) {
				foreach (var item in query.TrainingLables.Split(',')) {
				var	training = models.Where(c => c.TrainingLabels.Contains(item)).ToList();
					trainingList.AddRange(training);
				}
				models = trainingList.GroupBy(c=>c.Id).Select(f=>f.FirstOrDefault()).ToList();
			}
			if (query.FromDate != null && query.FromDate != DateTime.MinValue) {
				models = models.Where(c => c.From.Value.ToLocalTime().Date >= query.FromDate.Date && c.From.Value.ToLocalTime().Date <= query.ToDate.Date).ToList();
			}

			if (query.CostRangeTo > 0 && query.CostRangeFrom > 0) {
				models = models.Where(c => c.CostImplication >= query.CostRangeFrom && c.CostImplication <= query.CostRangeTo).ToList();
			}else if(query.CostRangeTo > 0 && query.CostRangeFrom == 0) {
				models = models.Where(c => c.CostImplication <= query.CostRangeTo).ToList();
			}else if (query.CostRangeFrom > 0 && query.CostRangeTo == 0) {
				models = models.Where(c => c.CostImplication >= query.CostRangeFrom).ToList();
			}
			if (!string.IsNullOrEmpty(query.TrainingType.ToString())) {

				if (!string.IsNullOrEmpty(query.ExternalTrainingProviders) && query.TrainingType == (int)TrainingActivityType.External) {
					var users = query.ExternalTrainingProviders.Split(',').ToList();
					models = models.Where(c => (int)c.TrainingActivityType.Value == (int)TrainingActivityType.External && c.ExternalTrainingActivityDetail.Invoices.Any() && c.ExternalTrainingActivityDetail.Invoices.All(x => users.Contains(x.Id.ToString()))).ToList();
				} else if (string.IsNullOrEmpty(query.ExternalTrainingProviders) && query.TrainingType == (int)TrainingActivityType.External) {
					models = models.Where(c => c.TrainingActivityType.Value == TrainingActivityType.External).ToList();
				}
				if (!string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.Internal) {
					var users = query.Trainers.Split(',').ToList();
					models = models.Where(c => (int)c.TrainingActivityType.Value == (int)TrainingActivityType.Internal && c.InternalTrainingActivityDetail.ConductedBy.Any() && c.InternalTrainingActivityDetail.ConductedBy.All(x => users.Contains(x.Id.ToString()))).ToList();
				} else if (string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.Internal) {
					models = models.Where(c => c.TrainingActivityType.Value == TrainingActivityType.Internal).ToList();
				}
				if (!string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.Bursary) {
					var users = query.Trainers.Split(',').ToList();
					models = models.Where(c => (int)c.TrainingActivityType.Value == (int)TrainingActivityType.Bursary && c.BursaryTrainingActivityDetail.ConductedBy.Any() && c.BursaryTrainingActivityDetail.ConductedBy.All(x => users.Contains(x.Id.ToString()))).ToList();
				} else if (string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.Bursary) {
					models = models.Where(c => c.TrainingActivityType.Value == TrainingActivityType.Bursary).ToList();
				}
				if (!string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.MentoringAndCoaching) {
					var users = query.Trainers.Split(',').ToList();
					models = models.Where(c => (int)c.TrainingActivityType.Value == (int)TrainingActivityType.MentoringAndCoaching && c.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Any() && c.MentoringAndCoachingTrainingActivityDetail.ConductedBy.All(x => users.Contains(x.Id.ToString()))).ToList();
				} else if (string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.MentoringAndCoaching) {
					models = models.Where(c => c.TrainingActivityType.Value == TrainingActivityType.MentoringAndCoaching).ToList();
				}
				if (!string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.ToolboxTalk) {
					var users = query.Trainers.Split(',').ToList();
					models = models.Where(c => (int)c.TrainingActivityType.Value == (int)TrainingActivityType.ToolboxTalk && c.ToolboxTalkTrainingActivityDetail.ConductedBy.Any() && c.ToolboxTalkTrainingActivityDetail.ConductedBy.All(x => users.Contains(x.Id.ToString()))).ToList();
				} else if (string.IsNullOrEmpty(query.Trainers) && query.TrainingType == (int)TrainingActivityType.ToolboxTalk) {
					models = models.Where(c => c.TrainingActivityType.Value == TrainingActivityType.ToolboxTalk).ToList();
				}

			}
			if (!string.IsNullOrEmpty(query.Trainees)) {
				models = models.Where(c => c.UsersTrained.Any() && c.UsersTrained.All(x => x.Id.ToString().Contains(query.Trainees))).ToList();
			}

			return models;
		}


		public TrainingActivityModel ExecuteQuery(FetchByIdQuery query) {
			if (string.IsNullOrWhiteSpace(query.Id?.ToString())) { return new TrainingActivityModel(); }
			var act = _repository.Find(query.Id.ToString().ConvertToGuid());
			var model = act == null ? null : Project.ToTrainingActivityModel.Compile().Invoke(act);
			if (model != null && !string.IsNullOrWhiteSpace(model.TrainingLabels)) {
				var labels = _labelRepository.List.AsQueryable().Where(x => !x.Deleted).Select(x => x.Name).ToList();
				var filtered = model.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(x => labels.Contains(x)).ToArray();
				if (filtered.Any())
					model.TrainingLabels = string.Join(",", filtered);
				else
					model.TrainingLabels = null;
			}
			return model;
		}

		public new IPaged<TrainingActivityListModel> ExecuteQuery(TrainingActivityListQuery query) {
			return query.GetPagedWithoutProjection(_repository.List.AsQueryable().Select(Project.ToTrainingActivityListModel).ToList());
		}

		public IEnumerable<string> ExecuteQuery(FetchAllKnownBursaryTypesQuery query) {
			return _repository.List.AsQueryable().Where(x => x.BursaryTrainingActivityDetail != null && x.BursaryTrainingActivityDetail.BursaryType != null).Select(x => x.BursaryTrainingActivityDetail.BursaryType).ToList();
		}

		private IQueryable<StandardUserTrainingActivityLog> FilterByTrainingActivityType(TrainingActivityListQuery query, IQueryable<StandardUserTrainingActivityLog> List) {
			var types = query.TrainingActivityTypes.Where(x => x.HasValue);
			if (types.Any())
				List = List.Where(x => types.Contains(x.TrainingActivityType));
			return List;
		}
		private List<TrainingActivityModel> FilterByTrainingLables(TrainingActivityListQuery query, List<TrainingActivityModel> List) {
			var labels = !string.IsNullOrWhiteSpace(query.TrainingLabels) ? query.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
			if (labels.Any())
				List = List.AsQueryable().Where(e => e.TrainingLabels != null && e.TrainingLabels.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Any(x => labels.Contains(x))).ToList();
			return List;
		}
		public List<TrainingActivityModel> ExecuteQuery(FetchAllTrainingActivityLogQuery query) {
			var entries = _repository.List.AsQueryable();
			var models = entries.OrderBy(x => x.Title).Select(Project.ToTrainingActivityLogModel).ToList();
			return models;
		}
		private IQueryable<StandardUserTrainingActivityLog> FilterByGroup(TrainingActivityListQuery query, IQueryable<StandardUserTrainingActivityLog> List) {
			var groups = query.Groups.Where(x => x.Id != Guid.Empty).Select(x => x.Id);
			if (groups.Any())
				List = List.Where(e => e.UsersTrained.Any(x => x.Group != null && groups.Contains(x.Group.Id)));
			return List;
		}
		private IQueryable<StandardUserTrainingActivityLog> FilterByRace(TrainingActivityListQuery query, IQueryable<StandardUserTrainingActivityLog> List) {
			var races = query.RaceCodes.Where(x => x.Id != Guid.Empty).Select(x => x.Id);
			if (races.Any())
				List = List.Where(e => e.UsersTrained.Any(x => x.RaceCodeId.HasValue && x.RaceCodeId.Value != Guid.Empty && races.Contains(x.RaceCodeId.Value)));
			return List;
		}
		private IQueryable<StandardUserTrainingActivityLog> FilterByUsersTrained(TrainingActivityListQuery query, IQueryable<StandardUserTrainingActivityLog> List) {
			var users = query.UsersTrained.Where(x => x.Id != Guid.Empty).Select(x => x.Id);
			if (users.Any())
				List = List.Where(e => e.UsersTrained.Any(x => users.Contains(x.Id)));
			return List;
		}
		public override void BuildReport(ReportDocument document, out string title,out string recepitent, TrainingActivityListQuery query) {
			title = "TrainingActivityReport";
			recepitent = string.Empty;
			var data = _executor.Execute<TrainingActivityListQuery, TrainingActivityReportModel>(query);

			var columns = new List<Tuple<string, int>>
			{
				new Tuple<string, int>("Title",300),
				new Tuple<string, int>("Description",300),
				new Tuple<string, int>("From",90),
				new Tuple<string, int>("To",90),
				new Tuple<string, int>("Type",90),
				new Tuple<string, int>("Cost Implication",90),
				new Tuple<string, int>("Venue",90),
				new Tuple<string, int>("Reward Points",90),
				new Tuple<string, int>("Training Labels",90),
				new Tuple<string, int>("Bursary Type",90),
				new Tuple<string, int>("Additional Info",300),
				new Tuple<string, int>("Trainees",90),
				new Tuple<string, int>("Trainers",90)
			};
			var section = CreateSection(title);
			var grid = CreateGrid();
			CreateTableHeader(grid, columns.ToArray());
			data.FilteredResults.ToList()
				.ForEach(x => CreateTableDataRow(grid,
					x.Title,
					x.Description,
					x.From?.ToShortDateString(),
					x.To?.ToShortDateString(),
					VirtuaCon.EnumUtility.GetFriendlyName(typeof(TrainingActivityType), x.TrainingActivityType),
					x.CostImplication,
					x.Venue,
					x.RewardPoints,
					x.TrainingLabels,
					x.BursaryTrainingActivityDetail?.BursaryType,
					x.AdditionalInfo,
					string.Join(";" + Environment.NewLine, x.UsersTrained.Select(u => u.Name)),
					GetTrainers(x)
				));
			section.AddElement(grid);
			document.AddElement(section);
		}

		private string GetTrainers(TrainingActivityModel x) {
			switch (x.TrainingActivityType) {
				case TrainingActivityType.Bursary:
					return string.Join(";" + Environment.NewLine, x.BursaryTrainingActivityDetail.ConductedBy.Select(u => u.Name));
				case TrainingActivityType.External:
					return string.Join(";" + Environment.NewLine, x.ExternalTrainingActivityDetail.ConductedBy.Select(u => u.CompanyName));
				case TrainingActivityType.Internal:
					return string.Join(";" + Environment.NewLine, x.InternalTrainingActivityDetail.ConductedBy.Select(u => u.Name));
				case TrainingActivityType.MentoringAndCoaching:
					return string.Join(";" + Environment.NewLine, x.MentoringAndCoachingTrainingActivityDetail.ConductedBy.Select(u => u.Name));
				case TrainingActivityType.ToolboxTalk:
					return string.Join(";" + Environment.NewLine, x.ToolboxTalkTrainingActivityDetail.ConductedBy.Select(u => u.Name));
				default:
					break;
			}
			return "";
		}
		TrainingActivityReportModel IQueryHandler<TrainingActivityListQuery, TrainingActivityReportModel>.ExecuteQuery(TrainingActivityListQuery query) {
			var list = new List<TrainingActivityModel>();
			if (query.Id.HasValue && _repository.Find(query.Id.Value) != null)
				return new TrainingActivityReportModel {
					FilteredResults = new[]
						{Project.ToTrainingActivityModel.Compile().Invoke(_repository.Find(query.Id.Value))}
				};
			query.From = query.From.AtBeginningOfDay();
			query.To = query.To.AtEndOfDay();
			var r = _repository.List.AsQueryable();
			if (query.From.HasValue)
				r = r.Where(x => x.From >= query.From);
			if (query.To.HasValue)
				r = r.Where(x => x.To <= query.To);
			if (query.CostFloor.HasValue)
				r = r.Where(x => x.CostImplication >= query.CostFloor.Value);
			if (query.CostCeiling.HasValue)
				r = r.Where(x => x.CostImplication <= query.CostCeiling.Value);
			r = FilterByRace(query, r);
			r = FilterByGroup(query, r);
			r = FilterByUsersTrained(query, r);
			r = FilterByTrainingActivityType(query, r);
			list = r.ToList().AsQueryable().Select(Project.ToTrainingActivityModel).ToList();
			if (!string.IsNullOrWhiteSpace(query.TrainingLabels))
				list = FilterByTrainingLables(query, list);
			return new TrainingActivityReportModel {
				FilteredResults = list
			};

		}
	}
}
namespace Ramp.Services.Projection {
	public static partial class Project {
		public static readonly Expression<Func<StandardUserTrainingActivityLog, TrainingActivityListModel>> ToTrainingActivityListModel
			= x => new TrainingActivityListModel {
				Id = x.Id.ToString(),
				CostImplication = x.CostImplication,
				Description = x.Description,
				From = x.From,
				LastEditDate = x.LastEditDate,
				Title = x.Title,
				To = x.To,
				TrainingActivityType = x.TrainingActivityType
			};
		public static readonly Expression<Func<StandardUserTrainingActivityLog, TrainingActivityModel>> ToTrainingActivityModel
			= x => new TrainingActivityModel {
				Id = x.Id.ToString(),
				AdditionalInfo = x.AdditionalInfo,
				CostImplication = x.CostImplication,
				CostPerUser = (x.CostImplication ?? 0M) / (x.UsersTrained.Any() ? x.UsersTrained.Count() : 1),
				Created = x.Created,
				Description = x.Description,
				LastEditDate = x.LastEditDate,
				From = x.From,
				RewardPoints = x.RewardPoints,
				Time = x.Time,
				Title = x.Title,
				To = x.To,
				Venue = x.Venue,
				TrainingActivityType = x.TrainingActivityType,
				CreatedBy = StandardUserToUserModelShort.Compile().Invoke(x.CreatedBy),
				EditedBy = StandardUserToUserModelShort.Compile().Invoke(x.EditedBy),
				UsersTrained = x.UsersTrained.AsQueryable().Select(StandardUserToUserModelShort).ToList(),
				TrainingLabels = x.TrainingLabels,
				Documents = x.Documents.AsQueryable().Select(Project.Upload_UploadResultViewModel).ToList(),
				BursaryTrainingActivityDetail = x.BursaryTrainingActivityDetail == null ? null : ToBursaryTrainingActivityDetailModel.Compile().Invoke(x.BursaryTrainingActivityDetail),
				ExternalTrainingActivityDetail = x.ExternalTrainingActivityDetail == null ? null : ToExternalTrainingActivityDetailModel.Compile().Invoke(x.ExternalTrainingActivityDetail),
				InternalTrainingActivityDetail = x.InternalTrainingActivityDetail == null ? null : ToInternalTrainingActivityDetailModel.Compile().Invoke(x.InternalTrainingActivityDetail),
				MentoringAndCoachingTrainingActivityDetail = x.MentoringAndCoachingTrainingActivityDetail == null ? null : ToMentoringAndCoachingTrainingActivityDetailModel.Compile().Invoke(x.MentoringAndCoachingTrainingActivityDetail),
				ToolboxTalkTrainingActivityDetail = x.ToolboxTalkTrainingActivityDetail == null ? null : ToToolboxTalkTrainingActivityDetailModel.Compile().Invoke(x.ToolboxTalkTrainingActivityDetail)
			};


		public static readonly Expression<Func<StandardUserTrainingActivityLog, TrainingActivityModel>> ToTrainingActivityLogModel
			= x => new TrainingActivityModel {
				Id = x.Id.ToString(),
				AdditionalInfo = x.AdditionalInfo,
				CostImplication = x.CostImplication,
				CostPerUser = (x.CostImplication ?? 0M) / (x.UsersTrained.Any() ? x.UsersTrained.Count() : 1),
				Created = x.Created,
				Description = x.Description,
				LastEditDate = x.LastEditDate,
				From = x.From,
				RewardPoints = x.RewardPoints,
				Time = x.Time,
				Title = x.Title,
				To = x.To,
				Venue = x.Venue,
				TrainingActivityType = x.TrainingActivityType,
				TrainingLabels = x.TrainingLabels
			};

		public static readonly Expression<Func<StandardUser, UserModelShort>> StandardUserToUserModelShort
			= x => new UserModelShort {
				//GroupId = x.Group == null ? new Guid?() : x.Group.Id,
				Id = x.Id,
				MobileNumber = x.MobileNumber,
				UserName = x.FirstName + " " + x.LastName,
				Name = !(x.LastName == null || x.LastName == "") ? !(x.FirstName == null || x.FirstName == "") ? x.LastName + ", " + x.FirstName : x.LastName : x.FirstName,
				Email = x.EmailAddress,
				CustomUserRoleId = x.CustomUserRoleId
			};
		public static readonly Expression<Func<StandardUser, UserModelShort>> StandardUserToUserModelShort_Firstname_LastNames
			= x => new UserModelShort {
				//GroupId = x.Group == null ? new Guid?() : x.Group.Id,
				Id = x.Id,
				MobileNumber = x.MobileNumber,
				UserName = x.FirstName + " " + x.LastName,
				Name = !(x.FirstName == null || x.FirstName == "") && !(x.LastName == null || x.LastName == "") ? x.FirstName + " " + x.LastName : x.FirstName,
				Email = x.EmailAddress,
			IsActive=x.IsActive
			};
		public static readonly Expression<Func<User, UserModelShort>> UserToUserModelShort
			= x => new UserModelShort {
				GroupId = null,
				Id = x.Id,
				MobileNumber = x.MobileNumber,
				UserName = x.FirstName + " " + x.LastName,
				Name = !(x.LastName == null || x.LastName == "") ? !(x.FirstName == null || x.FirstName == "") ? x.LastName + ", " + x.FirstName : x.LastName : x.FirstName,
				Email = x.EmailAddress
			
			};
		public static readonly Expression<Func<BursaryTrainingActivityDetail, BursaryTrainingActivityDetailModel>> ToBursaryTrainingActivityDetailModel
			= x => new BursaryTrainingActivityDetailModel {
				BursaryType = x.BursaryType,
				ConductedBy = x.ConductedBy.AsQueryable().Select(StandardUserToUserModelShort).ToList(),
				Invoices = x.Invoices.AsQueryable().Select(Project.Upload_UploadResultViewModel).ToList()
			};
		public static readonly Expression<Func<ExternalTrainingActivityDetail, ExternalTrainingActivityDetailModel>> ToExternalTrainingActivityDetailModel
			= x => new ExternalTrainingActivityDetailModel {
				ConductedBy = x.ConductedBy.AsQueryable().Select(ToExternalTrainingProviderListModel).ToList(),
				Invoices = x.Invoices.AsQueryable().Select(Project.Upload_UploadResultViewModel).ToList()
			};
		public static readonly Expression<Func<InternalTrainingActivityDetail, InternalTrainingActivityDetailModel>> ToInternalTrainingActivityDetailModel
			= x => new InternalTrainingActivityDetailModel {
				ConductedBy = x.ConductedBy.AsQueryable().Select(StandardUserToUserModelShort).ToList()
			};
		public static readonly Expression<Func<MentoringAndCoachingTrainingActivityDetail, MentoringAndCoachingTrainingActivityDetailModel>> ToMentoringAndCoachingTrainingActivityDetailModel
			= x => new MentoringAndCoachingTrainingActivityDetailModel {
				ConductedBy = x.ConductedBy.AsQueryable().Select(StandardUserToUserModelShort).ToList()
			};
		public static readonly Expression<Func<ToolboxTalkTrainingActivityDetail, ToolboxTalkTrainingActivityDetailModel>> ToToolboxTalkTrainingActivityDetailModel
			= x => new ToolboxTalkTrainingActivityDetailModel {
				ConductedBy = x.ConductedBy.AsQueryable().Select(StandardUserToUserModelShort).ToList()
			};
	}
}
