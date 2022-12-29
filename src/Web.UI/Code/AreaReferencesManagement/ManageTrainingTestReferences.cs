using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.UI.Code.Extensions;

namespace Web.UI.Code.AreaReferencesManagement
{
    public static class ManageTrainingTestScripts
    {
        public const string CreateScriptsVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/Create/Scripts",
                            TestEndedWithReviewScriptsVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/TestEndedWithReview/Scripts",
                            FocusAreaReportScriptsVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/FocusAreaReport/Scripts";
        #region Create
        private static string[] create()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingTests/Create.js");
            return scripts.ToArray();
        }

        public static string[] Create { get { return create(); } }

        #endregion Create

        #region TestEndedWithReview

        private static string[] testEndedWithReview()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingTests/TestEndedWithReview.js");
            return scripts.ToArray();
        }

        public static string[] TestEndedWithReview { get { return testEndedWithReview(); } }

        #endregion TestEndedWithReview

        #region FocusAreaReport
        private static string[] focusAreaReport()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingTests/FocusAreaReport.js");
            return scripts.ToArray();
        }
        public static string[] FocusAreaReport { get { return focusAreaReport(); } }
        #endregion

        #region AssignTrainingTestToUsersOrGroups
        private static string[] assignTrainingTestToUsersOrGroups()
        {
            var scripts = new List<string>();
            scripts.Add("~/Scripts/Areas/ManageTrainingTests/AssignTrainingTestToUsersOrGroups.js");
            return scripts.ToArray();
        }
        public static string[] AssignTrainingTestToUsersOrGroups { get { return assignTrainingTestToUsersOrGroups(); } }
        #endregion

    }

    public static class ManageTrainingTestStyles
    {
        public const string CreateStylesVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/Create/Styles",
                            TestEndedWithReviewStylesVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/TestEndedWithReview/Styles",
                            FocusAreaReportStylesVirtualPath = "~/ManageTrainingTest/ManageTrainingTest/FocusAreaReport/Styles";

        #region Create

        private static string[] create()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingTest/Create.css");
            return styles.ToArray();
        }

        public static string[] Create { get { return create(); } }

        #endregion Create

        #region TestEndedWithReview

        private static string[] testEndedWithReview()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingTest/TestEndedWithReview.css");
            return styles.ToArray();
        }

        public static string[] TestEndedWithReview { get { return testEndedWithReview(); } }

        #endregion TestEndedWithReview

        #region FocusAreaReport
        private static string[] focusAreaReport()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingTest/FocusAreaReport.css");
            return styles.ToArray();
        }
        public static string[] FocusAreaReport { get { return focusAreaReport(); } }
        #endregion

        #region AssignTrainingTestToUsersOrGroups
        private static string[] assignTrainingTestToUsersOrGroups()
        {
            var styles = new List<string>();
            styles.Add("~/Content/Areas/ManageTrainingTest/AssignTrainingTestToUsersOrGroups.css");
            return styles.ToArray();
        }
        public static string[] AssignTrainingTestToUsersOrGroups { get { return assignTrainingTestToUsersOrGroups(); } }
        #endregion

    }
}