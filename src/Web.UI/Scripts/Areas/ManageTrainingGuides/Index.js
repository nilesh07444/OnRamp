var Collaborators = new Array();
var SelectedGuideReferenceId = "";

function ToggleCollaborateModal(e) {
    if (e) {
        SelectedGuideReferenceId = $(e).closest('tr').find('.sorting_1').html().trim();
        if ($(e).attr('data-users') !== "") {
            AddColaboratingUsers(e);
        }
    }
    $('#collaborateModal').toggle();
}

function AddColaboratingUsers(e) {
    var userIds = $(e).attr('data-users');
    $('#collaborateModal').find('input').each(function() {
        if ($(this).attr('type') === 'checkbox') {
            if (userIds.indexOf($(this).val()) > -1) {
                $(this).prop('checked', true);
                Collaborators.push($(this).val());
            }
        }
    });
}
function AddRemoveUserToCollobaoratedList(e) {
    if ($(e).attr('locked') == undefined) {
        if ($(e).is(':checked')) {
            var k;
            var unique = true;
            for (k = 0; k < Collaborators.length; k++) {
                if (Collaborators[k] === $(e).val())
                    unique = false;
            }
            if (unique === true) {
                Collaborators.push($(e).val());
            }
        } else {
            var i = 0;
            for (i = 0; i < Collaborators.length; i++) {
                if (Collaborators[i] === $(e).val())
                    Collaborators[i] = "";
            }
        }
    } else {
        $(e).prop('checked', true);
    }
}
