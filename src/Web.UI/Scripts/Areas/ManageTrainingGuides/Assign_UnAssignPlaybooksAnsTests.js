function Controller(app, model, viewBag, links) {
    var self = this;
    self.app = app;
    self.details = ko.mapping.fromJS(model);
    self.chosenPlaybook = new ko.observable();
    self.chosenTest = new ko.observable();
    self.TrainingGuideHasLinkedTest = new ko.observable();
    self.SelectAllUsers = new ko.observable(false);
    self.SelectAllGroups = new ko.observable(false);
    self.SelectAllPlaybooks = new ko.observable(false);
    self.SelectAllTests = new ko.observable(false);
    self.searchUsers = new ko.observable();
    self.searchGroups = new ko.observable();
    self.SelectedUsers = new ko.observableArray();
    self.SelectedGroups = new ko.observableArray();
    
    self.showAssign = ko.computed(ShowAssign);
    self.filteredUsers = ko.computed(FilterUsers);
    self.filteredGroups = ko.computed(FilterGroups);

    self.NotifySuccess = notifySuccess;
    self.NotifyFailure = notifyFailure;
    self.AssignPlaybookToUser = AssignPlaybookToUser;
    self.AssignPlaybookToGroup = AssignPlaybookToGroup;
    self.AssignTestToUser = AssignTestToUser;
    self.AssignTestToGroup = AssignTestToGroup;
    self.SubmitCommand = SubmitCommand;

    self.chosenPlaybook.subscribe(GetTrainingGuideDetails);
    self.chosenTest.subscribe(GetDetailsTest);
    self.SelectAllPlaybooks.subscribe(SelectAllPlaybooksAction);
    self.SelectAllTests.subscribe(SelectAllTestsAction);
    self.details.AssignMode.subscribe(InitView);
    self.disabled = disabled;
    self.toggleForce = function (user) {
        user.ForceRetake(!user.ForceRetake());
    }
    function disabled(user) {
        if (user.PassedPreviousVersionOfTest())
            user.ForceRetake(true);
        return user.PassedTest();
    }
    function AssignCommand(mode, id, playbook, test,force) {
        var self = {
            AssignMode: mode,
            Id: id,
            Playbook: playbook,
            Test: test,
            ForceRetake : force ? force : false
        };
        return self;
    }

    function FilterUsers() {
        var filter;
        if (self.searchUsers()) {
            filter = self.searchUsers().toLowerCase();
        }
        if (!filter) {
            return self.details.ManageableUsers();
        } else {
            return ko.utils.arrayFilter(self.details.ManageableUsers()(), function (u) {
                return u.User.Name().toLowerCase().indexOf(filter) !== -1;
            });
        }
    }
    function FilterGroups() {
        var filter;
        if (self.searchGroups()) {
            filter = self.searchGroups().toLowerCase();
        }
        if (!filter) {
            return self.details.ManageableGroups();
        } else {
            return ko.utils.arrayFilter(self.details.ManageableGroups()(), function (u) {
                return u.Group.Name().toLowerCase().indexOf(filter) !== -1;
            });
        }
    }
    function ShowAssign() {
        return self.SelectedUsers().length > 0 || self.SelectedGroups().length > 0;
    }

    function SubmitCommand() {
        if (self.details.AssignMode() == 0)
            AssignToUsersCommand();
        else
            AssignToGroupsCommand();
    }
    function AssignToUsersCommand(){
        UpdateServer(self.SelectedUsers);
    }
    function AssignToGroupsCommand(){
        UpdateServer(self.SelectedGroups);
    }
    function UpdateServer(data) {
        ShowLoading(true);
        $.ajax({
            type: 'POST',
            url: links.Assign_Unassign,
            data: { AssignViewModels: ko.toJS(data), TrainingGuideId: ko.toJS(self.chosenPlaybook) , TrainingTestId : ko.toJS(self.chosenTest) },
            success: SaveSuccess,
            error: SaveError
        });
    }
    function ShowLoading(bool) {
        if (bool)
            $('#LoadingImageDiv').show();
        else
            $('#LoadingImageDiv').hide();
    }
    function SaveSuccess(data){
        if (data == true) {
            self.NotifySuccess('User updated successfully');
            return true;
        } else {
            self.NotifyFailure('Updating user failed');
        }
        ShowLoading(false);
    }
    function SaveError(error){
        self.NotifyFailure('Updating user failed');
        ShowLoading(false);
    }
    function notifySuccess(msg) {
        notif({
            msg: msg,
            type: "success"
        });
        window.setTimeout(function () { window.location.reload() }, 1500);
    }
    function notifyFailure(msg) {
        notif({
            msg: "<b>Error :</b> " + msg,
            type: "error"
        });
        window.setTimeout(function () { window.location.reload() }, 1500);
    }

    function InitView(newValue) {
        self.SelectedGroups.removeAll();
        self.SelectedUsers.removeAll();
        self.SelectAllUsers(false);
        self.SelectAllGroups(false);
        self.SelectAllPlaybooks(false);
        self.SelectAllTests(false);
        GetTrainingGuideDetails();
    }
    function AssignPlaybookToUser(user) {
        var entry = new AssignCommand(self.details.AssignMode(), user.User.Id(), user.AssignedTrainingGuide(), user.AssignedTrainingTest(),user.ForceRetake());
        CreateOrUpdateEntry(entry, self.SelectedUsers);
        return true;
    }
    function AssignPlaybookToGroup(group) {
        var entry = new AssignCommand(self.details.AssignMode(), group.Group.Id(), group.AllMemebersAssignedTrainingGuide(), group.AllMembersAssignedTest());
        CreateOrUpdateEntry(entry, self.SelectedGroups);
        return true;
    }
    function AssignTestToUser(user) {
        var entry = new AssignCommand(self.details.AssignMode(), user.User.Id(), user.AssignedTrainingGuide(), user.AssignedTrainingTest(),user.ForceRetake());
        CreateOrUpdateEntry(entry, self.SelectedUsers);
        return true;
    }
    function AssignTestToGroup(group) {
        var entry = new AssignCommand(self.details.AssignMode(), group.Group.Id(), group.AllMemebersAssignedTrainingGuide(), group.AllMembersAssignedTest());
        CreateOrUpdateEntry(entry, self.SelectedGroups);
        return true;
    }
    function CreateOrUpdateEntry(entry, array) {
        var found = false;
        var orginalEntry = 'undefined';
        $.each(array(), function () {
            var original = this;
            if (original.Id == entry.Id) {
                found = true;
                if (entry.Playbook !== undefined)
                    original.Playbook = entry.Playbook;
                if (entry.Test !== undefined)
                    original.Test = entry.Test;
            }
        });
        if (!found)
            array.push(entry);
        if (orginalEntry)
            array.remove(orginalEntry);
    }
    function GetTrainingGuideDetails(newValue) {
        if (self.details.FunctionalMode() == 0) {
            ShowLoading(true);
            $.ajax({
                type: 'POST',
                url: links.Assign_UnAssignPlaybooks,
                data: { SelectedTrainingGuideId: self.chosenPlaybook() },
                success: function (data) {
                    self.TrainingGuideHasLinkedTest(data.HasTest);
                    self.details.ManageableUsers(ko.mapping.fromJS(data.ManageableUsers));
                    self.details.ManageableGroups(ko.mapping.fromJS(data.ManageableGroups));
                    _adjustPageFooter();
                },
                complete: function () {
                    ShowLoading(false);
                }
            });
        }
    }
    function GetDetailsTest(newValue) {
        if (self.details.FunctionalMode() == 1) {
            ShowLoading(true);
            $.ajax({
                type: 'POST',
                url: links.Assign_UnAssignTests,
                data: { SelectedTrainingTestId: self.chosenTest(), FunctionalMode: self.details.FunctionalMode() },
                success: function (data) {
                    self.TrainingGuideHasLinkedTest(data.HasTest);
                    self.details.ManageableUsers(ko.mapping.fromJS(data.ManageableUsers));
                    self.chosenPlaybook(data.TrainingGuideId);
                    self.details.ManageableGroups(ko.mapping.fromJS(data.ManageableGroups));
                    _adjustPageFooter();
                },
                complete: function () {
                    ShowLoading(false);
                }
            });
        }
    }
    function SelectAllPlaybooksAction(newValue) {
        if (newValue) 
            AddAllPlaybooks();
        else {
            RemoveAllPlaybooks();
        }
    }
    function SelectAllTestsAction(newValue) {
        if (newValue)
            AddAllTests();
        else
            RemoveAllTests();
    }
    function AddAllUsers(playbook, test) {
        if (ko.isObservable(self.filteredUsers())) {
            AddAllCore(self.filteredUsers()(), playbook, test);
        } else {
            AddAllCore(self.filteredUsers(), playbook, test);
        }
    }
    function RemoveAllUsers(playbook,test) {
        if (ko.isObservable(self.filteredUsers())) {
            RemoveAllCore(self.filteredUsers()(), playbook, test);
        } else {
            RemoveAllCore(self.filteredUsers(), playbook, test);
        }
    }
    function AddAllCore(array, playbook, test) {
        $.each(array, function () {
            if (self.details.AssignMode() == 0) {
                var mu = ko.isObservable(this) ? this() : this;
                var entry = new AssignCommand(self.details.AssignMode(), mu.User.Id(), playbook, test);
                CreateOrUpdateEntry(entry, self.SelectedUsers);
                if (playbook)
                    mu.AssignedTrainingGuide(true);
                if (test)
                    if (self.TrainingGuideHasLinkedTest())
                        mu.AssignedTrainingTest(true);
            } else {
                var mg = this;
                var entry = new AssignCommand(self.details.AssignMode(), mg.Group.Id(), playbook, test);
                if (playbook)
                    mg.AllMemebersAssignedTrainingGuide(true);
                if (test)
                    if (self.TrainingGuideHasLinkedTest())
                        mg.AllMembersAssignedTest(true);
            }
        });
    }
    function RemoveAllCore(array, playbook, test) {
        $.each(array, function () {
            if (self.details.AssignMode() == 0) {
                var mu = this;
                var entry = new AssignCommand(self.details.AssignMode(), mu.User.Id(), playbook ? false : playbook, test == true ? false : test);
                CreateOrUpdateEntry(entry, self.SelectedUsers);
                if (playbook)
                    mu.AssignedTrainingGuide(false);
                if (test)
                    if (self.TrainingGuideHasLinkedTest())
                        mu.AssignedTrainingTest(false);
            } else {
                var mg = this;
                var entry = new AssignCommand(self.details.AssignMode(), mg.Group.Id(), playbook ? false : playbook, test ? false : test);
                CreateOrUpdateEntry(entry, self.SelectedGroups);
                if (playbook)
                    mg.AllMemebersAssignedTrainingGuide(false);
                if (test)
                    if (self.TrainingGuideHasLinkedTest())
                        mg.AllMembersAssignedTest(false);
            }
        });
    }
    function AddAllGroups(playbook, test) {
        if (ko.isObservable(self.filteredGroups())) {
            AddAllCore(self.filteredGroups()(), playbook,test);
        } else {
            AddAllCore(self.filteredGroups(), playbook, test);
        }
    }
    function RemoveAllGroups(playbook,test) {
        if (ko.isObservable(self.filteredGroups())) {
            RemoveAllCore(self.filteredGroups()(),playbook,test);
        } else {
            RemoveAllCore(self.filteredGroups(), playbook, test);
        }
    }
    function AddAllPlaybooks() {
        if (self.details.AssignMode() == 0) {
            AddAllUsers(true);
        }
        else {
            AddAllGroups(true);
        }
    }
    function RemoveAllPlaybooks() {
        if (self.details.AssignMode() == 0) {
            RemoveAllUsers(true);
        }
        else {
            RemoveAllGroups(true);
        }
    }
    function AddAllTests() {
        if (self.details.AssignMode() == 0) {
            AddAllUsers(undefined, true);
        }
        else {
            AddAllGroups(undefined, true);
        }
    };
    function RemoveAllTests() {
        if (self.details.AssignMode() == 0) {
            RemoveAllUsers(undefined, true);
        }
        else {
            RemoveAllGroups(undefined, true);
        }
    }
}