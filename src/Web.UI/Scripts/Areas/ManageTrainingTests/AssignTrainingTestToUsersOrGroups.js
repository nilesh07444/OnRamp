function Controller(app, model, links) {
    var self = this;
    self.app = app;
    self.details = ko.mapping.fromJS(model);
    self.selectedViewMode = new ko.observable('0');
    self.selectGroup = new ko.observable(2);
    self.viewMode = new ko.observableArray([{ Text: "Groups", Value: 'Group' }, { Text: "Users", Value: 'User' }]);
    self.filterGroupsId = new ko.observableArray();
    self.searchUsersFilter = new ko.observable();
    self.unallocatedOnly = new ko.observable(false).extend({ notify: 'always' });
    self.searchTrainingTests = new ko.observable();
    self.searchGroups = new ko.observable();
    self.filteredUsers = ko.computed(function () {
        var filterGroupsId;
        var filterUserSearch;
        var unallocatedOnly;
        if (self.filterGroupsId().length > 0) {
            filterGroupsId = self.filterGroupsId();
        }
        if (self.searchUsersFilter()) {
            filterUserSearch = self.searchUsersFilter().toLowerCase();
        }
        if (self.unallocatedOnly()) {
            unallocatedOnly = true;
        }
        if (!filterGroupsId && !filterUserSearch && !unallocatedOnly) {
            return self.details.Users();
        } else {
            return ko.utils.arrayFilter(self.details.Users(), function (u) {
                var shouldShow = true;
                if (filterGroupsId) {
                    shouldShow = false;
                    $.each(filterGroupsId, function () {
                        if (this.toLowerCase) {
                            if (u.GroupId().toLowerCase() === this.toLowerCase()) {
                                shouldShow = true;
                            }
                        }
                    });
                }
                if (filterUserSearch && shouldShow)
                    shouldShow = u.FullName().toLowerCase().indexOf(filterUserSearch) !== -1;
                if (unallocatedOnly && shouldShow) {
                    var allocated = false;
                    $.each(self.details.TrainingTests(), function () {
                        if (this.Selected()) {
                            var allocatedUsers = this.AssignedUserIds();
                            $.each(allocatedUsers, function () {
                                if (this == u.Id())
                                    allocated = true;
                            });
                        }
                    });
                    shouldShow = !allocated;
                }
                return shouldShow;
            });
        }
    });
    self.filteredTrainingTests = ko.computed(function () {
        var filter;
        if (self.searchTrainingTests()) {
            filter = self.searchTrainingTests().toLowerCase();
        }
        if (!filter) {
            return self.details.TrainingTests();
        } else {
            return ko.utils.arrayFilter(self.details.TrainingTests(), function (g) {
                return g.Title().toLowerCase().indexOf(filter) !== -1;
            });
        }
    });
    self.filteredGroups = ko.computed(function () {
        var filter;
        if (self.searchGroups()) {
            filter = self.searchGroups().toLowerCase();
        }
        if (!filter) {
            return self.details.Groups();
        } else {
            return ko.utils.arrayFilter(self.details.Groups(), function (g) {
                return g.Name().toLowerCase().indexOf(filter) !== -1;
            });
        }
    });
    self.selectedUsers = new ko.observableArray();
    self.selectedGroups = new ko.observableArray();
    self.selectedTrainingTests = new ko.observableArray();
    self.AddGroup = function (group) {
        if (group.Selected())
            self.selectedGroups.push(group);
        else
            self.selectedGroups.remove(group);
        return true;
    }
    self.AddUser = function (user) {
        if (user.Selected())
            self.selectedUsers.push(user);
        else
            self.selectedUsers.remove(user);
        return true;
    }
    self.AddTrainingTest = function (trainingTest) {
        if (trainingTest.Selected())
            self.selectedTrainingTests.push(trainingTest);
        else
            self.selectedTrainingTests.remove(trainingTest);
        return true;
    }
    self.showAssign = new ko.computed(function () {
        return (self.selectedGroups().length > 0 || self.selectedUsers().length > 0) && self.selectedTrainingTests().length > 0;
    });
    self.AssignTrainingTests = function () {
        self.Dispatch(self.CreateCommand(false));
    }
    self.ClearSelection = function () {
        $.each(self.details.Users(), function () {
            this.Selected(false);
        });
        $.each(self.details.Groups(), function () {
            this.Selected(false);
        });
        $.each(self.details.TrainingTests(), function () {
            this.Selected(false);
        });
        self.selectedGroups.removeAll();
        self.selectedUsers.removeAll();
        self.selectedTrainingTests.removeAll();
    }
    self.Dispatch = function (model) {
        $('#LoadingImageDiv').show();
        $.ajax({
            type: 'POST',
            url: links.AssignTestsToUsersOrGroup,
            data: model,
            success: function () {
                $('#LoadingImageDiv').hide();
                self.NotifySuccess('Tests assigned successfully.')
            },
            error: function () {
                $('#LoadingImageDiv').hide();
                self.NotifyFailure('Assignment failed.')
            }
        });
    }
    self.CreateCommand = function () {
        var command = {
            SelectedOption: self.selectedViewMode(),
            TrainingTests: self.GetAllSelectedTrainingTests()
        }
        if (self.selectedViewMode() === 'User') {
            command.CustomerStandardUsers = self.GetAllSelectedUsers();
        } else {
            command.Groups = self.GetAllSelectedGroups();
        }
        return command;
    }
    self.GetAllSelectedTrainingTests = function () {
        var model = [];
        $.each(self.selectedTrainingTests(), function () {
            model.push({
                TrainingTestId: this.Id()
            });
        });
        return model;
    }
    self.GetAllSelectedUsers = function () {
        var model = [];
        $.each(self.selectedUsers(), function () {
            model.push({
                Id: this.Id()
            });
        });
        return model;
    }
    self.GetAllSelectedGroups = function () {
        var model = [];
        $.each(self.selectedGroups(), function () {
            model.push({
                GroupId: this.Id()
            });
        });
        return model;
    }
    self.NotifySuccess = function (msg) {
        notif({
            msg: msg,
            type: "success"
        });
        window.setTimeout(function () { window.location.reload() }, 1500);
    }
    self.NotifyFailure = function (msg) {
        notif({
            msg: "<b>Error :</b> " + msg,
            type: "error"
        });
        window.setTimeout(function () { window.location.reload() }, 1500);
    }
}