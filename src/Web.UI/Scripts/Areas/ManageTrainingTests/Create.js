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
function Controller(app, model, viewBag, trainingGuides, links) {
    var self = this;
    self.app = app;
    
    self.details = ko.mapping.fromJS(model.data);
    self.details.TrainingGuides = ko.observableArray(trainingGuides);
    self.details.TrophyList = new ko.observableArray();
    self.details.IntroductionContent.extend({ required: true });
    self.details.gallery = new ko.observableArray();
    self.details.concurrentUploads = new ko.observableArray();
    self.details.CanSave = new ko.observable(CanSave).extend({ notity: 'always' });
    self.details.MaximumRewrites.extend({ preventNegative: 'true', pattern: { message: 'Please enter a number', params: '[0-9]' }, min : '1' });
    self.details.PassMarks.extend({ preventNegative: 'true' ,  required : true, min : 1, max : 100});
    self.details.PassPoints.extend({ preventNegative: 'true' });
    self.details.TestDuration.extend({ preventNegative: 'true' });
    
    self.AutoSaveFrequency = 120000;
    self.links = new ko.observable(links);
    self.fileuploadOptions = new FileUploadOptions();
    self.Action = new ko.observable();
    self.posting = ko.observable(false);
    ApplyValidation(self.details, model.validation);
    GetTrophies();
    SetSelectedTrainingGuide();
    SetDefaultExpiryDate();
    SetUpQuestionUploads();
    SetUpTour();
    SetUpCurrentAction();
   // AddDefaultIntroduction();
    AddDefaultQuestion();
    EnsureThreeAnswersPerQuestion();
    EnsureCorrectAnswerSelectedForEachQuestion();
    SetUpAutoSave();

    self.addQuestion = addQuestion;
    self.allocateDefaultMarksPerQuestion = allocateDefaultMarksPerQuestion;
    self.removeQuestion = removeQuestion;
    self.getSelectedId = getSelectedId;
    self.addMoreAnswers = addMoreAnswers;
    self.getScaledSize = getScaledSize;
    self.simulateUploadClick = simulateUploadClick;
    self.Save = Save;
    self.SaveAndExit = SaveAndExit;
    self.AutoSave = AutoSave;
    self.setTrophyName = setTrophyName;
    self.hideTrophyModal = hideTrophyModal;
    self.showTrophyModal = showTrophyModal;
    self.addToGallery = addToGallery;
    self.cancelQuestionUpload = cancelQuestionUpload;
    self.deleteQuestionUpload = deleteQuestionUpload;
    self.clearUpload = clearUpload;
    self.toDisplayString = toDisplayString;
    self.clearDefaultValueOnClick = clearDefaultValueOnClick;

    function AddDefaultIntroduction() {
        if (!self.details.IntroductionContent())
            self.details.IntroductionContent('[Introduction]');
    }
    function AddDefaultQuestion() {
        if (self.details.QuestionsList().length === 0) {
            addQuestion();
        }
    }
    function addMoreAnswers() {
        var question = this;
        var a = generateAnswer(question.TrainingTestQuestionId()).then(function (data) {
            question.TestAnswerList.push(data);
        });
    }
    function addQuestion() {
        var validation = {
            required: true
        };

        var question = {};
        question.TestQuestionNumber = new ko.observable(self.details.QuestionsList().length + 1);
        question.TestQuestion = new ko.observable('[Question]').extend(validation);
        question.TestAnswerList = new ko.observableArray();
        question.AnswerWeightage = new ko.observable('1');
        question.VideoUpload = new ko.observable();
        question.ImageUpload = new ko.observable();
        question.AudioUpload = new ko.observable();
        question.ImageContainer = new ko.observable();
        question.VideoContainer = new ko.observable();
        question.AudioContainer = new ko.observable();
        question.CorrectAnswerId = new ko.observable(null);
        question.Errors = new ko.observableArray();
        question.TrainingTestId = new ko.observable();
        question.DraftStatus = new ko.observable();
        question.ActiveStatus = new ko.observable();
        question.ActivePublishDate = new ko.observable();
        question.DraftEditDate = new ko.observable();
        question.ParentTrainingTestId = new ko.observable();
        question.CreateDate = new ko.observable();
        question.CreatedBy = new ko.observable();
        GetId().then(function (data) {
            question.TrainingTestQuestionId = new ko.observable(data);
            AddThreeAnswers(question.TrainingTestQuestionId).then(function (answers) {
                if (answers) {
                    $.each(answers(), function () {
                        question.TestAnswerList.push(this);
                    });
                    question.CorrectAnswerId(question.TestAnswerList()[0].TestAnswerId());
                    self.details.QuestionsList.push(question);
                }
            });
        });
    }
    function AddThreeAnswers(TrainingTestQuestionId) {
        var promise = new Promise(function (resolve, reject) {
            var index;
            var done = 0;
            var answers = new ko.observableArray();
            for (index = 0; index < 3; index++) {
                var answer = generateAnswer(TrainingTestQuestionId);
                answer.then(function (data) {
                    answers.push(data);
                    done++;
                    if (done === index)
                        resolve(answers);
                });
            };
        });
        return promise;
    }
    function addToGallery(container, question) {
        var uploads = [];
        uploads.push(new gallaryObject(question[container]()));
        blueimp.Gallery(uploads);
    }
    function allocateDefaultMarksPerQuestion(data) {
        if (data.details.AssignMarksToQuestions() === false) {
            $.each(data.details.QuestionsList(), function () {
                this.AnswerWeightage(1);
            });
        }
        return true;
    }
    function answerValidation() {
        var answerBookeeping = [];
        $.each(self.details.QuestionsList(), function () {
            var q = this;
            var theQuestionIsValid = false;
            var answerOptions = 0;
            var correctAnswer = false;
            var failedUploadReviewed = true;
            if (this.TestQuestion() !== null) {
                theQuestionIsValid = true;
                $.each(this.TestAnswerList(), function () {
                    answerOptions++;
                    if (this.TestAnswerId() === q.CorrectAnswerId()) {
                        if (this.Option()) {
                            this.Correct(true);
                            correctAnswer = true;
                        }
                        else {
                            answerOptions = 0;
                            return false;
                        }
                    }
                    
                });
            }
            if (this.Errors().length > 0) {
                failedUploadReviewed = false;
            }
            if (theQuestionIsValid === false) {
                answerBookeeping.push({ Valid: false, ErrorMessage: 'No Question Specified for Question ' + this.TestQuestionNumber(), QuestionNumber: this.TestQuestionNumber() });
            }
            else if (answerOptions === 0) {
                answerBookeeping.push({ Valid: false, ErrorMessage: 'No Answer Specified for Question ' + this.TestQuestionNumber(), QuestionNumber: this.TestQuestionNumber() });
            } else if (correctAnswer === false) {
                answerBookeeping.push({ Valid: false, ErrorMessage: 'No Answer Selected for Question ' + this.TestQuestionNumber(), QuestionNumber: this.TestQuestionNumber() });
            }
            else if (failedUploadReviewed === false) {
                answerBookeeping.push({ Valid: false, ErrorMessage: 'Please Review Failed Uploads.', QuestionNumber: this.TestQuestionNumber() });
            }
            else {
                answerBookeeping.push({ Valid: true });
            }
        });
        return answerBookeeping;
    }
    function ApplyValidation(model, validation) {
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
    }
    function fixPassiveSave() {
        var e = ContentTools.EditorApp.get();
        var focusedElm = ContentEdit.Root.get().focused();
        e.busy(true);
        e.stop(true);
        e.start();
        e.busy(false);
    }
    function AutoSave() {
        if (ko.unwrap(vm.posting))
            return;
        if (self.details.SelectedTrainingGuideId() === null) {
            notify('Auto Save Failed: Please link a Playbook to this Test.', 'Error');
        } else if (self.details.validationErrors().length > 0) {
            self.details.validationErrors.showAllMessages(true);
            notify('Auto Save Failed: Required Information Missing.', 'Error');
        }
        else {
            var validation = validate(true);
            if (validation.Error === false) {
                if (CanSave()) {
                    fixPassiveSave();
                    ShowLoading(true);
                    vm.posting(true);
                    if (self.details.ViewMode() === 0) {
                        if (self.details.TrainingTestId() === '00000000-0000-0000-0000-000000000000') {
                            $.ajax({
                                url: self.Action(),
                                method: "POST",
                                data: ko.toJS(self.details),
                                success: AutoSaveSuccess,
                                error: AutoSaveFailed
                            });
                        } else {
                            $.ajax({
                                url: self.links().AutoSave,
                                method: "POST",
                                data: ko.toJS(self.details),
                                success: AutoSaveSuccess,
                                error: AutoSaveFailed
                            });
                        }
                    } else {
                        $.ajax({
                            url: self.Action(),
                            method: "POST",
                            data: ko.toJS(self.details),
                            success: AutoSaveSuccess,
                            error: AutoSaveFailed
                        });
                    }
                }
            } else {
                notify('Auto Save Failed: ' + validation.Message, 'Error');
            }
        }
    }
    function AutoSaveFailed(jqXHR) {
        vm.posting(false);
        ShowLoading(false);
        var msg = '';
        if (jqXHR.status === 500) {
            msg = 'Auto Save failed.'
        } else {
            msg = "Auto Save failed. Please check your connectivity.";
        }
        notify(msg, 'Error');
    }
    function AutoSaveSuccess(data) {
        vm.posting(false);
        if (data.Created === true || data.Updated === true || data.Saved === true || data.DraftCreated === true) {
            if (data.Created) {
                self.details.TrainingTestId(data.TrainingTestId);
                self.Action(self.links().AutoSave);
            }
            if (data.DraftCreated) {
                Reconcile(data.Model);
            }
            CorrectQuestionNumbers();
            ShowLoading(false);
            var msg = "Auto Save Successful.";
            notify(msg, "Success");
        } else {
            AutoSaveFailed({ status: 500 });
        }

    }
   

    function cancelQuestionUpload(container, question) {
        question[container]().SubmitEvent.data().jqxhr.abort();
        self.details.concurrentUploads.remove(question[container]());
        deleteQuestionUpload(container, question);
    }
    function CanSave() {
        if (self.details.concurrentUploads().length < 1) {
            $('#Save').removeClass('disabled');
            $('#SaveAndExit').removeClass('disabled');
            return true;
        } else {
            if (!$('#Save').hasClass('disabled'))
                $('#Save').addClass('disabled');
            if (!$('#SaveAndExit').hasClass('disabled'))
                $('#SaveAndExit').addClass('disabled');
            return false;
        }
    }
    function checkFileType(fileName) {
        var ext = fileName.substring(fileName.lastIndexOf('.') + 1).toUpperCase();
        if (ext.toUpperCase() === "PNG" || ext.toUpperCase() === "JPEG" || ext.toUpperCase() === "JPG" || ext.toUpperCase() === "GIF" || ext.toUpperCase() === "BMP") {
            return true;
        }
        else if (ext.toUpperCase() === "MP4") {
            return true;
        }
        else if (ext.toUpperCase() === "MP3"){
            return true;
        }else {
            notif({
                msg: "<b>Error :</b> Please select a valid file type to upload.",
                type: "error"
            });
            return false;
        }
    }
    function clearDefaultValueOnClick(value) {
        var v = null;
        if (ko.isObservable(value.TestQuestion))
            v = value.TestQuestion;
        if (ko.isObservable(value.Option))
            v = value.Option;
        if (ko.isObservable(v))
            if(v() === '[Question]'|| v() === '[Option]'|| v() === '[Introduction]')
                v(null);
    }
    function clearUpload(container, upload) {
        container.Errors.remove(upload);
    }

    function deleteQuestionUpload(container, question) {
        question[container](null);
    }

    function EnsureThreeAnswersPerQuestion() {
        $.each(self.details.QuestionsList(), function () {
            var question = this;
            if (question.TestAnswerList().length - 1 < 2) {
                for (var i = question.TestAnswerList().length - 1 ; i < 2 ; i++) {
                    generateAnswer(question.TrainingTestQuestionId()).then(function (data) {
                        question.TestAnswerList.push(data);
                        EnsureCorrectAnswerSelectedForEachQuestion();
                    });
                }
            }
        });
    }
    function EnsureCorrectAnswerSelectedForEachQuestion() {
        $.each(self.details.QuestionsList(), function () {
            var id = findCorrectAnswerId(this);
            if (!id && this.TestAnswerList().length > 0 ) {
                this.CorrectAnswerId(this.TestAnswerList()[0].TestAnswerId());
            }
        });
    };
    function findCorrectAnswerId(question) {
        var id = null;
        $.each(question.TestAnswerList(), function () {
            if (this.TestAnswerId() === question.CorrectAnswerId())
                id = this.TestAnswerId();
        });
        if (id)
            question.CorrectAnswerId(id);
        return id;
    };

    function FileUploadOptions() {
        return {
            showProgress: false,
            url: self.links().uploadLink,
            before: onBeforeUploadEventHandler,
            success: onUploadSuccessEventHandler,
            error: onUploadErrorEventHandler,
            onProgress: onProgressEventHandler,
            autoupload: true
        };
    }

    function gallaryObject(upload) {
        return {
            title: upload.Name(),
            href: upload.Url(),
            type: upload.Type()
        };
    }
    function generateAnswer(uniqueId) {
        var promise = new Promise(function (resolve, reject) {
            var answer = {};
            answer.Option = new ko.observable('[Option]');
            answer.Correct = new ko.observable("off");
            answer.TrainingQuestionId = new ko.observable(uniqueId);
            answer.TestAnswerId = new ko.observable();
            GetId().then(function (data) {
                answer.TestAnswerId(data);
                resolve(answer);
            });
        });
        return promise;
    }
    function GetId() {
        return $.ajax({
            method: 'GET',
            url: self.links().getId,
            cache : false,
            error: function (error) {
                notify("Please check your Connectivity.", 'Error');
            }
        });
    }
    function getScaledSize(total) {
        var totalString = "";
        var unit = { KB: ' KB', MB: ' MB', GB: ' GB' };
        var totalBytes = total;
        totalString = (totalBytes / 1000).toPrecision(5) + unit.KB;
        if (total / 1000 > 1000) {
            totalString = (totalBytes / (1000000)).toPrecision(5) + unit.MB;
        }
        if (total / (1000000) > 1000) {
            totalString = (totalBytes / 1000000000).toPrecision(5) + unit.GB;
        }
        return totalString;
    }
    function getSelectedId(data) {
        var selected = null;
        $.each(data, function () {
            if (this.Selected) {
                if (this.Selected === true) {
                    selected = this.Value;
                }
            }
        });
        return selected;
    }
    function GetTrophies() {
        $.ajax({
            url: links.trophyLink,
            type: 'GET',
            success: function (data) {
                self.details.TrophyList(data);
            }
        });
    }

    function hideTrophyModal() {
        $('#TrophyModal').hide();
    }

    function notify(message, type) {
        if (type.toLowerCase() === 'error') {
            notif({
                type: type.toLowerCase(),
                msg: '<b>' + type + ' - </b>' + message
            });
        }
        if (type.toLowerCase() === 'success') {
            notif({
                type: type.toLowerCase(),
                msg: message
            });
        }
    }

    function onBeforeUploadEventHandler(element, fileName, valueProperty) {
        if (checkFileType(fileName)) {
            var uploadViewModel = new uploadObject();
            uploadViewModel.Name(fileName.replace('C:\\fakepath\\', ''));
            uploadViewModel.InProcess(true);
            uploadViewModel.Progress('0%');
            valueProperty(uploadViewModel);
            self.details.concurrentUploads().push(valueProperty);
            self.details.CanSave()();
            return true;
        }
        return false;
    }
    function onProgressEventHandler(event, position, total, percentage, fileName, valueProperty, submitEvent, valueAccessor) {
        valueProperty().Size(total);
        valueProperty().Progress(percentage + '%');
        valueProperty().SubmitEvent = submitEvent;
    }
    function onUploadErrorEventHandler(valueProperty, container) {
        self.details.concurrentUploads.remove(valueProperty);
        container.Errors.push(ko.mapping.fromJS({ Name: valueProperty().Name() }));
        valueProperty(null);
    }
    function onUploadSuccessEventHandler(data, textStatus, xhr, valueProperty, valueAccessor) {
        valueProperty().InProcess(false);
        valueProperty().Progress('100%');
        self.details.concurrentUploads.remove(valueProperty);
    }
    function Reconcile(model) {
        self.details.TrainingTestId(model.TrainingTestId);
        self.details.DraftStatus(model.DraftStatus);
        self.details.ActiveStatus(model.ActiveStatus);
        self.details.ActivePublishDate(model.ActivePublishDate);
        self.details.DraftEditDate(model.DraftEditDate);
        self.details.ParentTrainingTestId(model.ParentTrainingTestId);
        self.details.CreateDate(model.CreateDate);
        self.details.CreatedBy(model.CreatedBy);
        $.each(self.details.QuestionsList(), function (index) {
            var q = this;
            var mq = model.QuestionsList[index];
            q.TrainingTestId(mq.TrainingTestId);
            q.TrainingTestQuestionId(mq.TrainingTestQuestionId);
            q.TestQuestionNumber(index);
            q.CorrectAnswerId(mq.CorrectAnswerId);
            $.each(q.TestAnswerList(), function (index) {
                var a = this;
                if (mq.TestAnswerList && mq.TestAnswerList.length >= index) {
                    var ma = mq.TestAnswerList[index];
                    a.TrainingQuestionId(ma.TrainingQuestionId);
                    a.TestAnswerId(ma.TestAnswerId);
                }                
            });
            if (ko.isObservable(q.ImageContainer) && q.ImageContainer() !== null) {
                q.ImageContainer().Id(mq.ImageContainer.Id);
            }
            if (ko.isObservable(q.VideoContainer) && q.VideoContainer() !== null) {
                q.VideoContainer().Id(mq.VideoContainer.Id);
            }
            if (ko.isObservable(q.AudioContainer) && q.AudioContainer() !== null) {
                q.AudioContainer().Id(mq.AudioContainer.Id);
            }
        });
    }
    function CorrectQuestionNumbers() {
        $.each(self.details.QuestionsList(), function (index) {
            var q = this;
            q.TestQuestionNumber(index + 1);
        });
    }
    function removeQuestion() {
        self.details.QuestionsList.remove(this);
    }

    function Save() {
        if (self.details.SelectedTrainingGuideId() === null) {
            notify('Save Failed: Please link a Playbook to this Test.', 'Error');
        } else if (self.details.validationErrors().length > 0) {
            self.details.validationErrors.showAllMessages(true);
            notify('Save Failed: Required Information Missing.', 'Error');
        }
        else {
            var validation = validate();
            if (validation.Error === false) {
                ShowLoading(true);
                if (self.details.ViewMode() === 0) {
                    if (self.details.TrainingTestId() === '00000000-0000-0000-0000-000000000000') {
                        $.ajax({
                            url: self.Action(),
                            method: "POST",
                            data: ko.toJS(self.details),
                            success: SaveSuccess,
                            error: SaveFailed
                        });
                    } else {
                        $.ajax({
                            url: self.links().AutoSave,
                            method: "POST",
                            data: ko.toJS(self.details),
                            success: SaveSuccess,
                            error: SaveFailed
                        });
                    }
                } else {
                    $.ajax({
                        url: self.Action(),
                        method: "POST",
                        data: ko.toJS(self.details),
                        success: SaveSuccess,
                        error: SaveFailed
                    });
                }
            } else {
                notify('Save Failed: ' + validation.Message, 'Error');
            }
        }
    }
    function SaveFailed(jqXHR) {
        ShowLoading(false);
        var msg = '';
        if (jqXHR.status === 500) {
            msg = 'Save failed.'
        } else {
            msg = "Save failed. Please check your connectivity.";
        }
        notify(msg, 'Error');

    }
    function SaveSuccess(data) {
        if (data.Created === true || data.Updated === true || data.Saved === true || data.DraftCreated === true) {
            if (data.Created) {
                self.details.TrainingTestId(data.TrainingTestId);
                self.Action(self.links().AutoSave);
            }
            if (data.DraftCreated) {
                Reconcile(data.Model);
            }
            CorrectQuestionNumbers();
            ShowLoading(false);
            var msg = "Save Successful.";
            notify(msg, "Success");
        } else {
            SaveFailed({ status: 500 });
        }

    }
    function SaveAndExit() {
        if (self.details.SelectedTrainingGuideId() === null) {
            notify('Save Failed: Please link a Playbook to this Test.', 'Error');
        } else if (self.details.validationErrors().length > 0) {
            self.details.validationErrors.showAllMessages(true);
            notify('Save Failed: Required Information Missing.', 'Error');
        }
        else {
            var validation = validate();
            if (validation.Error === false) {
                ShowLoading(true);
                if (self.details.ViewMode() === 0) {
                    if (self.details.TrainingTestId() === '00000000-0000-0000-0000-000000000000') {
                        $.ajax({
                            url: self.Action(),
                            method: "POST",
                            data: ko.toJS(self.details),
                            success: SaveAndExitSuccess,
                            error: SaveFailed
                        });
                    } else {
                        $.ajax({
                            url: self.links().AutoSave,
                            method: "POST",
                            data: ko.toJS(self.details),
                            success: SaveAndExitSuccess,
                            error: SaveFailed
                        });
                    }
                } else {
                    $.ajax({
                        url: self.Action(),
                        method: "POST",
                        data: ko.toJS(self.details),
                        success: SaveAndExitSuccess,
                        error: SaveFailed
                    });
                }
            } else {
                notify('Save Failed: ' + validation.Message, 'Error');
            }
        }
    }
    function SaveAndExitSuccess(data) {
        if (data.Created === true || data.Updated === true || data.Saved === true || data.DraftCreated === true) {
            if (data.Created) {
                self.details.TrainingTestId(data.TrainingTestId);
                self.Action(self.links().AutoSave);
            }
            if (data.DraftCreated) {
                Reconcile(data.Model);
            }
            CorrectQuestionNumbers();
            ShowLoading(false);
            var msg = "Save Successful.";
            notify(msg, "Success");
            window.setTimeout(function () { window.location.href = self.links().index; }, 1000);
        } else {
            SaveFailed({ status: 500 });
        }
    }

    function SetDefaultExpiryDate() {
        if (self.details.TestExpiryDate() === null) {
            var minDate = new Date();
            var month = minDate.getMonth() + 1;
            self.details.TestExpiryDate(minDate.setMonth(month));
        }
        if (viewBag.Action === 'Create') {
            self.details.IsTestExpiryDate(false);
        }
    }
    function SetSelectedTrainingGuide() {
        if (!self.details.SelectedTrainingGuideId())
            self.details.SelectedTrainingGuideId(getSelectedId(trainingGuides));
    }
    function setTrophyName(data) {
        self.details.TrophyName(data.FileName);
        self.details.TrophyDataBase64Encoded(data.Content);
        hideTrophyModal();
    }
    function SetUpAutoSave() {
        window.setInterval(AutoSave, self.AutoSaveFrequency);
    }
    function SetUpCurrentAction() {
        if (self.details.ViewMode) {
            if (self.details.ViewMode() === 0) {
                self.Action(self.links().Create);
            } else if (self.details.ViewMode() === 1) {
                self.Action(self.links().EditTrainingTest);
            }
        }
    }
    function SetUpQuestionUploads() {
        $.each(self.details.QuestionsList(), function () {
            if (!ko.isObservable(this.ImageContainer)) {
                var container = this.ImageContainer;
                this.ImageContainer = new ko.observable(container);
            }
            if (!ko.isObservable(this.VideoContainer)) {
                container = this.VideoContainer;
                this.VideoContainer = new ko.observable(container);
            }
            if (!ko.isObservable(this.AudioContainer)) {
                container = this.AudioContainer;
                this.AudioContainer = new ko.observable(container);
            }
        });
    }
    function SetUpTour() {
        tour.steps= [
              {
                  target: 'TestTitle',
                  title: 'Title',
                  content: 'Enter a name for your test.',
                  placement: 'top',
                  xOffset: 'center',
                  arrowOffset: 'center'
              },

             {
                 target: 'PassMarks',
                 placement: 'top',
                 title: 'Reqiured Pass Mark %',
                 content: 'Enter your required pass mark percentage.',
                 xOffset: 'center',
                 arrowOffset: 'center'
             },
             {
                 target: 'SelectedTrainingGuideId',
                 placement: 'top',
                 title: 'Playbook Link',
                 content: 'This displays the playbook that the test is linked to.',
                 xOffset: 'center',
                 arrowOffset: 'center'
             },

             {
                 target: 'TestDuration',
                 placement: 'top',
                 title: 'Test Duration',
                 content: 'Enter the number of minutes you will allow the test to run for.',
                 xOffset: 'center',
                 arrowOffset: 'center'
             },
              {
                  target: 'TestExpiryDate',
                  placement: 'top',
                  title: 'Test expiry date',
                  content: 'Click here to set an expiration for your test. Users will be sent a text message informing them that they must complete the test by the specified date.',
                  xOffset: 'center',
                  arrowOffset: 'center',
                  width: 240
              },
                {
                    target: 'TestExpiry',
                    placement: 'top',
                    title: 'Test Expire',
                    content: 'Click here to prevent your test form expiring.',
                    xOffset: 'center',
                    arrowOffset: 'center',
                    width: 240
                },

                {
                    target: 'TestIntroduction',
                    placement: 'top',
                    title: 'Introduction',
                    content: 'Enter a brief description of your test in this box - i.e. who should take the test and what content they should have covered before taking the test.',
                    xOffset: 'center',
                    arrowOffset: 'center',
                    width: 240
                },

             {
                 target: 'AddQuestion',
                 placement: 'top',
                 title: 'Add Question',
                 content: 'Click here to add questions to your test.',
                 xOffset: 'center',
                 arrowOffset: 'center',
                 width: 150
             },
                {
                    target: 'Save',
                    placement: 'top',
                    title: 'Save',
                    content: 'Click here to save your test.',
                    xOffset: 'center',
                    arrowOffset: 'center',
                    width: 240
                },
        ];
    }
    function ShowLoading(bool) {
        var busy = true;
        if(bool)
            while (busy) {
                busy = Busy();
            }
        if (bool) {
            $('#LoadingImageDiv').show();
        }
        else {
            $('#LoadingImageDiv').hide();
        }
    }
    function Busy() {
        return $('#LoadingImageDiv').css('display') !== 'none';
          
    }
    function showTrophyModal() {
        $('#TrophyModal').show();
    }
    function simulateUploadClick(data,event) {
        if (event.target.src.indexOf('AudioIcon') > -1 && ko.unwrap(data.AudioContainer)) {
            notif({
                msg: "<b>Error :</b> Only one audio upload allowed per question.",
                type: "error"
            });
            return false;
        } 
        else if (event.target.src.indexOf('imgDoc') > -1 && data.ImageContainer()) {
            notif({
                msg: "<b>Error :</b> Only one image upload allowed per question.",
                type: "error"
            });
            return false;
        } else if (event.target.src.indexOf('video') > -1 && data.VideoContainer()) {
            notif({
                msg: "<b>Error :</b> Only one video upload allowed per question.",
                type: "error"
            });
            return false;
        }
        else {
            $(event.target).parent().find('input').click();
        }
    }

    function toDisplayString(value, maxCharacters) {
        var displayString = [];
        var string = '';
        var index = 0;
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

    function uploadObject() {
        return {
            Name: new ko.observable(),
            InProcess: new ko.observable(),
            Progress: new ko.observable(),
            Size: new ko.observable(),
            ThumbnailUrl: new ko.observable(),
            DeleteType: new ko.observable(),
            DeleteUrl: new ko.observable(),
            QuestionNumber: new ko.observable(),
            Type: new ko.observable(),
            Url: new ko.observable()
        };
    }

    function validate(skipIntroduction) {
        var questionValidationResult = answerValidation();
        var error = false;
        var message = '';
        if (self.details.QuestionsList().length < 1) {
            error = true;
            message = "Please Create At Least One Question.";
        }
        if (self.details.EnableMaximumTestRewriteFunction() === true && (self.details.MaximumRewrites() === null || self.details.MaximumRewrites() === 0)) {
            error = true;
            message = 'Specify maximum number of test rewrites allowed (Default None).';
            $('#MaximumRewrites').focus(100);
        }
        if (!skipIntroduction) {
            var e = ContentTools.EditorApp.get();
            if (e.getState() === 'editing')
                e.stop(true);
            else
                e.save(true);
            if (self.details.IntroductionContent() === undefined || self.details.IntroductionContent() === null || self.details.IntroductionContent() === '') {
                error = true;
                message = 'Please Enter a Test Introduction';
                $('#IntroductionContent').focus(100);
            }
        }
        if (error === false) {
            $.each(questionValidationResult, function () {
                if (this.Valid === false) {
                    error = true;
                    message = this.ErrorMessage;
                    $('#' + this.QuestionNumber).focus(1000);
                    return false;
                }
            });
        }
        return { Error: error, Message: message };
    }
  
 
    
  
   
   
   
   
    
   
    
  
   
   
   
    
    
  
   
}