$('.form-group input, .form-group select, .form-group textarea').not('input[type=checkbox]').addClass('form-control');

//$('input.tagsinput').tagsinput({
//    confirmKeys: [188,110,32,13, 44]
//});

if (!window.location.origin) {
    window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
}

$('.dropdown-menu .preventclose').click(function (e) {
    e.stopPropagation(); //This will prevent the event from bubbling up and close the dropdown when you type/click on text boxes.
});

$('.dropdown-menu.dropdown-interactive').click(function (e) {
    e.stopPropagation(); //This will prevent the event from bubbling up and close the dropdown when you type/click on text boxes.
});

$('button.dropdown-toggle[data-toggle-dropdown]').click(function(e) {
    var id = $(this).data('toggle-dropdown');
    $('#' + id).slideToggle('fast');
    e.stopPropagation();
});

$('body').click(function() {
    $('.dropdown-menu[id]').slideUp('fast');
});