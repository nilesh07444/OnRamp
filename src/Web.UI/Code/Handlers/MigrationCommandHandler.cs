using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.Migration;
using Ramp.Services;
using Ramp.Services.QueryHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Web.UI.Code.Handlers
{
    public class MigrationCommandHandler : ICommandHandlerBase<CreateDefaultIconSetCommand>,
                                           ICommandHandlerBase<AddIconsToDefaultIconSetCommand_ORP1047>,
                                           ICommandHandlerBase<AddDefaultGroupToCustomerCompanies_ORP989>,
                                           ICommandHandlerBase<AddDuplicateIconToDefaultIconSetCommand_ORP914>,
                                           ICommandHandlerBase<AddSprint9IconsCommand>,
                                           ICommandHandlerBase<AppendToDefaultIconSet_Spint12_Command>,
                                           ICommandHandlerBase<AppendToDefaultIconSet_Sprint16_Command>
    {
        private readonly IRepository<IconSet> _iconSetRepository;
        private readonly IRepository<CustomerGroup> _groupRepository;
        public MigrationCommandHandler(IRepository<IconSet> iconSetRepository, IRepository<CustomerGroup> groupRepository)
        {
            _iconSetRepository = iconSetRepository;
            _groupRepository = groupRepository;
        }

        public CommandResponse Execute(AddDefaultGroupToCustomerCompanies_ORP989 command)
        {
            if (!_groupRepository.List.Any())
            {
                var group = new CustomerGroup
                {
                    Description = "Default",
                    Id = Guid.NewGuid(),
                    IsforSelfSignUpGroup = true,
                    Title = "Default"
                };
                _groupRepository.Add(group);
                _groupRepository.SaveChanges();
            }
            return null;

        }

        public CommandResponse Execute(AddIconsToDefaultIconSetCommand_ORP1047 command)
        {
            var defaultSet = _iconSetRepository.List.AsQueryable().FirstOrDefault(x => x.Master);
            if (defaultSet == null)
                throw new Exception("No Default set found to append Icons to");
            if (!defaultSet.Icons.Any(x => x.IconType == IconType.AddUsers))
                defaultSet.Icons.Add(getIcon(IconType.AddUsers, "~/Content/images/OR_icons_original_plus_person.png"));
            if (!defaultSet.Icons.Any(x => x.IconType == IconType.ResetPassword))
                defaultSet.Icons.Add(getIcon(IconType.ResetPassword, "~/Content/images/OR_icons_original_lock.png"));

            _iconSetRepository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(CreateDefaultIconSetCommand command)
        {
            var set = new IconSet
            {
                Name = "DEFAULT",
                Id = Guid.NewGuid(),
                Master = true
            };

            set.Icons.Add(getIcon(IconType.Assign_UnAssignPlaybooks, "~/Content/images/Layout/assign_unassign_playbooks_lg.png"));
            set.Icons.Add(getIcon(IconType.Assign_UnAssignTests, "~/Content/images/Layout/assign_unassign_tests_lg.png"));
            set.Icons.Add(getIcon(IconType.Build, "~/Content/images/Layout/build_lg.png"));
            set.Icons.Add(getIcon(IconType.Collaborate, "~/Content/images/collaborateIcon.png"));
            set.Icons.Add(getIcon(IconType.Delete, "~/Content/images/Layout/delete.png"));
            set.Icons.Add(getIcon(IconType.Edit, "~/Content/images/Layout/pencil.png"));
            set.Icons.Add(getIcon(IconType.Feedback, "~/Content/images/btnexistingfeedback.png"));
            set.Icons.Add(getIcon(IconType.FeedbackUnread, "~/Content/images/btnunreadfeedback.png"));
            set.Icons.Add(getIcon(IconType.GroupManagement, "~/Content/images/Layout/manageUsers_lg.png"));
            set.Icons.Add(getIcon(IconType.Home, "~/Content/images/Layout/home_md.png"));
            set.Icons.Add(getIcon(IconType.ManageCategories, "~/Content/images/Layout/manage_categories_lg.png"));
            set.Icons.Add(getIcon(IconType.ManagePlaybookLibrary, "~/Content/images/Layout/playbookLibrary_lg.png"));
            set.Icons.Add(getIcon(IconType.ManageTestLibrary, "~/Content/images/Layout/manage_test-lib_lg.png"));
            set.Icons.Add(getIcon(IconType.MyPlaybooks, "~/Content/images/managePlyBooks.png"));
            set.Icons.Add(getIcon(IconType.MyTests, "~/Content/images/manageTests.png"));
            set.Icons.Add(getIcon(IconType.PortalSettings, "~/Content/images/AdminSetting.png"));
            set.Icons.Add(getIcon(IconType.Report_Category_Certification_Utilization, "~/Content/images/CategoryCertification.png"));
            set.Icons.Add(getIcon(IconType.Report_Points, "~/Content/images/pointsexport.png"));
            set.Icons.Add(getIcon(IconType.Report_Test_History, "~/Content/images/systemReport.png"));
            set.Icons.Add(getIcon(IconType.Report_User_Activities, "~/Content/images/UserActivities.png"));
            set.Icons.Add(getIcon(IconType.Report_User_KPI, "~/Content/images/UserKPIReport.png"));
            set.Icons.Add(getIcon(IconType.Report_User_Login_Stats, "~/Content/images/UserLogin.png"));
            set.Icons.Add(getIcon(IconType.SelfSignupManagement, "~/Content/images/Layout/self-signup-users-2_lg.png"));
            set.Icons.Add(getIcon(IconType.Send, "~/Content/images/Layout/send_md.png"));
            set.Icons.Add(getIcon(IconType.Track, "~/Content/images/Layout/report_md.png"));
            set.Icons.Add(getIcon(IconType.TrophyCabinet, "~/Content/images/smTrophy.png"));
            set.Icons.Add(getIcon(IconType.UserManagement, "~/Content/images/Layout/manageUsersAndGroups_lg.png"));
            set.Icons.Add(getIcon(IconType.ViewDraftTest, "~/Content/images/Layout/draft.png"));
            set.Icons.Add(getIcon(IconType.ViewPlaybook, "~/Content/images/Layout/view.png"));
            set.Icons.Add(getIcon(IconType.ViewPublishedTest, "~/Content/images/Layout/preview.png"));
            set.Icons.Add(getIcon(IconType.CreateTest, "~/Content/images/btnCreate.png"));
            set.Icons.Add(getIcon(IconType.CheckListType, "~/Content/images/checklist.png"));

            _iconSetRepository.Add(set);
            _iconSetRepository.SaveChanges();
            return null;
        }
        private Icon getIcon(IconType type, string relativeImagePath)
        {
            return new Icon
            {
                Id = Guid.NewGuid(),
                IconType = type,
                Upload = new FileUpload
                {
                    Id = Guid.NewGuid(),
                    FileType = Domain.Enums.FileUploadType.Image,
                    MIMEType = MimeMapping.GetMimeMapping(Path.GetFileName(relativeImagePath)),
                    Name = $"{type.GetFriendlyName()}{Path.GetExtension(relativeImagePath)}",
                    Data = getImageDatafromPath(relativeImagePath)
                }
            };
        }
        private byte[] getImageDatafromPath(string relativePath)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath(relativePath);
                if (File.Exists(path))
                {
                    var data = File.ReadAllBytes(path);
                    return data;
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }
            return null;
        }

        public CommandResponse Execute(AddDuplicateIconToDefaultIconSetCommand_ORP914 command)
        {
            var defaultSet = _iconSetRepository.List.AsQueryable().FirstOrDefault(x => x.Master);
            if (defaultSet == null)
                throw new Exception("No Default set found to append Icons to");
            if (!defaultSet.Icons.Any(x => x.IconType == IconType.DuplicatePlaybook))
                defaultSet.Icons.Add(getIcon(IconType.DuplicatePlaybook, "~/Content/images/Layout/OR_icons_Corporate_Duplicate_Playbook.png"));


            _iconSetRepository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(AddSprint9IconsCommand command)
        {
            var defaultSet = _iconSetRepository.List.AsQueryable().FirstOrDefault(x => x.Master);
            if (defaultSet == null)
                throw new Exception("No Default set found to append Icons to");
            defaultSet.Icons.Add(getIcon(IconType.Export, "~/Content/images/Layout/OR_icons_Original_Export.png"));
            defaultSet.Icons.Add(getIcon(IconType.ManageBEECertificate, "~/Content/images/Layout/OR_icons_Original_BBECertificate.png"));
            defaultSet.Icons.Add(getIcon(IconType.ManageExternalTrainingProviders, "~/Content/images/Layout/OR_icons_Original_ETP.png"));
            defaultSet.Icons.Add(getIcon(IconType.ManageTrainingActivities, "~/Content/images/Layout/OR_icons_Original_ManageTrainingActivities.png"));
            defaultSet.Icons.Add(getIcon(IconType.Report_TrainingActivities, "~/Content/images/Layout/OR_icons_Original_TrainingActivitiesReports.png"));
            defaultSet.Icons.Add(getIcon(IconType.ManageTrainingLabels, "~/Content/images/Layout/OR_icons_Original_TrainingLabelManagement.png"));
            _iconSetRepository.SaveChanges();

            return null;
        }

        public CommandResponse Execute(AppendToDefaultIconSet_Spint12_Command command)
        {
            var defaultSet = _iconSetRepository.List.AsQueryable().FirstOrDefault(x => x.Master);
            if (defaultSet == null)
                throw new Exception("No Default set found to append Icons to");
            defaultSet.Icons.Add(getIcon(IconType.TakeTest, "~/Content/images/btnTakeTest.png"));
            defaultSet.Icons.Add(getIcon(IconType.TestExpired, "~/Content/images/btnTestExpired.png"));
            defaultSet.Icons.Add(getIcon(IconType.Print, "~/Content/images/OR_icons_Original_Print.png"));
            defaultSet.Icons.Add(getIcon(IconType.DownloadCertificate, "~/Content/images/btnDownloadCertificate.png"));
            _iconSetRepository.SaveChanges();
            return null;
        }

        public CommandResponse Execute(AppendToDefaultIconSet_Sprint16_Command command)
        {
            var defaultSet = _iconSetRepository.List.AsQueryable().FirstOrDefault(x => x.Master);
            if (defaultSet == null)
                throw new Exception("No Default set found to append Icons to");
            var iconList = new[]
            {
                getIcon(IconType.SendAssign, "~/Content/images/OR_RebuildIcons_Assign.png"),
                getIcon(IconType.SendUnassign, "~/Content/images/OR_RebuildIcons_UnAssign.png"),
                getIcon(IconType.Send, "~/Content/images/OR_RebuildIcons_Send.png"),
                getIcon(IconType.Build, "~/Content/images/OR_RebuildIcons_Build.png"),
                getIcon(IconType.BuildFromScratch, "~/Content/images/OR_RebuildIcons_BuildFromScratch.png"),
                getIcon(IconType.BuildFromTemplate, "~/Content/images/OR_RebuildIcons_BuildFromTemplate.png"),
                getIcon(IconType.MyDocuments, "~/Content/images/OR_RebuildIcons_MyDocuments.png"),
                getIcon(IconType.MyProgress, "~/Content/images/OR_RebuildIcons_MyProgress.png"),
                getIcon(IconType.PortalSettings, "~/Content/images/OR_RebuildIcons_PortalSettings.png"),
                getIcon(IconType.UserManagement, "~/Content/images/OR_RebuildIcons_UserManagement.png"),
                getIcon(IconType.Track, "~/Content/images/OR_RebuildIcons_Track.png"),
                getIcon(IconType.MemoType, "~/Content/images/OR_RebuildIcons_Memo1.png"),
                getIcon(IconType.PolicyType, "~/Content/images/OR_RebuildIcons_Policy.png"),
                getIcon(IconType.TestType, "~/Content/images/OR_RebuildIcons_Test.png"),
                getIcon(IconType.CheckListType, "~/Content/images/checklist.png"),
                getIcon(IconType.TrainingManualType, "~/Content/images/OR_RebuildIcons_TrainingManual.png"),
                //getIcon(IconType.CustomType, "~/Content/images/CustomDocument.png")
            };
            var iconTypesToReplace = iconList.Select(i => i.IconType);
            var iconsToRemove = defaultSet.Icons.Where(i => iconTypesToReplace.Contains(i.IconType)).ToList();
            foreach (var icon in iconsToRemove)
            {
                defaultSet.Icons.Remove(icon);
            }

            foreach (var icon in iconList)
            {
                defaultSet.Icons.Add(icon);
            }

            _iconSetRepository.SaveChanges();
            return null;
        }
    }
}