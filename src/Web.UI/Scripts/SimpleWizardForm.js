function DisplayStep() {
    var selectedStep = null;
    var firstInputError = $("input.input-validation-error:first"); // check for any invalid input fields
    if (firstInputError.length) {
        selectedStep = $(".wizard-confirmation");
        if (selectedStep && selectedStep.length) { // the confirmation step should be initialized and selected if it exists
            UpdateConfirmation();
        }
        else {
            selectedStep = firstInputError.closest(".wizard-step"); // the first step with invalid fields should be displayed
        }
    }
    if (!selectedStep || !selectedStep.length) {
        selectedStep = $(".wizard-step:first"); // display first step if no step has invalid fields
    }

    $(".wizard-step:visible").hide(); // hide the step that currently is visible
    selectedStep.fadeIn(); // fade in the step that should become visible

    // enable/disable the prev/next/submit buttons
    if (selectedStep.prev().hasClass("wizard-step")) {
        $("#wizard-prev").show();
    }
    else {
        $("#wizard-prev").hide();
    }
    if (selectedStep.next().hasClass("wizard-step")) {
        $("#wizard-submit").hide();
        $("#wizard-next").show();
    }
    else {
        $("#wizard-next").hide();
        $("#wizard-submit").show();
    }
}

function PrevStep() {
    var currentStep = $(".wizard-step:visible"); // get current step

    if (currentStep.prev().hasClass("wizard-step")) { // is there a previous step?

        currentStep.hide().prev().fadeIn();  // hide current step and display previous step

        $("#wizard-submit").hide(); // disable wizard-submit button
        $("#wizard-next").show(); // enable wizard-next button

        if (!currentStep.prev().prev().hasClass("wizard-step")) { // disable wizard-prev button?
            $("#wizard-prev").hide();
        }
    }
}

function NextStep() {
    var currentStep = $(".wizard-step:visible"); // get current step

    var validator = $("form").validate(); // get validator
    var valid = true;
    currentStep.find("input:not(:blank)").each(function () { // ignore empty fields, i.e. allow the user to step through without filling in required fields
        if (!validator.element(this)) { // validate every input element inside this step
            valid = false;
        }
    });
    if (!valid)
        return; // exit if invalid

    if (currentStep.next().hasClass("wizard-step")) { // is there a next step?

        if (currentStep.next().hasClass("wizard-confirmation")) { // is the next step the confirmation?
            
            UpdateConfirmation();
        }

        currentStep.hide().next().fadeIn();  // hide current step and display next step

        $("#wizard-prev").show(); // enable wizard-prev button

        if (!currentStep.next().next().hasClass("wizard-step")) { // disable wizard-next button and enable wizard-submit?
            $("#wizard-next").hide();
            $('#ActiveFinishPreview').removeAttr('disabled');
            $("#wizard-submit").show();
        }
    }
}

function Submit() {
    clearInterval(window.timerId);
    var count = $('#viewLaterQuestionCount').val();
    var qCount = $('#AllQuestion label').length;
    var selects = $('select').map(function () { return $(this).val(); });
    var unanswered = $.inArray('', selects);
    if (unanswered > -1) {
        bootbox.confirm("Some questions are unattempted. Do you really want to submit the test?", function (result) {
            if (result) {
                $('#SubmitTest').submit();
            } 
        });
 
    } else {
        bootbox.confirm("Do you really want to submit the test?", function (result) {
            if (result) {
                $('#SubmitTest').submit();
            }
        });
    }


    /* if (/*$("form").valid()#1#1) { // validate all fields, including blank required fields
        $("#SubmitTest").submit();
        var allAmswers = $('select');
    }
    else {
        DisplayStep(); // validation failed, redisplay correct step
    }*/
}

function UpdateConfirmation() {
   // UpdateValidationSummary();
    var fieldList = $("<ol/>");
    $(".wizard-step:not(.wizard-confirmation)").find("input").each(function () {
        var input = this;
        var value;
        switch (input.type) {
            case "hidden":
                return;
            case "checkbox":
                value = input.checked;
                break;
            default:
                value = input.value;
        }
        var name = $('label[for="' + input.name + '"]').text();
        fieldList.append("<li><label>" + name + "</label><span>" + value + "&nbsp;</span></li>");
    });
    $("#field-summary").children().remove();
    $("#field-summary").append(fieldList);
}

function UpdateValidationSummary() {
    var validationSummary = $("#validation-summary");
    if (!validationSummary.find(".validation-summary-errors").length) { // check if validation errors container already exists, and if not create it
        $('<div class="validation-summary-errors"><ul></ul></div>').appendTo(validationSummary);
    }
    var errorList = $(".validation-summary-errors ul");
    errorList.find("li.field-error").remove(); // remove any field errors that might have been added earlier, leaving any server errors intact
    $('.field-validation-error').each(function () {
        var element = this;
        $('<li class="field-error">' + element.innerText + '</li>').appendTo(errorList); // add the current field errors
    });
    if (errorList.find("li").length) {
        $("#validation-summary span").show();
    }
    else {
        $("#validation-summary span").hide();
    }
}

$(function () {
    // attach click handlers to the nav buttons
    $("#wizard-prev").click(function () { PrevStep(); });
    $("#wizard-next").click(function () { NextStep(); });
    $("#wizard-submit").click(function () { Submit(); });

    // display the first step (or the confirmation if returned from server with errors)
    DisplayStep();
});

$.validator.setDefaults({ ignore: "" }); // default is ":hidden" for jquery.validate 1.8+