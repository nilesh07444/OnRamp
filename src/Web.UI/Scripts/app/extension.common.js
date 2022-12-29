// KNOCKOUT EXTENSIONS

// currency
ko.bindingHandlers.currency = {
    symbol: ko.observable('R'),
    update: function (element, valueAccessor, allBindingsAccessor) {
        return ko.bindingHandlers.text.update(element, function () {
            var value = +(ko.utils.unwrapObservable(valueAccessor()) || 0),
                symbol = ko.utils.unwrapObservable(allBindingsAccessor().symbol === undefined
                    ? ko.bindingHandlers.currency.symbol
                    : allBindingsAccessor().symbol);

            return symbol + ' ' + value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1 ");
        });
    }
};

// Here's a custom Knockout binding that makes elements shown/hidden via jQuery's fadeIn()/fadeOut() methods
// Could be stored in a separate utility library
ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).fadeIn() : $(element).fadeOut();
    }
};

// Here's a custom Knockout binding that makes elements shown/hidden via jQuery's slideUp()/slideDown() methods
// Could be stored in a separate utility library
ko.bindingHandlers.slideVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.unwrap(value) ? $(element).slideDown() : $(element).slideUp();
    }
};

//
//ko.bindingHandlers.date = {
//    format: 'DD MMM YYYY',
//    update: function (element, valueAccessor, allBindingsAccessor) {
//        return ko.bindingHandlers.text.update(element, function () {
//            var value = +(ko.utils.unwrapObservable(valueAccessor()) || 0),
//                format = ko.utils.unwrapObservable(allBindingsAccessor().format === undefined
//                    ? ko.bindingHandlers.date.format
//                    : allBindingsAccessor().format);

//            return new moment(value).toString(format);
//        });
//    }
//};

// hidden
ko.bindingHandlers.hidden = {
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        ko.bindingHandlers.visible.update(element, function () { return !value; });
    }
};

// not hidden
ko.bindingHandlers.nothidden = {
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        ko.bindingHandlers.visible.update(element, function () { return value; });
    }
};

// ToJson
ko.bindingHandlers.toJSON = {
    update: function (element, valueAccessor) {
        return ko.bindingHandlers.text.update(element, function () {
            return ko.toJSON(valueAccessor(), null, 2);
        });
    }
};

// Stop binding
ko.bindingHandlers.stopBinding = {
    init: function () {
        return { controlsDescendantBindings: true };
    }
};
ko.virtualElements.allowedBindings.stopBinding = true;

// TimeAgo
function toTimeAgo(dt) {
    var secs = (((new Date()).getTime() - dt.getTime()) / 1000),
        days = Math.floor(secs / 86400);

    return days === 0 && (
        secs < 60 && "just now" ||
        secs < 120 && "a minute ago" ||
        secs < 3600 && Math.floor(secs / 60) + " minutes ago" ||
        secs < 7200 && "an hour ago" ||
        secs < 86400 && Math.floor(secs / 3600) + " hours ago") ||
        days === 1 && "yesterday" ||
        days < 31 && days + " days ago" ||
        days < 60 && "one month ago" ||
        days < 365 && Math.ceil(days / 30) + " months ago" ||
        days < 730 && "one year ago" ||
        Math.ceil(days / 365) + " years ago";
};

ko.bindingHandlers.timeAgo = {
    update: function (element, valueAccessor) {
        var val = unwrap(valueAccessor()),
            date = new Date(val), // WARNING: this is not compatibile with IE8
            timeAgo = toTimeAgo(date);
        return ko.bindingHandlers.html.update(element, function () {
            return '<time datetime="' + encodeURIComponent(val) + '">' + timeAgo + '</time>';
        });
    }
};

// Toggle binding
ko.bindingHandlers.toggle = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        ko.applyBindingsToNode(element, {
            click: function () {
                value(!value());
            }
        });
    }
};

// Binding for enter key
ko.bindingHandlers.enterkey = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var allBindings = allBindingsAccessor();

        $(element).on('keypress', 'input, textarea, select', function (e) {
            var keyCode = e.which || e.keyCode;
            if (keyCode !== 13) {
                return true;
            }

            var target = e.target;
            target.blur();
            target.select();
            target.focus();

            allBindings.enterkey.call(viewModel, viewModel, target, element);

            return false;
        });
    }
};

// Binding to mask for decimal values ONLY all other characters will be ignored.
ko.bindingHandlers.decimalonly = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var allBindings = allBindingsAccessor();

        var isSelected = function isTextSelected(input) {
            try {
                if (!input.selectionStart)
                    return false;
            } catch (e) {
                return false;
            }

            if (typeof input.selectionStart == "number") {
                return input.selectionStart == 0 && input.selectionEnd == input.value.length;
            } else if (typeof document.selection != "undefined") {
                input.focus();
                return document.selection.createRange().text == input.value;
            }

            return false;
        };

        $(element).on("keypress keyup blur", function (event) {
            var input = $(element);
            var wasSelected = isSelected(element);

            setTimeout(function () {
                input.val(input.val().replace(/[^0-9\.]/g, ''));
                if (event.type == 'keyup' && wasSelected)
                    input.select();
            }, 5);
        });
    }
};

// Custom binding for modal dialog
ko.bindingHandlers.modal = {
    init: function (element, valueAccessor) {
        var options = ko.utils.unwrapObservable(valueAccessor());
        var show = null;

        $(element).modal();

        if (options.show)
            show = options.show;
        else
            show = valueAccessor();

        $(element).on('hide.bs.modal', function (e) {
            show(false);
        });

        $(element).on('show.bs.modal', function (e) {
            show(true);
        });

        var focus = true;
        if (options.focus != null)
            focus = options.focus;

        if (focus) {
            $(element).on('shown.bs.modal', function (e) {
                $(element).find('input:visible, select:visible, textarea:visible').not('[readonly]').first().focus().select();
            });
        }
    },
    update: function (element, valueAccessor) {
        var options = ko.utils.unwrapObservable(valueAccessor());

        var show = false;

        if (options.show)
            show = ko.utils.unwrapObservable(options.show);
        else
            show = ko.utils.unwrapObservable(options);

        if (show) {
            $(element).modal('show');
        }
        else {
            $(element).modal('hide');
        }
    }
};

ko.bindingHandlers.validatewith = {
    update: function (element, valueAccessor, allBindings) {
        var value = valueAccessor();

        if ($(element).hasClass('validatewith')) {
            $(element).removeClass('validatewith');
            $(element).parent().parent().find('.field-validation-error').remove();
            $(element).parent().removeClass('has-error');
            $(element).closest('.form-group').removeClass('has-error');
        }

        var name = $(element).attr('name');
        var id = $(element).attr('id');
        var fieldname = $(element).data('model-field');

        if (fieldname == null)
            fieldname = name;

        if (fieldname == null)
            fieldname = id;

        //fieldname = allBindings.get('name') || fieldname;

        var state = ko.utils.unwrapObservable(value);

        if (state != null) {
            var modelState = state.ModelState || state;

            var errorMessages = null;

            if (fieldname != null) {
                errorMessages = modelState[fieldname];

                if (errorMessages != null) {
                    var el = $(element);

                    el.addClass('validatewith');

                    if ($(element).parent().hasClass('input-group')) {
                        el = $(element).parent();
                    }

                    el.parent().addClass('has-error');
                    el.closest('.form-group').addClass('has-error');

                    $(element).popover({
                        content: '<span class="field-validation-error">' + errorMessages + '</span>',
                        placement: $(element).attr('data-placement') || 'bottom',
                        html: true,
                        trigger: 'focus hover'
                    });

                    el.addClass('validation-error');

                    //el.after('<span class="field-validation-error">' + errorMessages + '</span>');
                    if (!$(element).data('DateTimePicker')) { $(element).on('change', _remove); }
                    else { $(element).on('dp.change', _remove); }
                    function _remove() {
                        if ($(this).parent().hasClass('input-group')) {
                            $(this).parent().parent().removeClass('has-error');
                            $(this).parent().removeClass('validation-error');
                            el.closest('.form-group').removeClass('has-error');
                        } else {
                            $(this).parent().removeClass('has-error');
                            $(this).removeClass('validation-error');
                            el.closest('.form-group').removeClass('has-error');
                        }
                        //$(this).siblings('.field-validation-error').remove();
                        try {
                            $(this).popover('destroy');
                        } catch (ex) {
                            //already destroyed
                        }
                        return true;
                    };
                    setTimeout(function () {
                        var first = $('.validation-error:first');

                        if (!first.is(':focus')) {
                            first.focus();
                        }
                    }, 50);
                }
            } else {
                errorMessages = modelState[""];

                if (errorMessages == null)
                    errorMessages = modelState.Message;

                if (errorMessages != null) {
                    $(element).alert(errorMessages(), 'danger');
                }
            }
        } else {
            $(element).popover('hide');
            $(element).popover('destroy');
        }
    }
};

ko.bindingHandlers.fileUpload = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        var options = allBindingsAccessor()['fileuploadOptions'];
        if (options) {
            var showProgress = ko.utils.unwrapObservable(options.showProgress);

            if (showProgress == null)
                showProgress = true;

            $(element).wrap("<form name='upload' id='upload'></form>");
            if (showProgress) {
                var progress = $('<div class="result alert"></div>');
                progress.hide();
                $(element).after(progress);
            }
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var options = allBindingsAccessor()['fileuploadOptions'];
        var valueProperty = allBindingsAccessor()['valueProperty'];
        var container = allBindingsAccessor()['container'];
        if (options) {
            var url = ko.utils.unwrapObservable(options.url);
            var before = ko.utils.unwrapObservable(options.before);
            var success = ko.utils.unwrapObservable(options.success);
            var error = ko.utils.unwrapObservable(options.error);
            var complete = ko.utils.unwrapObservable(options.complete);
            var onProgress = ko.utils.unwrapObservable(options.onProgress);
            var submit = null;
            var doNotAutoMap = ko.utils.unwrapObservable(options.doNotAutoMap);

            if (url) {
                $(element).change(function (event) {
                    console.log('uploading started');
                    var value = element.value;
                    valueAccessor()(value);
                    if (element.value.length > 0) {
                        var $this = $(this),
                            fileName = $this.val();

                        var alert = $(element).next('div.result');

                        if (alert.length > 0) {
                            alert.removeClass('alert-info');
                            alert.removeClass('alert-danger');
                            alert.removeClass('alert-success');
                            alert.addClass('alert alert-info');
                            alert.html('Uploading...');
                            alert.slideDown('slow');
                        }
                        var passedBefore = true;
                        if (before) {
                            if (valueProperty && container) {
                                passedBefore = before($(element), fileName, valueProperty, container);
                            }
                            else if (valueProperty)
                                passedBefore = before($(element), fileName, valueProperty);
                            else
                                passedBefore = before($(element), fileName);
                        }
                        if (passedBefore) {
                            $(element.form).ajaxSubmit({
                                url: url,
                                type: 'POST',
                                dataType: "json",
                                headers: { 'Content-Disposition': 'attachment; filename=' + fileName, Accept: "application/json charset=utf-8" },
                                accepts: {
                                    text: 'application/json'
                                },
                                beforeSubmit: function (formData, $form, options) {
                                    submit = $form;
                                },
                                uploadProgress: function (event, position, total, percentage) {
                                    if (onProgress && valueProperty && container) {
                                        onProgress(event, position, total, percentage, fileName, valueProperty, submit, valueAccessor, container);
                                    }
                                    else if (onProgress && valueProperty) {
                                        onProgress(event, position, total, percentage, fileName, valueProperty, submit, valueAccessor);
                                    }
                                    else if (onProgress)
                                        onProgress(event, position, total, percentage, fileName)
                                },
                                success: function (data, textStatus, xhr) {
                                    console.log('upload successful');
                                    if ((doNotAutoMap && doNotAutoMap == true) && valueProperty) {
                                        //dont do anything
                                    }
                                    else if (valueProperty) {
                                        valueProperty(ko.mapping.fromJS(data));
                                    }

                                    if (alert.length > 0) {
                                        alert.removeClass('alert-info');
                                        alert.addClass('alert-success');
                                        alert.html('Uploading Completed');
                                    }
                                    if (success && valueProperty && container) {
                                        success(data, textStatus, xhr, valueProperty, valueAccessor, container);
                                    }
                                    else if (success && valueProperty) {
                                        success(data, textStatus, xhr, valueProperty, valueAccessor);
                                    }
                                    else if (success) {
                                        success(data, textStatus, xhr);
                                    }
                                },
                                error: function (xhr, errorThrown) {
                                    if (error && valueProperty && container) {
                                        error(valueProperty, container);
                                    }
                                    else if (error && valueProperty) {
                                        error(valueProperty);
                                    }
                                    else if (error)
                                        error(errorThrown);

                                    if (alert.length > 0) {
                                        alert.removeClass('alert-info');
                                        alert.addClass('alert-danger');
                                        alert.html(errorThrown);
                                    }
                                },
                                complete: function () {
                                    valueAccessor()(null);
                                    element.value = '';
                                    if (complete && valueProperty && container) {
                                        complete(valueProperty, container);
                                    }
                                    else if (complete && valueProperty) {
                                        complete(valueProperty);
                                    }
                                    else if (complete) {
                                        complete();
                                    }
                                }
                            });
                        }
                    }

                    event.preventDefault();
                });
            }
        }
    }
};

ko.bindingHandlers.date = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        moment.lang('en', {
            longDateFormat: {
                LT: "h:mm A",
                L: "YYYY/MM/DD",
                l: "YYYY/M/D",
                LL: "MMMM Do YYYY",
                ll: "MMM D YYYY",
                LLL: "MMMM Do YYYY LT",
                lll: "MMM D YYYY LT",
                LLLL: "dddd, MMMM Do YYYY LT",
                llll: "ddd, MMM D YYYY LT"
            }
        });

        moment.lang('en', {
            'calendar': {
                lastDay: '[Yesterday]',
                sameDay: '[Today]',
                nextDay: '[Tomorrow]',
                lastWeek: '[last] ddd L',
                nextWeek: 'ddd L',
                sameElse: 'L'
            }
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var binding = ko.utils.unwrapObservable(valueAccessor());
        var value = ko.utils.unwrapObservable(binding.value);

        var options = binding;

        if (binding.options != null)
            options = ko.utils.unwrapObservable(binding.options);

        var format = 'DD MMM YYYY';
        if (options.format != null)
            format = ko.utils.unwrapObservable(options.format);

        var attr = null;
        if (options.attr != null)
            attr = ko.utils.unwrapObservable(options.attr);

        var toDisplay = '';

        if (value) {
            var m = moment(value);

            if (format)
                toDisplay = m.format(format);
            else
                toDisplay = m.calendar({ 'lastDay': 'D MMMM' });
        }

        if (!attr) {
            // detect type of control
            if ($(element).is('input'))
                $(element).val(toDisplay);
            else
                $(element).text(toDisplay);
        } else
            $(element).attr(attr, toDisplay);
    }
};
ko.bindingHandlers.datetimepicker = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        allBindings = allBindingsAccessor();
        var options = {
            format: 'DD MMM YYYY h:mm A',
            showClear: true,
            showTodayButton: true,
            useCurrent: false
        };
        ko.utils.extend(options, allBindings.dateTimePickerOptions);

        $(element).datetimepicker(options).on('dp.change', function (evntObj) {
            var observable = valueAccessor();
            var picker = $(this).data('DateTimePicker');
            var d = picker.date();

            if (d != null) {
                observable(d.toISOString());
            } else {
                observable(null);
                $(this).data('DateTimePicker:Date', null);
            }
            return true;
        });
        $(element).datetimepicker(options).on("dp.show", function (evntObj) {
            var picker = $(this).data("DateTimePicker");
            var date = $(this).data("DateTimePicker:Date");
            if (date == null)
                picker.date(null);
        });
        if (ko.bindingHandlers.validationCore) {
            ko.bindingHandlers.validationCore.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor());

        if (value != null)
            value = new moment(value);
        else
            value = null;

        $(element).data("DateTimePicker").date(value);
        $(element).data("DateTimePicker:Date", value);
    }
}
ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        allBindings = allBindingsAccessor();

        var options = {
            format: 'DD MMM YYYY',
            showClear: true,
            showTodayButton: true,
            useCurrent: false
        };

        ko.utils.extend(options, allBindings.dateTimePickerOptions);

        $(element).datetimepicker(options).on("dp.change", function (evntObj) {
            var observable = valueAccessor();

            //if (evntObj.timeStamp !== undefined) {
            var picker = $(this).data("DateTimePicker");
            var d = picker.date();

            if (d != null)
                observable(d.toISOString());
            else {
                observable(null);
                $(this).data('DateTimePicker:Date', null);
            }
            //}
        });

        $(element).datetimepicker(options).on("dp.show", function (evntObj) {
            var picker = $(this).data("DateTimePicker");
            var date = $(this).data("DateTimePicker:Date");
            if (date == null)
                picker.date(null);
        });
        if (ko.bindingHandlers.validationCore) {
            ko.bindingHandlers.validationCore.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor());

        if (value != null)
            value = new moment(value);
        else
            value = null;

        $(element).data("DateTimePicker").date(value);
        $(element).data("DateTimePicker:Date", value);
    }
};

ko.bindingHandlers.fixedHeaderTable = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        console.log('init headers');
        $(element).find('table:first').fixedHeaderTable({ height: 300, footer: false, cloneHeadToFoot: false, autoShow: true, fixedColumn: false });
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        //var options = ko.utils.unwrapObservable(valueAccessor());
        //$(element).find('table:first').fixedHeaderTable('hide');
    }
};

ko.extenders.trackChange = function (target, track) {
    if (track) {
        target.isDirty = ko.observable(false);
        target.recentlyChanged = ko.observable(false);
        target.originalValue = target();
        target.resetTracking = function () {
            target.isDirty(false);
            target.originalValue = target();
        };
        target.subscribe(function (newValue) {
            // use != not !== so numbers will equate naturally
            target.isDirty(newValue != target.originalValue);
            target.recentlyChanged(true);
            setTimeout(function () {
                target.recentlyChanged(false);
            }, 50);
        });
    }
    return target;
};


// TYPEAHEAD
ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindings) {
        var options = ko.unwrap(valueAccessor()) || {},
            value = allBindings.get("value"),
            $el = $(element),
            triggerChange = function (e, suggestion) {
                if (value && ko.isObservable(value) && options.value) {
                    value(suggestion[options.value]);
                }
                $el.change();
            }


        $el.typeahead(null, options)
            .on("typeahead:selected", triggerChange)
            .on("typeahead:autocompleted", triggerChange);

        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $el.typeahead("destroy");
            $el = null;
        });
    }
};


ko.bindingHandlers.contentToolsWithImageUpload = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        ContentTools.DEFAULT_TOOLS = [
            [
                'bold',
                'italic',
                'link',
                'align-left',
                'align-center',
                'align-right'
            ], [
                'heading',
                'subheading',
                'paragraph',
                'unordered-list',
                'ordered-list',
                'table',
                'indent',
                'unindent',
                'line-break'
            ], [
                'image',
                'video',
                'preformatted'
            ], [
                'undo',
                'redo',
                'remove'
            ]
        ];
        var e = ContentTools.EditorApp.get();
        var options = allBindingsAccessor();
        if (!$(element).attr(options.contentToolsWithImageUpload.identifier))
            $(element).attr(options.contentToolsWithImageUpload.identifier, options.identifier);
        if (!e.domRegions()) {
            e.init(options.contentToolsWithImageUpload.query, options.contentToolsWithImageUpload.identifier);
        } else {
            e.syncRegions(options.contentToolsWithImageUpload.query);
        }
        e.addEventListener('saved', function (ev) {
            var e = ContentTools.EditorApp.get();
            var region = e.regions()[options.identifier];
            if (region)
                valueAccessor().value(region.html());
        });

        if (ko.bindingHandlers.validationCore) {
            ko.bindingHandlers.validationCore.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
        //set up imageuploader
        if (options && options.imageuploader && options.imageuploader)
            ContentTools.IMAGE_UPLOADER = function (dialog) {
                var image, xhr, xhrComplete, xhrProgress;
                dialog.addEventListener('imageuploader.cancelupload', function () {
                    if (xhr) {
                        xhr.upload.removeEventListener('progress', xhrProgress);
                        xhr.removeEventListener('readystatechange', xhrComplete);
                        xhr.abort();
                    }
                    dialog.state('empty');
                    if (options.imageuploader.cancelupload)
                        options.imageuploader.cancelupload();
                })
                dialog.addEventListener('imageuploader.clear', function () {
                    if (options.imageuploader.clear)
                        options.imageuploader.clear();
                    dialog.clear();
                    image = null;
                })
                dialog.addEventListener('imageuploader.fileready', function (ev) {
                    // Upload a file to the server
                    var formData;
                    var file = ev.detail().file;

                    // Define functions to handle upload progress and completion
                    xhrProgress = function (ev) {
                        // Set the progress for the upload
                        dialog.progress((ev.loaded / ev.total) * 100);
                        if (options.imageuploader.progress)
                            options.imageuploader.progress(ev);
                    }

                    xhrComplete = function (ev) {
                        var response;

                        // Check the request is complete
                        if (ev.target.readyState != 4) {
                            return;
                        }

                        // Clear the request
                        xhr = null
                        xhrProgress = null
                        xhrComplete = null

                        // Handle the result of the upload
                        if (parseInt(ev.target.status) == 200) {
                            // Unpack the response (from JSON)
                            response = JSON.parse(ev.target.responseText);

                            // Store the image details
                            image = {
                                size: response.size,
                                url: response.url
                            };

                            // Populate the dialog
                            dialog.populate(image.url, image.size);
                            if (options.imageuploader.complete)
                                options.imageuploader.complete(image);
                        } else {
                            // The request failed, notify the user
                            new ContentTools.FlashUI('no');
                        }
                    }

                    // Set the dialog state to uploading and reset the progress bar to 0
                    dialog.state('uploading');
                    dialog.progress(0);

                    // Build the form data to post to the server
                    formData = new FormData();
                    formData.append('image', file);
                    if (options.imageuploader.compositeKey)
                        formData.append('trainingGuideId', options.imageuploader.compositeKey);

                    // Make the request
                    xhr = new XMLHttpRequest();
                    xhr.upload.addEventListener('progress', xhrProgress);
                    xhr.addEventListener('readystatechange', xhrComplete);
                    xhr.open('POST', options.imageuploader.uploadPathInitial, true);
                    xhr.send(formData);
                })
                dialog.addEventListener('imageuploader.save', function (ev) {
                    var crop, cropRegion, formData;

                    // Define a function to handle the request completion
                    xhrComplete = function (ev) {
                        // Check the request is complete
                        if (ev.target.readyState !== 4) {
                            return;
                        }

                        // Clear the request
                        xhr = null
                        xhrComplete = null

                        // Free the dialog from its busy state
                        dialog.busy(false);

                        // Handle the result of the rotation
                        if (parseInt(ev.target.status) === 200) {
                            // Unpack the response (from JSON)
                            var response = JSON.parse(ev.target.responseText);

                            // Trigger the save event against the dialog with details of the
                            // image to be inserted.
                            dialog.save(
                                response.url,
                                response.size,
                                {
                                    'alt': response.alt,
                                    'data-ce-max-width': response.size[0],
                                });
                        } else {
                            // The request failed, notify the user
                            new ContentTools.FlashUI('no');
                        }
                    }

                    // Set the dialog to busy while the rotate is performed
                    dialog.busy(true);

                    // Build the form data to post to the server
                    formData = new FormData();
                    formData.append('url', image.url);

                    // Set the width of the image when it's inserted, this is a default
                    // the user will be able to resize the image afterwards.
                    formData.append('width', 600);

                    // Check if a crop region has been defined by the user
                    if (dialog.cropRegion()) {
                        formData.append('crop', dialog.cropRegion());
                    }
                    if (options.imageuploader.compositeKey)
                        formData.append('trainingGuideId', options.imageuploader.compositeKey);
                    // Make the request
                    xhr = new XMLHttpRequest();
                    xhr.addEventListener('readystatechange', xhrComplete);
                    xhr.open('POST', options.imageuploader.uploadPathCommit, true);
                    xhr.send(formData);
                })
                dialog.addEventListener('imageuploader.rotateccw', function () {
                    var direction = 'CCW';
                    // Request a rotated version of the image from the server
                    var formData;

                    // Define a function to handle the request completion
                    xhrComplete = function (ev) {
                        var response;

                        // Check the request is complete
                        if (ev.target.readyState != 4) {
                            return;
                        }

                        // Clear the request
                        xhr = null
                        xhrComplete = null

                        // Free the dialog from its busy state
                        dialog.busy(false);

                        // Handle the result of the rotation
                        if (parseInt(ev.target.status) == 200) {
                            // Unpack the response (from JSON)
                            response = JSON.parse(ev.target.responseText);

                            // Store the image details (use fake param to force refresh)
                            image = {
                                size: response.size,
                                url: response.url + '?_ignore=' + Date.now()
                            };

                            // Populate the dialog
                            dialog.populate(image.url, image.size);
                        } else {
                            // The request failed, notify the user
                            new ContentTools.FlashUI('no');
                        }
                    }

                    // Set the dialog to busy while the rotate is performed
                    dialog.busy(true);

                    // Build the form data to post to the server
                    formData = new FormData();
                    formData.append('url', image.url);
                    formData.append('direction', direction);
                    if (options.imageuploader.compositeKey)
                        formData.append('trainingGuideId', options.imageuploader.compositeKey);
                    // Make the request
                    xhr = new XMLHttpRequest();
                    xhr.addEventListener('readystatechange', xhrComplete);
                    xhr.open('POST', options.imageuploader.rotateImagePath, true);
                    xhr.send(formData);
                })
                dialog.addEventListener('imageuploader.rotatecw', function () {
                    var direction = 'CW';
                    // Request a rotated version of the image from the server
                    var formData;

                    // Define a function to handle the request completion
                    xhrComplete = function (ev) {
                        var response;

                        // Check the request is complete
                        if (ev.target.readyState != 4) {
                            return;
                        }

                        // Clear the request
                        xhr = null
                        xhrComplete = null

                        // Free the dialog from its busy state
                        dialog.busy(false);

                        // Handle the result of the rotation
                        if (parseInt(ev.target.status) == 200) {
                            // Unpack the response (from JSON)
                            response = JSON.parse(ev.target.responseText);

                            // Store the image details (use fake param to force refresh)
                            image = {
                                size: response.size,
                                url: response.url + '?_ignore=' + Date.now()
                            };

                            // Populate the dialog
                            dialog.populate(image.url, image.size);
                        } else {
                            // The request failed, notify the user
                            new ContentTools.FlashUI('no');
                        }
                    }

                    // Set the dialog to busy while the rotate is performed
                    dialog.busy(true);

                    // Build the form data to post to the server
                    formData = new FormData();
                    formData.append('url', image.url);
                    formData.append('direction', direction);
                    if (options.imageuploader.compositeKey)
                        formData.append('trainingGuideId', options.imageuploader.compositeKey);
                    // Make the request
                    xhr = new XMLHttpRequest();
                    xhr.addEventListener('readystatechange', xhrComplete);
                    xhr.open('POST', options.imageuploader.rotateImagePath, true);
                    xhr.send(formData);
                })
            }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        element.addEventListener('DOMSubtreeModified', function () {
            var region = e.regions()[options.identifier];
            if (region)
                valueAccessor().value(region.html());
        }, false);
        var options = allBindingsAccessor();
        var e = ContentTools.EditorApp.get();
        if (!valueAccessor().value()) {
            e.syncRegions(options.contentToolsWithImageUpload.query);
        }
        else if (valueAccessor().value()) {
            var region = undefined;
            if (e.regions()[options.identifier]) {
                region = e.regions()[options.identifier];
                if (region && region.html() !== valueAccessor().value())
                    region.setContent(valueAccessor().value());
            } else {
                $.each(e.domRegions() || [], function (index, c) {
                    if (c.getAttribute('name') === options.identifier && !region)
                        region = c;
                });
                if (region)
                    $(region).html(ko.utils.unwrapObservable(valueAccessor().value()));
            }
        }
    }
};
ko.bindingHandlers.contentTools = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        ContentTools.DEFAULT_TOOLS = [
            [
                'bold',
                'italic',
                'link',
                'align-left',
                'align-center',
                'align-right'
            ], [
                'heading',
                'subheading',
                'paragraph',
                'unordered-list',
                'ordered-list',
                'table',
                'indent',
                'unindent',
                'line-break'
            ], [
                'preformatted'
            ], [
                'undo',
                'redo',
                'remove'
            ]
        ];
        var e = ContentTools.EditorApp.get();
        var options = allBindingsAccessor();
        if (!$(element).attr(options.contentTools.identifier))
            $(element).attr(options.contentTools.identifier, options.identifier);
        e.init(options.contentTools.query, options.contentTools.identifier);
        e.addEventListener('saved', function (ev) {
            var regions = ev.detail().regions;
            if (Object.keys(regions).length == 0) {
                return;
            }
            $.each(Object.keys(regions), function () {
                if (this.toString() == options.identifier)
                    valueAccessor().value(regions[this.toString()]);
            });
            if (options && options.saved)
                options.saved(ev);
        });
        if (ko.bindingHandlers.validationCore) {
            ko.bindingHandlers.validationCore.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
    }
};
ko.bindingHandlers.validationCore = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // insert the message

        if (valueAccessor() && valueAccessor().isValid) {
            var span = document.createElement('SPAN'); //element to hold message
            span.className = ko.validation.configuration.errorMessageClass; //message style
            var parent = $(element).parent().closest(".input-group"); //find the holder div of the input
            if (parent.length > 0) {
                $(parent).after(span); //has holder: add message holder just after the input holder
            } else {
                $(element).after(span); //no holderL add message holder just after the input itself
            }

            try {
                ko.applyBindingsToNode(span, { validationMessage: valueAccessor() });
            } catch (e) {
            }
        }
    }
};
ko.bindingHandlers.jstree = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var valueProperty = allBindingsAccessor()['valueProperty'];
        var options = allBindingsAccessor()['jstreeOptions'];
        if (valueAccessor()) {
            $(element).jstree($.extend({
                'core': {
                    'data': ko.toJS(valueAccessor())
                }
            }, options));
            $(element).on('changed.jstree', function (node, action, selected, event) {
                if (valueProperty) {
                    valueProperty(action.selected[0]);
                }
                if (options) {
                    if (options.onChange) {
                        options.onChange(node, action);
                    }
                }
            });
        }
    }
}
ko.bindingHandlers.jstreeWithSearch = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var valueProperty = allBindingsAccessor()['valueProperty'];
        var filterProperty = allBindingsAccessor()['filterProperty'];

        var options = allBindingsAccessor()['jstreeOptions'];
        if (valueAccessor()) {
            $(element).jstree($.extend({
                "search": {
                    "case-insensitive": true,
                },
                'core': {
                    'data': ko.toJS(valueAccessor())
                },
                "plugins": ["search"]
            }, options));
            $(element).on('changed.jstree', function (node, action, selected, event) {
                if (valueProperty) {
                    valueProperty(action.selected[0]);
                }
                if (options) {
                    if (options.onChange) {
                        options.onChange(node, action);
                    }
                }
            });
            if (filterProperty && ko.isObservable(filterProperty))
                filterProperty.subscribe(function (newValue, oldValue) {
                    if (newValue != oldValue)
                        $(element).jstree('search', newValue);
                });
        }
    }
}

ko.bindingHandlers.uiSortableList = {
    init: function (element, valueAccessor, allBindingsAccesor, context) {
        var $element = $(element),
            list = valueAccessor(),
            handle = allBindingsAccesor()['handle'],
            callback = allBindingsAccesor()['callback'];

        $element.sortable({
            stop: UpdateList,
            handle: handle
        });
        function UpdateList(event, ui) {
            console.log('UpdateList', list());

            var actualArray = list();
            var item = ko.dataFor(ui.item[0]);
            var newIndex = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= list().length) newIndex = list().length - 1;
            var observableToRemove = {};
            $.each(actualArray, function () {
                if (ko.isObservable(this)) {
                    if (this() === item) {
                        observableToRemove = this;
                        return false;
                    }
                } else {
                    if (this === item) {
                        observableToRemove = this;
                        return false;
                    }
                }
            });
            ko.utils.arrayRemoveItem(actualArray, observableToRemove);
            actualArray.splice(newIndex, 0, observableToRemove);
            //  list([]);//clear observableArray

            for (let i = 0; i < actualArray.length; i++) {
                actualArray[i].params.CustomDocumentOrder(i)
            }
            list(actualArray);

            if (callback)
                callback(list);
        }
    }
};
ko.bindingHandlers.selectPicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        if ($(element).is('select')) {
            if (allBindingsAccessor().selectPicker) {
                $(element).selectpicker(allBindingsAccessor().selectPicker);
            }
            $(element).selectpicker('refresh');
        }
    }
};
ko.bindingHandlers.fancybox = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());

        var content = $(element).find('.content');

        content.hide();

        if (content.length > 0) {
            content = $(content[0]).html();
        } else
            content = null;

        var defaultValues = {
            content: content ? 'please wait' : null,
            afterLoad: function () {
                var fancyBox = this;
                var content = $(element).find('.content');

                if (content.length > 0) {
                    content = $(content[0]).clone();
                } else
                    content = null;

                if (content) {
                    fancyBox.content = content;
                }
            }
        };

        value = ko.utils.extend(defaultValues, value);
        setTimeout(function () {
            $(element).fancybox(value);
        }, 0);

        if (!ko.bindingHandlers.fancybox.updater) {
            ko.bindingHandlers.fancybox.updater = true;
            setInterval(function () {
                $.fancybox.update();
            }, 600);
        }
    }
};
ko.bindingHandlers.confirm = {
    init: function (element, valueAccessor, allBindings) {
        var options = ko.unwrap(valueAccessor)();
        ko.applyBindingsToNode(element, {
            click: function () {
                if (!ko.unwrap(options.cancellationToken)) {
                    bootbox.confirm(options.message, function (result) {
                        if (result) {
                            options.delegate(options.model);
                        }
                    });
                }
            }
        });
    }
};
ko.bindingHandlers.dataTable = {
    init: function (element, valueAccessor) {
        $(element).DataTable();
    }
}