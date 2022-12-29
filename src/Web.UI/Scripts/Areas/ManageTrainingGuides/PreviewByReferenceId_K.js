function Controller(app, model, viewBag, links) {
    var self = this;
    self.currentChapterT = ko.observable();
    self.currentChapter = function (chapt) {
        self.currentChapterT(chapt);
        if (!ko.unwrap(self.StorybookMode))
            smootScroll(chapt.ChapterNumber);
    }
    self.showChapter = function (chapt) {
        if (!ko.unwrap(self.StorybookMode))
            return true;
        if (!ko.unwrap(self.currentChapterT))
            return false;
        return ko.unwrap(self.currentChapterT).ChapterNumber() === ko.unwrap(chapt.ChapterNumber);
    }
    self.showCover = function(){
        if (!ko.unwrap(self.StorybookMode))
            return true;
        if (!ko.unwrap(self.currentChapterT) && ko.unwrap(self.details.CoverPictureVM))
            return true;
        return false;
    }
    self.previousChapter = function () {
        var currentChapter = ko.unwrap(self.currentChapterT).ChapterNumber();
        var numberNorm = self.details.TraningGuideChapters()[0].ChapterNumber();
        if (currentChapter == 0 + numberNorm)
            self.currentChapter(null);
        $.each(self.details.TraningGuideChapters(), function () {
            if (ko.unwrap(this.ChapterNumber) ==  currentChapter - 1)
                self.currentChapterT(this);    
        });
    }
    self.nextChapter = function () {
        var currentChapter = 0;
        if (!ko.unwrap(self.currentChapterT) && self.details.TraningGuideChapters().length > 0) {
            self.currentChapterT(self.details.TraningGuideChapters()[0]);
            return true;
        } else
            currentChapter = ko.unwrap(self.currentChapterT).ChapterNumber();
        $.each(self.details.TraningGuideChapters(), function () {
            if (ko.unwrap(this.ChapterNumber) == currentChapter + 1)
                self.currentChapterT(this);
        });
    }
    self.isCurrentChapter = function (chapt) {
        if (!ko.unwrap(self.currentChapterT))
            return false;
        return ko.unwrap(self.currentChapterT).ChapterNumber() === ko.unwrap(chapt.ChapterNumber);   
    }
    self.setPreviewModeLandscape = function () {
        self.currentChapterT(null);
        if (!ko.unwrap(self.details.CoverPictureVM))
            self.nextChapter();
        self.details.PlaybookPreviewMode(1);
    }
    self.setPreviewModePortrait = function () {
        self.currentChapterT(null);
        self.details.PlaybookPreviewMode(0);
    }
    
   
    self.action = ko.observable(viewBag.Action);
    self.app = app;
    self.details = ko.mapping.fromJS(model.data);
    self.StorybookMode = ko.computed(function () { return self.details.PlaybookPreviewMode() === 1; });
    self.isLastChapter = ko.computed(function () {
        if (self.details.TraningGuideChapters().length > 0) {
            var numberNorm = self.details.TraningGuideChapters()[0].ChapterNumber();
            if (!ko.unwrap(self.currentChapterT))
                return false;
            return ko.unwrap(self.currentChapterT).ChapterNumber() == (ko.unwrap(self.details.TraningGuideChapters).length - 1 + numberNorm);
        }
        return false;
    });
    self.ShowTakeTest = ko.computed(function () { return self.details.PlaybookPreviewMode() !== 1 || self.isLastChapter();});
    self.isCover = ko.computed(function () {
        var c = ko.unwrap(self.currentChapterT);
        var numberNorm = self.details.TraningGuideChapters()[0].ChapterNumber();
        if (c == null || c == undefined)
            return true;
        if (!ko.unwrap(self.details.CoverPictureVM) && (c.ChapterNumber() == (0 + numberNorm)))
            return true;
        return false;
    });
    self.links = links;
    self.feedbackOptions = ko.observableArray(['Question', 'Complaint', 'Praise', 'Suggestion']);
    self.feedbackOption = ko.observable().extend({ required: true});
    self.feedbackMessage = ko.observable().extend({ required: true});
    self.validationErrors = ko.validation.group([self.feedbackOption, self.feedbackMessage]);
    self.sendFeedback = sendFeedback;
    self.getHref = function (chapter) {
        return '#' + chapter.ChapterNumber().toString();
    }
    self.truncate = function (string,length) {
        var v = string;
        if (ko.isObservable(v))
            v = v();
        if (v)
            if (v.length && length && length > 0)
                if (v.length > length)
                    v = v.substring(0, length -4) + '...';
        return v;
    }
};
function sendFeedback() {
    var self = this;
    self.feedbackMessage = self.feedbackMessage.extend({ required: true });
    if (self.validationErrors().length > 0) {
        self.validationErrors.showAllMessages();
        return;
    }
    self.app.data.ajax(self.action(),
                        {
                            type: 1,
                            subject: self.details.ReferenceId,
                            option: self.feedbackOption(),
                            message: self.feedbackMessage()
                        },
                        'POST',
                        null,
                        null,
                        function (data) {
                            if (data.success == true) {
                                    notif({
                                        msg: "Thank you for your feedback.<p>It has been sent to the creator of this Playbook.</p>",
                                        type: "success"
                                    });
                                    self.feedbackOption('Question');
                                    self.feedbackMessage('');
                                    self.feedbackMessage.isModified(false);
                            } else {
                                notif({
                                    msg: "<b>Error :</b> We cannot send your feedback at this time.",
                                    type: "error"
                                });
                            }
                        });
}
function previewUpload(parent, data) {
    var type = data.Type();
    if (type.indexOf('image') > -1 || type.indexOf('video') > -1 || type.indexOf('Youtube') > -1 || type.indexOf('Vimeo') > -1) {
        parent.AttachmentPreview(data);
    } else {
        window.open(data.PreviewPath(),'_blank');
        return true;
    }
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
function setUpClickAction(upload, event) {
    var type = upload.Type();
    if (type.indexOf('image') > -1 || type.indexOf('video') > -1) {
        addToGallery(upload);
        event.preventDefault();
    } else {
        return true;
    }
}
$(function () {
    onscroll = function () {
        var divStart = $('#divContent').parent().offset().top;
        var divEnd = divStart + $('#divContent').outerHeight();
        if ($('#divContent').parent().offset().top < $(window).scrollTop() + $('.navbar').height()) {
            $("#divContent").css({ "top": $('.navbar').height(), "position": "fixed", "z-index": "0", "width": $("#divContent").parent().width() });
            if($('footer').height() + $('#page-wrapper').height() + $('.navbar').height()  - $('body').scrollTop() < $('#divContent').height())
                $("#divContent").css({ "top": $('#page-wrapper').height() - $('#divContent').height() - $('.navbar').height(), "position": "absolute", "z-index": "0", "width": $("#divContent").parent().width() });
        }
        else
            $("#divContent").css({ "position": "relative", "width": "100%", "z-index": "0", "top": "auto" });
        
    }
    $(window).resize(function () {
        onscroll();
    });
});
function smootScroll(data) {
    $target = $('#' + ko.unwrap(data));
    event.preventDefault();
    $('html, body').animate({
        scrollTop: $target.offset().top - $('.navbar').height()
    }, 1000);
}