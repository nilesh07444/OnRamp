/*
 To ensure corrent behavior with ko.validation.remote:
  !=the parameter of the remote validation method should be named "model"
  !=the parameter validated should only be one property of the ViewModel
*/

ko.validation.rules['remote'] = {
    async: true,
    validator: function (val, data, callback) {
        var $propertyName = data.property;
        var defaults = {};
        defaults.url = data.url;
        defaults.type = 'POST';
        model = {};
        model[$propertyName] = val;
        defaults.data = { model: model };
        $.ajax(defaults).then(function (response) {
            if (response !== true) {
                callback(false);
            } else {
                callback(true);
            }
        }, function () {
            callback(false);
        });
    }
};
