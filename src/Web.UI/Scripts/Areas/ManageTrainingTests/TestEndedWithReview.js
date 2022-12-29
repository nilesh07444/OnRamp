function Controller(app, model, viewBag, trainingGuides, links) {
    var self = this;
    self.app = app;
    self.details = ko.mapping.fromJS(model);
}