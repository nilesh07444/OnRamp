function Controller(app, model, viewBag, links) {
    var self = this;
    self.action = ko.observable(viewBag.Action);
    self.app = app;
    self.details = ko.mapping.fromJS(model);
    self.links = links;
    self.selectedIcon = new ko.observable();
    self.changeSelection = _changeSelection;
    self.getOrCreateIcon = _getOrCreateIcon;
    self.simulateUploadClick = _simulateUploadClick;
    self.clearUpload = _clearUpload;
    self.fileuploadOptions = _fileUploadOptions();
    self.save = _save;
    self.getUrl = _getUrl;
    self.details.Name.extend({ required: true });
    function _getUrl(icon) {
        if (ko.isObservable(icon)) {
            if (!icon())
                return links.defaultImageUrl;
            else {
                return icon().Url();
            }
        }
        else
            return icon.Url();
    }
    function _showLoading(bool) {
        if (bool)
            $('#LoadingImageDiv').show();
        else
            $('#LoadingImageDiv').hide();
    }
    function _changeSelection(iconType) {
        return self.getOrCreateIcon(iconType);
    }
    function _getOrCreateIcon(iconType) {
        var match = null;
        $.each(self.details.Icons(), function () {
                if (this.IconType() == iconType.Value())
                    match = this;
        });
        if (!match) {
            match = ko.mapping.fromJS({
                Id: null,
                IconType: iconType.Value()
            });
            match.UploadModel = new ko.observable();
            self.details.Icons.push(match);
        }
        match.Input = new ko.observable();
        match.Name = new ko.observable(iconType.Text());
        if (!ko.isObservable(match.UploadModel))
            match.UploadModel = new ko.observable(match.UploadModel);
        self.selectedIcon(match);
        return match;
    }
    function _simulateUploadClick() {
        $('#uploadInput').click();
    }
    function _clearUpload(icon) {
        icon.UploadModel(ko.mapping.fromJS({
            Id: null,
            Data: [],
            Name: null,
            Type: null,
            ContentType: null,
            Url:links.defaultImageUrl
        }));
    }
    function _fileUploadOptions() {
        return {
            showProgress: false,
            url: links.uploadLink,
            before: _onBeforeUploadEventHandler,
            success: _onUploadSuccessEventHandler,
            error: _onUploadErrorEventHandler,
            autoupload: true
        };
    }
    function _onBeforeUploadEventHandler(element, fileName, valueProperty) {
        _showLoading(true);
        return true;

    }
    function _onUploadSuccessEventHandler(data,textStatus,xhr,valueProperty) {
        _showLoading(false);
    }
    function _onUploadErrorEventHandler(valueProperty) {
        _showLoading(false);
    }
    function _save() {
        $.ajax({
            method: 'POST',
            url: links.save,
            data: ko.toJS(self.details),
            success: _saveSuccess,
            error: _saveError
        });
    }
    function _saveSuccess(data,code) {
        notif({
            msg: "<b>Success :</b> Saved.",
            type: "success",
        });
        window.location.href = links.index;
    }
    function _saveError(data,statusCode) {
        notif({
            msg: "<b>Failed :</b> Please try again.",
            type: "error",
        });
    }
    return self;    
};