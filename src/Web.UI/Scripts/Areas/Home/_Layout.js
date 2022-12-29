$(document).ready(function () {
    var path = window.location.pathname;

    if (path.indexOf("Home/Index") >= 0) {
        $("#allNavigation").addClass('hide');
        $("#divHelp").css("display", "none");
        //$('.navbar-toggle').addClass('hide');
    } else {
        $("#allNavigation").removeClass('hide');
        $("#divHelp").css("display", "block");
        //$('.navbar-toggle').removeClass('hide');
    }
    //setect active menu
    var url = window.location.href;

    if (url.indexOf("/Home/Index") > -1) {
        $("#m1").addClass('active');
        $('.main-menu').hide();
        $('.no-resize').css('padding-top', '0px');
    }
    if (url.indexOf("/PackageManagement/Package") > -1) {
        $("#m2").addClass('active');
    }

    if (url.indexOf("/CustomerManagement/CustomerMgmt") > -1) {
        $("#m4").addClass('active');
    }

    if (url.indexOf("/Configurations/ManagerUser") > -1) {
        $("#m5").addClass('active');
    }

    if (url.indexOf("/ProvisionalManagement/ProvisionalMgmt") > -1 ||
        url.indexOf("/ProvisionalManagement/ProvisionalMgmt/ReassignCompanyUser") > -1 ||
        url.indexOf("/Configurations/ManagerUser/CustomerCompaniesLinkedToProvisionalUser") > -1) {
        $("#m3").addClass('active');
        $("#m5").removeClass('active');
    }
    if (url.indexOf("/ManageTrainingGuides/ManageTrainingGuides/AdminViewTrainingGuide") > -1
        || url.indexOf("/ManageTrainingGuides/ManageTrainingGuides/CopyTrainingGuideToCustomer") > -1 ||
        url.indexOf("/Home/UploadTrophy") > -1) {
        $("#m6").addClass('active');
    }
    if (url.indexOf("/CustomerManagement/CustomerMgmt/ViewCustomerSurvey") > -1 ||
    url.indexOf("/Configurations/ManagerUser/KpiReport") > -1) {
        $("#m7").addClass('active');
    }
    if (url.indexOf("/CustomerManagement/CustomerMgmt/ViewCustomerSurvey") > -1 ||
        url.indexOf("/Configurations/ManagerUser/KpiReport") > -1 || url.indexOf("/Categories/Categories/ViewCategoryStatistics") > -1 ||
        url.indexOf("/ManageTrainingTest/ManageTrainingTest/TestHistoryReport") > -1 || url.indexOf("Configurations/ManagerUser/UserActivityDetails") > -1 ||
       url.indexOf("/Configurations/ManagerUser/UserCorrespondenceDetails") > -1 || url.indexOf("/ProvisionalManagement/ProvisionalMgmt/GetUsersLoginStatistics") > -1 ||
       url.indexOf("/ProvisionalManagement/ProvisionalMgmt/GetUserFrequency") > -1 || url.indexOf("/ProvisionalManagement/ProvisionalMgmt/CustomerExpiryDateReport") > -1 ||
         url.indexOf("/Home/AdminReports") > -1) {
        $("#m7").addClass('active');
        $("#m3").removeClass('active');
        $("#m4").removeClass('active');
        $("#m5").removeClass('active');
    }
    var id = $('#companyId').val();

    $(window).on('resize', function () {
        var length = $('.menuItem').length;
        if ($($('.menuItem')[length - 1]).position().top != $($('.menuItem')[0]).position().top) {
            $('.body-wrapper').addClass('resize');
            $('.body-wrapper').removeClass('no-resize');
        }
        else {
            if (!$('.body-wrapper').hasClass('resize')) {
                $('.body-wrapper').addClass('no-resize');
            }
        }
    });
});

$(".nav li a").on("click", function () {
    $(".nav").find(".active").removeClass("active");
    $(this).parent().addClass("active");
});