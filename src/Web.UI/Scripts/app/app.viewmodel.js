var AppViewModel = function (dataModel) {
    var self = this;

    dataModel.app = this;
    self.applicationpath = '';
    self.autosearchdelay = 500;
    self.currencysymbol = 'R';

    self.ResolveUrl = function (url) {
        var u = url;

        if (self.applicationpath != '/') {
            var u = self.applicationpath + url;
        }

        return u;
    };

    self.resolveurl = self.ResolveUrl;

    self.getquerystringvalue = function (name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    };

    self.syncObservableObjects = function (objToUpdate, sourceObjectName, destObjectName) {
        var sourceObject = eval("objToUpdate." + destObjectName);
        for (var key in sourceObject) {
            //if (key.left(1) != "_") {
            var thisObj = eval("objToUpdate." + destObjectName + "." + key);
            if ($.isFunction(thisObj)) {
                eval("objToUpdate." + destObjectName + "." + key + "(objToUpdate." + sourceObjectName + "." + key + "())");
            } else {
                var objToUpdate2 = eval("objToUpdate." + destObjectName + "." + key);
                self.syncObservableObjects(objToUpdate, sourceObjectName + "." + key, destObjectName + "." + key);
            }
            //}
        }
    };

    function printpreview(print) {
        var location = window.location;
        location = location + "?print=" + print;
        window.open(location);
        return;
    }

    self.printing = function () {
        var key = 'print';
        key = key.replace(/[*+?^$.\[\]{}()|\\\/]/g, "\\$&"); // escape RegEx meta chars
        var match = location.search.match(new RegExp("[?&]" + key + "=([^&]+)(&|$)"));
        return match && decodeURIComponent(match[1].replace(/\+/g, " "));
    };

    self.print2 = function () {
        window.print();
    };

    self.print = function (model, e) {
        var t = model;
        if (typeof t == 'string' || t instanceof String) {
        } else {
            t = '';
        }

        var cl = 'print';
        if (t != '')
            cl = t;

        var printbuttons = $('.' + cl + ':first');

        if (e != null) {
            if ($(e.currentTarget).hasClass('print'))
                printpreview(t);
            else {
                printbuttons.trigger('click');
            }
        }
    };

    self.map = function (source, destination, configuration) {
        ko.mapping.fromJS(source, configuration || {}, destination);
    };

    self.bind = function (destination, source, configuration) {
        self.map(destination, source, configuration);
    };

    self.Message = new MessagingViewModel(this);
    self.message = self.Message;

    self.DataModel = dataModel;
    self.Data = dataModel;

    self.data = dataModel;
    self.datamodel = dataModel;
}

var MessagingViewModel = function (app) {
    var self = this;
    self.create = function (message, type) {
        var box = $('<div class="alert alert-' + type + ' alert-dismissable injected"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button></div>');
        console.log(messages);
        if (messages instanceof Array) {
            $.each(messages, function () {
                box.append('<li>' + this + '</li>');
            });
        } else {
            box.append(messages);
        }

        return box;
    };

    self.log = function (message, duration, type) {
        if (type == null)
            type = "log";

        if (!duration)
            duration = 15;

        return alertify.notify(message, type, duration);
    };

    self.show = function (message, duration, type) {
        return self.log(message, duration, type);
    };

    self.info = function (message, duration) {
        return self.log(message, duration, 'info');
    };

    self.warn = function (message, duration) {
        return self.log(message, duration, 'error');
    };

    self.error = function (message, duration) {
        if (duration)
            return self.log(message, duration, 'error');
        else {
            return self.alert('<span class="text-danger"><span class="glyphicon glyphicon-remove"></span>&nbsp;ERROR</span>', new moment().format('dddd, MMMM Do YYYY, HH:mm:ss') + '</br></br>' + message).set({ 'closableByDimmer': false });
        }
    };

    self.success = function (message, duration) {
        return self.log(message, duration, 'success');
    };

    self.alert = function (title, message) {
        return alertify.alert(title, message);
    };

    self.confirm = function (message, callback) {
        return self.confirmlabels(message, 'Yes', 'No', callback);
    };

    self.confirmlabels = function (message, yes, no, callback) {
        alertify.set({
            'labels': { ok: yes, cancel: no },
            'onok': callback,
            'title': 'Confirm'
        });
        return alertify.confirm(message);
    };

    self.yesno = function (message, callback) {
        return self.confirm(message, callback);
    };

    self.prompt = function (message, callback) {
        alertify.labels.ok = 'Ok';
        alertify.labels.cancel = 'Cancel';
        return alertify.prompt(message, callback);
    };

    self.processqs = function (app) {
        var toAlert = app.getquerystringvalue('alert');
        var toLog = app.getquerystringvalue('log');
        var toError = app.getquerystringvalue('error');
        var toSuccess = app.getquerystringvalue('success');

        if (toAlert != '')
            self.alert(toAlert);

        if (toSuccess != '')
            self.success(toSuccess);

        if (toLog != '')
            self.log(toLog);

        if (toError != '')
            self.error(toError);
    }

    self.processqs(app);
}

var app = new AppViewModel(new AppDataModel());

// jquery extensions
jQuery.fn.extend({
    alert: function (messages, type) {
        return this.each(function () {
            var c = app.alert.create(messages, type);
            $(this).append(c);
        });
    }
});

// string.format extension
// 'My string {0}'.f(variable)
String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};

// string.format extension
// 'My string {0}'.f(variable)
String.prototype.format = String.prototype.date = function () {
};