var $groupDropDownHtml;
$(function () {
    $groupDropDownHtml = $('#GroupDropDown').html();
    showHideRoles();
});

function showHideRoles() {
    if ($('#UserViewModel_SelectedCustomerType').val() === 'StandardUser') {
        setCheckboxes($('#AdminRoles'), false);
        $('#AdminRoles').hide();
        $('#GroupDropDown').html($groupDropDownHtml);
    } else {
        $('#AdminRoles').show();
        $('#GroupDropDown').html("");
    }
}
function updateRoleSelection() {
    if ($('#UserViewModel_CustomerAdmin').is(':checked')) {
        setCheckboxes($('#AdminRoles'), true);
    } else {
        setCheckboxes($('#AdminRoles'), false);
    }
}

function setCheckboxes(rolesDiv, checkedOption) {
    var inputs = $(rolesDiv).find('input');
    inputs.each(function () {
        if ($(this).attr('type') == 'checkbox') {
            $(this).prop('checked', checkedOption);
        }
    });
}
function validateAdminRoleSelection(caller) {
    if ($('#UserViewModel_SelectedCustomerType').val() === 'Administrator') {
        var adminDiv = $('#AdminRoles');
        var valid = false;
        adminDiv.find('input').each(function() {
            if ($(this).is(':checked'))
                valid = true;
        });
        if (valid === false) {

            notif({
                msg: "<b>Error :</b> Whoops. Please select an admin role.",
                type: "error"
            });
        } else {
            caller.submit();
        }
    } else {
        caller.submit();
    }
}