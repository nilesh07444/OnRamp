using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.UI.Code.Extensions;
using System.Web.Optimization;

namespace Web.UI.Code.AreaReferencesManagement
{
    public static class ManageTrainingGuidesScripts
    {
        public const string CreateScriptsVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/Create/Scripts",
                            PreviewByReferenceIdScriptsVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/PreviewByReferenceId/Scripts",
                            Assign_UnAssignPlaybooksAndTestsScriptsVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/Assign_UnassignPlaybooksAndTests/Scripts",
                            MyTrainingGuidesVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/MyTrainingGuides/Scripts";

        #region Create
        private static string[] create()
        {
            var scripts = new List<string>();
            scripts.AddUnique("~/Scripts/Areas/ManageTrainingGuides/Create.js");
            return scripts.ToArray();
        }

        public static string[] Create { get { return create(); } }

        #endregion Create

        #region PreviewByReferenceId_K

        private static string[] previewByReferenceId_K()
        {
            var scripts = new List<string>();
            scripts.AddUnique("~/Scripts/Areas/ManageTrainingGuides/PreviewByReferenceId_K.js");
            return scripts.ToArray();
        }

        public static string[] PreviewByReferenceId_K { get { return previewByReferenceId_K(); } }

        #endregion PreviewByReferenceId_K

        #region Assign_UnAssignPlaybooksAnsTests

        private static string[] assign_UnAssignPlaybooksAnsTests()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingGuides/Assign_UnAssignPlaybooksAnsTests.js");
            return scripts.ToArray();
        }

        public static string[] Assign_UnAssignPlaybooksAnsTests { get { return assign_UnAssignPlaybooksAnsTests(); } }

        #endregion Assign_UnAssignPlaybooksAnsTests

        #region AssignTrainingGuideToUsersOrGroups
        private static string[] assignTrainingGuideToUsersOrGroups()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingGuides/AssignTrainingGuideToUsersOrGroups.js");
            return scripts.ToArray();
        }
        public static string[] AssignTrainingGuideToUsersOrGroups { get { return assignTrainingGuideToUsersOrGroups(); } }
        #endregion

        #region MyTrainingGuides
        public static string[] MyTrainingGuides { get { return new[] { "~/Scripts/Areas/ManageTrainingGuides/MyPlaybookCategory.js" }; } }
        #endregion
    }

    public static class ManageTrainingGuidesStyles
    {
        public const string CreateStylesVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/Create/Styles",
                            PreviewByReferenceIdStylesVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/PreviewByReferenceId/Styles",
                            Assign_UnAssignPlaybooksAndTestsStylesVirtualPath = "~/Areas/ManageTrainingGuides/ManageTrainingGuides/Assign_UnassignPlaybooksAndTests/Styles";

        #region Create

        private static string[] create()
        {
            var styles = new List<string>();
            styles.AddUnique("~/Content/Areas/ManageTrainingGuides/Create.css");
            return styles.ToArray();
        }

        public static string[] Create { get { return create(); } }

        #endregion Create

        #region PreviewByReferenceId_K

        private static string[] previewByReferenceId_K()
        {
            var styles = new List<string>();
            styles.AddUnique("~/Content/Areas/ManageTrainingGuides/PreviewByReferenceId_K.css");
            return styles.ToArray();
        }

        public static string[] PreviewByReferenceId_K { get { return previewByReferenceId_K(); } }

        #endregion PreviewByReferenceId_K

        #region Assign_UnAssignPlaybooksAnsTests

        private static string[] assign_UnAssignPlaybooksAnsTests()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingGuides/Assign_UnAssignPlaybooksAnsTests.css");
            return styles.ToArray();
        }

        public static string[] Assign_UnAssignPlaybooksAnsTests { get { return assign_UnAssignPlaybooksAnsTests(); } }

        #endregion Assign_UnAssignPlaybooksAnsTests

        #region AssignTrainingGuideToUsersOrGroups
        private static string[] assignTrainingGuideToUsersOrGroups()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingGuides/AssignTrainingGuideToUsersOrGroups.css");
            return styles.ToArray();
        }
        public static string[] AssignTrainingGuideToUsersOrGroups { get { return assignTrainingGuideToUsersOrGroups(); } }
        #endregion
    }
}