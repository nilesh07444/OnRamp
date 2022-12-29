function Controller(app, model, links) {
    var self = this;
    self.app = app;
    self.details = ko.mapping.fromJS(model);
    self.selectedTest = new ko.observable(model.SelectedTestId);
    self.showData = new ko.observable(false);
    self.generate = function () {
        if (self.selectedTest())
            window.location.href = links.FocusAreaReportQuery + "?TestId=" + self.selectedTest();
    };
}