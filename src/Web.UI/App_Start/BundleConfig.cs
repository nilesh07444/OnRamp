using System.Web.Optimization;
using Web.UI.Code.AreaReferencesManagement;

namespace Web.UI
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new Bundle("~/bundles/css").Include(
                "~/Content/alertify/themes/alertify.bootstrap.css",
                "~/Content/bootstrap-toggle/css/bootstrap-toggle.css",
                "~/Content/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css",
                "~/Content/fileUpload/jquery.fileupload.css",
                "~/Content/fileUpload/jquery.fileupload-ui.css",
                "~/Content/hopscotch/dist/css/hopscotch.css",
                "~/Content/notifit/notifIt/css/notifIt.css",
                "~/Content/jquery.dataTables.css",
                "~/Content/bootstrap-colorpicker/css/bootstrap-colorpicker.min.css",
                "~/Content/star-rating.min.css",
                "~/Content/bootstrap-dropdownhover.min.css",
                "~/Content/bootstrap-tour.css",
                "~/Content/SampleWizardForm.css",
                "~/Content/blueimp-gallery/css/blueimp-gallery.css",
                "~/Content/bootstrap-tagsinput/dist/bootstrap-tagsinput-typeahead.css"
                ));
            bundles.Add(new Bundle("~/bundles/js").Include(
                "~/Scripts/jquery.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/moment.js",
                "~/Scripts/knockout.js",
                "~/Scripts/jquery.fancybox.js",
                "~/Scripts/jquery.fancybox-buttons.js",
                "~/Scripts/jquery.fancybox-media.js",
                "~/Scripts/jquery.fancybox-thumbs.js",
                "~/Scripts/raphael.min.js",
                "~/Scripts/canvas-to-blob.min.js",
                "~/Scripts/morris.min.js",
                "~/Scripts/notifIt.min.js",
                "~/Scripts/jquery.browser.min.js",
                "~/Scripts/star-rating.min.js",
                "~/Scripts/load-image.all.min.js",
                "~/Scripts/jquery.dataTables.min.js",
                "~/Scripts/hopscotch.min.js",
                "~/Scripts/typeahead.bundle.js",
                "~/Scripts/bootstrap-tagsinput.js",
                "~/Scripts/knockout.*",
                "~/Scripts/blueimp-gallery.js",
                "~/Scripts/blueimp-*",
                "~/Scripts/bootstrap-select.min.js",
                "~/Content/bootstrap-colorpicker/js/bootstrap-colorpicker.min.js",
                "~/Scripts/content-tools.js",
                "~/Scripts/additional-methods.js",
                "~/Scripts/alertify.js",
                "~/Scripts/bootstrap-*",
                "~/Scripts/canvas-to-blob.min.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/jquery.blueimp-gallery.js",
                "~/Scripts/jquery.form.js",
                "~/Scripts/jquery.timeago.js",
                "~/Scripts/jquery.smartWizard.js",
                "~/Scripts/jquery.metisMenu.js",
                "~/Scripts/SimpleWizardForm.js",
                "~/Scripts/star-rating.min.js",
                "~/Scripts/navbarmenu.js",
                "~/Scripts/modernizr-2.8.3.js",
                "~/Scripts/bootbox.js",
                "~/Scripts/promise.min.js"
                ));
            #region ManageTrainingTest

            bundles.Add(new ScriptBundle(ManageTrainingTestScripts.CreateScriptsVirtualPath).Include(ManageTrainingTestScripts.Create));
            bundles.Add(new StyleBundle(ManageTrainingTestStyles.CreateStylesVirtualPath).Include(ManageTrainingTestStyles.Create));
            bundles.Add(new ScriptBundle(ManageTrainingTestScripts.TestEndedWithReviewScriptsVirtualPath).Include(ManageTrainingTestScripts.TestEndedWithReview));
            bundles.Add(new StyleBundle(ManageTrainingTestStyles.TestEndedWithReviewStylesVirtualPath).Include(ManageTrainingTestStyles.TestEndedWithReview));
            bundles.Add(new ScriptBundle(ManageTrainingTestScripts.FocusAreaReportScriptsVirtualPath).Include(ManageTrainingTestScripts.FocusAreaReport));
            bundles.Add(new StyleBundle(ManageTrainingTestStyles.FocusAreaReportStylesVirtualPath).Include(ManageTrainingTestStyles.FocusAreaReport));

            #endregion ManageTrainingTest

            #region ManageTrainingGuide

            bundles.Add(new ScriptBundle(ManageTrainingGuidesScripts.CreateScriptsVirtualPath).Include(ManageTrainingGuidesScripts.Create));
            bundles.Add(new StyleBundle(ManageTrainingGuidesStyles.CreateStylesVirtualPath).Include(ManageTrainingGuidesStyles.Create));
            bundles.Add(new ScriptBundle(ManageTrainingGuidesScripts.MyTrainingGuidesVirtualPath).Include(ManageTrainingGuidesScripts.MyTrainingGuides));
            bundles.Add(new ScriptBundle(ManageTrainingGuidesScripts.Assign_UnAssignPlaybooksAndTestsScriptsVirtualPath).Include(ManageTrainingGuidesScripts.Assign_UnAssignPlaybooksAnsTests));
            bundles.Add(new StyleBundle(ManageTrainingGuidesStyles.Assign_UnAssignPlaybooksAndTestsStylesVirtualPath).Include(ManageTrainingGuidesStyles.Assign_UnAssignPlaybooksAnsTests));
            bundles.Add(new ScriptBundle(ManageTrainingGuidesScripts.PreviewByReferenceIdScriptsVirtualPath).Include(ManageTrainingGuidesScripts.PreviewByReferenceId_K));
            bundles.Add(new StyleBundle(ManageTrainingGuidesStyles.PreviewByReferenceIdStylesVirtualPath).Include(ManageTrainingGuidesStyles.PreviewByReferenceId_K));

            #endregion ManageTrainingGuide
        }
    }
}