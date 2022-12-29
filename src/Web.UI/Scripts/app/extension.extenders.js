ko.extenders.numeric = function (target, precision) {
    //create a writable computed observable to intercept writes to our observable
    var result = function (newValue) {
        var current = target(),
            roundingMultiplier = Math.pow(10, precision),
            newValueAsNum = isNaN(newValue) ? 0 : +newValue,
            valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

        //only write if it changed
        if (valueToWrite !== current) {
            target(valueToWrite);
        } else {
            //if the rounded value is the same, but a different value was written, force a notification for the current field
            if (newValue !== current) {
                target.notifySubscribers(valueToWrite);
            }
        }
    };

    //initialize with current value to make sure it is rounded appropriately
    target.subscribe(result);

    //return the new computed observable
    return target;
};
ko.extenders.currency = function (target, symbol) {
    var result = ko.dependentObservable({
        read: function () {
            var value = target();
            var roundingMultiplier = Math.pow(10, 2),
            newValueAsNum = isNaN(value) ? 0 : parseFloat(+value),
            valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

            if (!symbol)
                symbol = app.currencysymbol;

            return symbol + ' ' + valueToWrite.toFixed(2);
        },
        write: target
    });

    result.raw = target;
    return result;
};

ko.extenders.logChange = function (target, option) {
    target.subscribe(function (newValue) {
        console.log(option + ": " + newValue);
    });
    return target;
};

ko.extenders.date = function (target, format) {
    var result = ko.dependentObservable({
        read: function () {
            var value = target();

            if (value) {
                var m = moment(value);

                if (format)
                    return m.format(format);
                else
                    return m.format('DD MMM YYYY');
                    //return m.calendar({ 'lastDay': 'D MMMM' });
            }

            return target();
        },
        write: target
    });

    result.raw = target;
    return result;
};
ko.extenders.preventNegative = function (target, track) {
    if (track) {
        target.subscribe(function (newValue) {
            if (target() < 0)
                target(0);
        });
    }

    return target;
};