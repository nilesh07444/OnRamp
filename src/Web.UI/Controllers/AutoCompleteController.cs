using Ramp.Contracts.Query.Label;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.TrainingActivity;
using Ramp.Contracts.QueryParameter.TrainingLabel;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ramp.Contracts.Query.User;
using Web.UI.Code.ActionFilters;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class AutoCompleteController : RampController
    {
        [OutputCache(CacheProfile = "AutoComplete-User")]
        public ActionResult Users(UserSearchQuery query)
        {
            var users = ExecuteQuery<UserSearchQuery, IEnumerable<UserModelShort>>(query);
            return Json(
                users.Select(x => new
                {
                    Value = x.Name,
                    Extra = x.Email,
                    Id = x.Id,
                    Tokens = x.Name.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                }), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(CacheProfile = "AutoComplete-UserAndGroup")]
        public ActionResult UsersAndGroups()
        {
            var users = ExecuteQuery<UsersAndGroupsQuery, IEnumerable<UserWithGroupModel>>(new UsersAndGroupsQuery());
            return Json(users.Select(u => new
            {
                Value = u.Name,
                Extra = u.GroupTitle,
                u.Id,
                Tokens = u.Name.Split(new [] {" "}, StringSplitOptions.RemoveEmptyEntries)
            }), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile = "AutoComplete-User")]
        public ActionResult ExternalTrainingProviders(ExternalTrainingProviderListQuery query)
        {
            var providers = ExecuteQuery<ExternalTrainingProviderListQuery,IEnumerable<ExternalTrainingProviderListModel>>(query);
            return Json(
                providers.Select(x => new
                {
                    Value = x.CompanyName,
                    Extra = x.CompanyName,
                    Id = x.Id,
                    Tokens = x.CompanyName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                }), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile ="AutoComplete-TrainingLabel")]
        public ActionResult TrainingLabels(LabelListQuery query)
        {
            var labels = ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(query);
            return Json(labels.Select(x => x.Name), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile = "AutoComplete-Label")]
        public ActionResult Labels(LabelListQuery query)
        {
            var labels = ExecuteQuery<LabelListQuery, IEnumerable<TrainingLabelListModel>>(query);
            return Json(labels.Select(x => x.Name), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile = "AutoComplete-BursaryType")]
        public ActionResult BursaryType(FetchAllKnownBursaryTypesQuery query)
        {
            var types = ExecuteQuery<FetchAllKnownBursaryTypesQuery, IEnumerable<string>>(query);
            return Json(types.Select(x => new {
                Id = "",
                Value = x,
                Extra = x,
                Tokens = x.Split(new[] { " " },StringSplitOptions.RemoveEmptyEntries)
            }), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile = "AutoComplete-Race")]
        public ActionResult Races(GetAllRaceCodesQuery query)
        {
            var races = ExecuteQuery<GetAllRaceCodesQuery, List<RaceCodeViewModel>>(query);
            return Json(races.Select(x => new {
                Id = x.Id.ToString(),
                Value = x.Code,
                Extra = x.Description,
                Tokens = x.Description.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
            }), JsonRequestBehavior.AllowGet);
        }
        [OutputCache(CacheProfile = "AutoComplete-Group")]
        public ActionResult Groups(AllGroupsByCustomerAdminQueryParameter query)
        {
            var groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(query);
            return Json(groups.Select(x => new {
                Id = x.GroupId.ToString(),
                Value = x.Title,
                Extra = x.Description,
                Tokens = x.Title.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
            }), JsonRequestBehavior.AllowGet);
        }
    }
}