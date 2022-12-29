var playbookSaveInterval = undefined;
$(function () {
    playbookSaveInterval = window.setInterval(function () {
        AutoSave();
    }, 120 * 1000);
    ko.validation.init({
        registerExtenders: true,
        insertMessages: true,
        decorateElement: true,
        errorElementClass: 'has-error',
        errorMessageClass: 'help-block'
    });
    ko.validation.onShowAllMessages = function () {
        $('.has-error').first().find('input, textarea, select').first().focus();
    }
});
function Controller(app, model, viewBag, links) {
    var self = this;
    self.applyValidation = function (model, validation) {
        var validationGroup = [];
        $.each(model, function (i, property) {
            if (validation[i]) {
                $.each(validation[i], function (x, validator) {
                    model[i].extend(validator);
                });
                validationGroup.push(property);
            }
        });
        model.validationErrors = ko.validation.group(validationGroup);
    };

    self.app = app;
    self.details = ko.mapping.fromJS(model.data);
    self.applyValidation(self.details, model.validation);
    if (viewBag.PageTitle) {
        self.details.PageTitle = new ko.observable(viewBag.PageTitle);
    }
    self.posting = ko.observable(false);
    self.ckeditorOptions = { customConfig: links.customConfigPath };
    self.uploadPathInitial = links.uploadPathInitial;
    self.uploadPathCommit = links.uploadPathCommit;
    self.rotateImagePath = links.uploadImagePath;
    self.trainingGuideId = new ko.observable(model.data.TrainingGuidId);
    self.uploadContentToolsSuccess = uploadContentToolsSuccess;
    self.uploadContentToolsError = uploadContentToolsError;
    self.fileuploadOptions = {
        showProgress: false,
        url: links.uploadLink,
        before: onBeforeUploadEventHandler,
        complete: onUploadCompleteEventHandler,
        success: onUploadSuccessEventHandler,
        onProgress: onProgressEventHandler,
        autoupload: true,
        doNotAutoMap: true,
        error: onUploadErrorHandler
    };
    self.fileuploadOptionsCoverPicture = {
        showProgress: false,
        url: links.uploadLink,
        before: onBeforeUploadEventHandler_CP,
        complete: onUploadCompleteEventHandler_CP,
        success: onUploadSuccessEventHandler_CP,
        onProgress: onProgressEventHandler_CP,
        autoupload: true,
        error: onUploadError_CP
    };
    self.Models = viewBag.Models;
    self.modalTitle = new ko.observable();
    self.Action = new ko.observable(viewBag.Action);
    self.jstreeOptions = {
        onChange: selectedCategoryChanged
    };
    self.links = links;
    self.concurrentUploads = new ko.observableArray();
    self.details.CanSave = new ko.observable(function () {
        if (self.concurrentUploads().length < 1) {
            $('#Save').removeClass('disabled');
            return true;
        } else {
            if (!$('#Save').hasClass('disabled'))
                $('#Save').addClass('disabled');
            return false;
        }
    }).extend({ notity: 'always' });
    if (!ko.isObservable(self.details.CoverPictureVM)) {
        self.details.CoverPictureVM = new ko.observable(self.details.CoverPictureVM);
    }
    self.Minimized = new ko.observable(false);
    self.UploadCoverPictureError = new ko.observable(false);
    self.setPreviewModeLandscape = function () {
        self.details.PlaybookPreviewMode(1);
    }
    self.setPreviewModePortrait = function () {
        self.details.PlaybookPreviewMode(0);
    }
    self.print = print;
    self.downloadReport = downloadReport;
    self.setPrintableTrue = function () {
        self.details.Printable(true);
    }
    self.setPrintableFalse = function () {
        self.details.Printable(false);
    }
}
function print(guide) {
    AutoSave();
    var url = ko.unwrap(vm.links.print);
    var query = {
        TrainingGuideId: vm.details.TrainingGuidId() =='00000000-0000-0000-0000-000000000000' ? null : ko.unwrap(vm.details.TrainingGuidId)
    };
    if (!query.TrainingGuideId) {
        notif({
            msg: 'Please save first, then try again',
            type: 'error'
        });
        return;
    }
    url = url + '?' + $.param(query);
    vm.downloadReport(url);
}
function downloadReport(url) {
    var request = new XMLHttpRequest();
    request.open("GET", url);
    request.responseType = 'blob';
    request.onload = function () {
        var userAgent = window.navigator.userAgent;
        var allowBlob = userAgent.indexOf('Chrome') > -1 || userAgent.indexOf('Firefox') > -1;
        if (!allowBlob) {
            window.navigator.msSaveBlob(this.response,
                this.getResponseHeader('filename') || "download-" + $.now());
        } else {
            var url = window.URL.createObjectURL(this.response);
            var a = document.createElement("a");
            document.body.appendChild(a);
            a.href = url;
            a.download = this.getResponseHeader('filename') || "download-" + $.now();
            a.click();
            window.setTimeout(function () { document.body.removeChild(a); }, 500);
        }
    }
    request.send();
}
function uploadContentToolsSuccess(response) {
}
function uploadContentToolsError(response) {
}
function selectedCategoryChanged(node, action) {
    vm.details.CategoryName(action.node.text);
    $('#categoryMenu').hide();
};
function toggleCategoryMenu() {
    $('#categoryMenu').toggle();
}
function toDisplayString(value, maxCharacters) {
    var displayString = [];
    var string = '';
    var index = 0;
    if (!value)
        return value;
    if (value.length - 1 <= maxCharacters) {
        string = value;
    } else {
        do {
            var next = '';
            var nextIndex = (index + maxCharacters) < value.length ? (index + maxCharacters) : value.length;
            next = value.substring(index, nextIndex);
            next += ' ';
            index = nextIndex;
            displayString.push(next);
        } while (index < value.length - 1);
        for (var i = 0; i < displayString.length; i++) {
            string += displayString[i];
        }
    }
    return string;
}
function addLink(data) {
    var link = data.Link();
    var chapterLink = ko.mapping.fromJS(vm.Models[1].fileUploadResult);
    var url = data.Link();
    if (data.LinkType() == "Youtube" && url.indexOf('watch?v=') > -1) {
        url = url.replace('watch?v=', 'embed/');
    } else if (data.LinkType() == 'Vimeo' && url.indexOf('https://vimeo.com/') > -1) {
        var webUrl = url.split('/');
        var id = webUrl[webUrl.length - 1];
        url = 'https://player.vimeo.com/video/' + id;
    }
    chapterLink.Url(url);
    chapterLink.Number(data.ChapterNumber());
    chapterLink.Type(data.LinkType());
    chapterLink.Embeded(true);
    data.Attachments.push(new ko.observable(chapterLink));
    data.Link(null);
    data.AddLink(false);
}
function GetSubString(string, length) {
    if (string) {
        return string.substring(0, length);
    }
}
function GetId() {
    return $.ajax({
        method: 'GET',
        url: vm.links.getId,
        cache: false,
        error: function (error) {
            notify("Please check your Connectivity.", 'Error');
        }
    });
}
function AddPlaybookChapter() {
    addInitialiedChapter().then(function (chapter) {
        vm.details.TraningGuideChapters.push(chapter);
        var e = ContentTools.EditorApp.get();
        e.ignition()._domEdit.click();
    });  
}
function addInitialiedChapter() {
    var promise = new Promise(function (resolve, reject) {
        var chapter = ko.mapping.fromJS(vm.Models[0].trainingGuideChapter);
        var newChapterNumber = 1;
        $.each(vm.details.TraningGuideChapters(), function () {
            var chapter = this;
            if ($('#' + chapter.ChapterNumber())) {
                if (chapter.ChapterNumber() == 0) {
                    newChapterNumber += 2;
                } else {
                    newChapterNumber = chapter.ChapterNumber() + 1;
                }
            }

        });
        chapter.ChapterNumber(newChapterNumber);
        chapter.ChapterName('Your chapter name here...');
        GetId().then(function (data) {
            chapter.TraningGuideChapterId(data);
            chapter.New(true);
            resolve(chapter);
        });
    });
    return promise;
}

function simulateUploadClick(data,event) {
    var newElement = $('<input data-bind="fileUpload:Upload,fileuploadOptions:vm.fileuploadOptions,valueProperty:new ko.mapping.fromJS(vm.Models[1].fileUploadResult),container:$data" class="upload hidden" type="file" name="files[]" accept="Image/*" />');

    if (event.target.src.indexOf('imgDoc.png') > 0)
        $(newElement).attr('accept', 'image/*');
    else if (event.target.src.indexOf('WordDoc.png') > 0)
        $(newElement).attr('accept', '.doc,.docx');
    else if (event.target.src.indexOf('excelDoc.png') > 0)
        $(newElement).attr('accept', '.xls,.xlsx');
    else if (event.target.src.indexOf('ppDoc.png') > 0)
        $(newElement).attr('accept', '.ppt,.pptx,.pps,.ppsx');
    else if (event.target.src.indexOf('videoDoc.png') > 0)
        $(newElement).attr('accept', 'video/mp4,video/x-m4v,video/*');
    else if (event.target.src.indexOf('otherDoc.png') > 0)
        $(newElement).attr('accept', '.pdf');
    else if (event.target.src.indexOf('AudioIcon') > 0){
        $(newElement).attr('accept', 'audio/*');
    }
    $(newElement).appendTo('#inputs');
    ko.applyBindingsToNode(newElement[0], null, data);
    $(newElement).click();
}
function showDialog(data, event) {
    if (event.toElement.src.indexOf('youTube') > 0) {
        data.LinkType('Youtube');
    }
    if (event.toElement.src.indexOf('vimeo') > 0) {
        data.LinkType('Vimeo');
    }
    data.AddLink(true);
}
function onBeforeUploadEventHandler_CP(element, fileName, valueProperty) {
    if (checkFileType(fileName)) {
        vm.concurrentUploads.push(valueProperty);
        return true;
    }
    return false;
}
function onUploadSuccessEventHandler_CP(data, textStatus, xhr, valueProperty, valueAccessor) {
    if (data !== false) {
        vm.concurrentUploads.remove(valueProperty);
        vm.UploadCoverPictureError(false);
    }
    else {
        onUploadError_CP(valueProperty);
    }
}
function onUploadCompleteEventHandler_CP(valueProperty) {
}
function onProgressEventHandler_CP(event, position, total, percentage, fileName, valueProperty, submitEvent, valueAccessor) {
}
function onBeforeUploadEventHandler(element, fileName, valueProperty, container) {
    if (checkFileType(fileName)) {
        valueProperty.InProcess(true);
        valueProperty.Name(fileName.replace('C:\\fakepath\\', ''));
        valueProperty.Description(fileName.replace('C:\\fakepath\\', ''));
        container.Attachments.push(valueProperty);
        vm.concurrentUploads.push(valueProperty);
        return true;
    }
    return false;
}
function onUploadCompleteEventHandler(valueProperty, container) {
}
function onUploadSuccessEventHandler(data, textStatus, xhr, valueProperty, valueAccessor, container) {
    if (data !== false) {
        var upload = new ko.observable(ko.mapping.fromJS(data));
        upload().Index = new ko.observable(container.Attachments().length);
        var oldUpload = container.Attachments.remove(valueProperty);
        container.Attachments.push(upload);
        vm.concurrentUploads.remove(valueProperty);
    } else {
        onUploadErrorHandler(valueProperty, container);
    }
}
function onUploadErrorHandler(valueProperty, container) {
    valueProperty.Name();
    valueProperty.Error = new ko.observable(true);
    valueProperty.InProcess(false);
    container.Attachments.remove(valueProperty);
    container.Errors.push(valueProperty);
    vm.concurrentUploads.remove(valueProperty);
}
function onUploadError_CP(valueProperty) {
    vm.UploadCoverPictureError(true);
    vm.concurrentUploads.remove(valueProperty);
}
function onProgressEventHandler(event, position, total, percentage, fileName, valueProperty, submitEvent, valueAccessor, container) {
    valueProperty.Progress(percentage + "%");
    valueProperty.Size(total);
    valueProperty.submitEvent = submitEvent;
    valueProperty.container = new ko.observable(container);
}
function getScaledSize(total) {
    var totalString = "";
    var unit = { KB: ' KB', MB: ' MB', GB: ' GB' };
    totalString = (total / 1000).toPrecision(5) + unit.KB;
    if (total / 1000 > 1000) {
        totalString = (total / (1000000)).toPrecision(5) + unit.MB;
    }
    if (total / (1000000) > 1000) {
        totalString = (total / 1000000000).toPrecision(5) + unit.GB;
    }
    return totalString;
}
function removeLink(parent, upload) {
    var item;
    $.each(parent.Attachments(), function () {
        var url;
        if (ko.isObservable(this)) {
            url = this().Url();
        } else {
            url = this.Url();
        }
        if (url === upload.Url()) {
            item = this;
        }
    });
    parent.Attachments.remove(item);
}
function removeItemFromArray(parent, upload) {
    var item;
    $.each(parent.Attachments(), function () {
        var id;
        if (ko.isObservable(this)) {
            id = this().Id();
        } else {
            id = this.Id();
        }
        if (id === upload.Id()) {
            item = this;
        }
    });
    if (item) {
        parent.Attachments.remove(item);
    }
}
function cancelUpload(parent, upload) {
    $.each(upload.container().Attachments(), function (i) {
        var match = false;
        if (ko.isObservable(this)) { match = (this() == upload); }
        else { match = (this == upload); }

        if (match) {
            upload.submitEvent.data().jqxhr.abort();
            parent.Attachments.remove(upload);
        }
    });
}
function deleteUpload(parent, upload) {
    removeItemFromArray(parent, upload);
}
function clearUpload(parent, upload) {
    parent.Errors.remove(upload);
}
function checkFileType(fileName) {
    var ext = fileName.substring(fileName.lastIndexOf('.') + 1).toUpperCase();
    if (ext == "PNG" || ext == "JPEG" || ext == "JPG" || ext == "GIF" || ext == "BMP") {
        return true;
    } else if (ext == "MP4") {
        return true;
    } else if (ext == "PPSX" || ext == "PPS" || ext == "DOC" || ext == "DOCX" || ext == "XLS" || ext == "XLSX" || ext == "PPT" || ext == "PPTX" || ext == "PDF" || ext == "CSV") {
        return true;
    } else if (ext == "MP3") {
        return true;
    } else {
        notif({
            msg: "<b>Error :</b> Please select a valid file type to upload.",
            type: "error"
        });
        return false;
    }
}
function setUpClickAction(upload) {
    if ((upload.Type().indexOf('image') > -1) || (upload.Type().indexOf('video') > -1)) {
        addToGallery(upload);
        return false;
    }
    return true;
}
function addToGallery(upload) {
    var uploads = [];
    uploads.push(new gallaryObject(upload));
    blueimp.Gallery(uploads);
}
function gallaryObject(upload) {
    return {
        title: upload.Name(),
        href: upload.Url(),
        type: upload.Type()
    };
}
function updateCoverPicture() {
    $(".coverPictureUpload").click();
}
function Validation() {
    if (vm.details.TraningGuideChapters().length < 2) {
        return false;
    }
    return true;
}
function getImages(chapter) {
    // Return an object containing image URLs and widths for all regions
    var e = ContentTools.EditorApp.get();
    if (e.getState() === 'editing')
        e.stop(true);
    else
        e.save(true);

    var html = $(chapter.ChapterContent());
    $.each(html,function () {
        var tag = $(this);
        var src = tag.attr('src');
        if (src) {
            var present = false;
            $.each(chapter.ContentToolsUploads(), function () {
                if (this.url === src)
                    present = true;
            });
            if (!present)
                chapter.ContentToolsUploads.push({ url: src });
        }
    });
    
}

function Save() {
    if (vm.details.validationErrors() < 1) {
        if (!Validation()) {
            notif({
                msg: "<b>Error :</b> Please create a minimum of two chapters.",
                type: "error"
            });
        } else {
            var stopExecution = false;
            $.each(vm.details.TraningGuideChapters(), function () {
                var chapter = this;
                getImages(chapter);
                if (chapter.Errors().length > 0) {
                    notif({
                        msg: "<b>Error :</b> Please review unsuccessful uploads.",
                        type: "error"
                    });
                    $('.uploaderror').focus();
                    stopExecution = true;
                }

            });
            if (!stopExecution) {
                var e = ContentTools.EditorApp.get();
                e.save(true);
                app.data.ajax(vm.Action(), vm.details, 'POST', null, null, function (data) {
                    if (data.Created == true) {
                        if (vm.Action().indexOf('Create') > -1) {
                            notif({
                                msg: "Created successfully",
                                type: "success"
                            });
                        } else if (vm.Action().indexOf('EditTrainingGuide') > -1) {
                            notif({
                                msg: "Saved successfully",
                                type: "success"
                            });
                        }
                        window.setTimeout(function () { window.location.href = vm.links.index }, 2000);
                    } else {
                        notif({
                            msg: "<b>Error :</b> An Error has occured, please try again.",
                            type: "error"
                        });
                    }
                });
            }
        }
    } else {
        vm.details.validationErrors.showAllMessages(true);
    }
}
function fixPassiveSave() {
    var e = ContentTools.EditorApp.get();
    if (e.getState() === 'editing')
        e.stop(true);
    else
        e.save(true);
}
function AutoSave() {
    if (ko.unwrap(vm.posting))
        return;
    window.clearInterval(playbookSaveInterval);
    if (vm.details.validationErrors() < 1) {
        fixPassiveSave();
        vm.posting(true);
        if (vm.Action().indexOf('Create') > -1) {
            $.ajax({
                type: 'POST',
                url: vm.Action(),
                data: ko.mapping.toJS(vm.details),
                success: function (data) {
                    vm.posting(false);
                    playbookSaveInterval = window.setInterval(function () {
                        AutoSave();
                    }, 120 * 1000);
                    if (data.Created == true) {
                        notif({
                            msg: "Saved successfully",
                            type: "success"
                        });
                        vm.Action(vm.links.EditTrainingGuide);
                        vm.details.TrainingGuidId(data.Id);
                        $.each(ko.unwrap(vm.details.TraningGuideChapters) || [], function () {
                            if (ko.unwrap(this.New) && $.isFunction(this.New))
                                this.New(false);
                        });
                    } else {
                        notif({
                            msg: "<b>Error :</b> Auto save failed.",
                            type: "error"
                        });
                    }
                },
                error: function () {
                    vm.posting(false);
                    playbookSaveInterval = window.setInterval(function () {
                        AutoSave();
                    }, 120 * 1000);
                    notif({
                        msg: "<b>Error :</b> Auto save failed.Please check you connectivity.",
                        type: "error"
                    });
                }
            });
        } else if (vm.Action().indexOf('EditTrainingGuide') > -1) {
            $.ajax({
                type: 'POST',
                url: vm.Action(),
                data: ko.mapping.toJS(vm.details),
                success: function (data) {
                    vm.posting(false);
                    $.each(ko.unwrap(vm.details.TraningGuideChapters) || [], function () {
                        if (ko.unwrap(this.New) && $.isFunction(this.New))
                            this.New(false);
                    });
                    if (data.Created == true) {
                        notif({
                            msg: "Saved successfully",
                            type: "success"
                        });
                    } else {
                        notif({
                            msg: "<b>Error :</b> Auto save failed.",
                            type: "error"
                        });
                    }
                },
                error: function () {
                    vm.posting(false);
                    notif({
                        msg: "<b>Error :</b> Auto save failed. Please check you connectivity.",
                        type: "error"
                    });
                }
            });
        }
    }
}
function toggleChapterContents(chapter) {
    chapter.Minimized(!chapter.Minimized());
}
function removeChapter(chapter) {
    vm.details.TraningGuideChapters.remove(chapter);
}
function rearrangeChapters() {
    var e = ContentTools.EditorApp.get();
    e.save(true);
    e.start();
    if (event.target.type !== 'button' && event.target.type !== 'text' && event.target.type !== 'select-one' && event.target.type !== 'submit') {
        $.each(vm.details.TraningGuideChapters(), function () {
            focus($(event.target).parent());
            this.Minimized(true);
        });
    } else {
        return true;
    }
}


tour.steps = [
      {
          target: 'Title',
          title: 'Title',
          content: 'Every playbook needs a title - choose something short and sweet which indicates the content of your Playbook.',
          placement: 'top',
          xOffset: 'center',
          arrowOffset: 'center'
      },

     {
         target: 'description',
         placement: 'top',
         title: 'Description',
         content: 'You can elaborate further on the contents of your playbook - usually two or three sentences which sum up who the playbook applies to, why it is important for your users should learn the content and what the expected outcomes of their learning would be.',
         xOffset: 'center',
         arrowOffset: 'center'
     },
     {
         target: 'category',
         placement: 'top',
         title: 'Category',
         content: 'Select a category to file your playbook to make it easy for your users to reference. You must create categories before you start creating your playbook. If you need to create a category, click the “Manage Playbook Categories” button at the top of the screen.',
         xOffset: 'center',
         arrowOffset: 'center'
     },

     {
         target: 'coverPicture',
         placement: 'top',
         title: 'Cover Picture',
         content: 'You can use this feature to display a cover picture for your Playbook.',
         xOffset: 'center',
         arrowOffset: 'center'
     },
        {
            target: 'linkPlaybook',
            placement: 'top',
            title: 'Link this page to',
            content: 'Optional - you can link a specific page in your playbook to another playbook in the system. I.e. if your playbook was on the maintenance of a vehicle, you could link a specific page about changing the oil to an entire separate Playbook which explains oil changes in detail.',
            arrowOffset: 100,
            xOffset: 'center',
            arrowOffset: 'center',
            width: 240
        },

        {
            target: 'chapterContent',
            placement: 'top',
            title: 'Text Editor',
            content: 'Use these features to Bold, Underline and other changes to your page copy.',
            arrowOffset: 100,
            xOffset: 'center',
            arrowOffset: 'center',
            width: 240
        },
        {
            target: 'resources',
            placement: 'top',
            title: 'Resorces',
            content: 'Use these options to add resources to your chapter.You can attach Word, Excel and PowerPoint Documents, PDF and various forms of multimedia.',
            xOffset: 'center',
            arrowOffset: 'center',
            width: 240
        },
      {
          target: 'addChapter',
          placement: 'top',
          title: 'Add Chapter',
          content: 'Click here to add a new chapter to your Playbook.',
          xOffset: 'center',
          arrowOffset: 'center',
          width: 160
      },
      {
          target: 'save',
          placement: 'top',
          title: 'Save Button',
          content: 'Click here to save your progress as you build your Playbook.',
          arrowOffset: 100,
          xOffset: 'center',
          arrowOffset: 'center',
          width: 210
      }

];