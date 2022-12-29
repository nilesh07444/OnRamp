$(document).ready(function () {
	// makes anchor overlay target
	$('a[data-overlay]').each(function () {
		var id = $(this).data('overlay');
		var w = $('#' + id).width();
		var h = $('#' + id).height();
		
		$(this).css('position', 'absolute');
		$(this).width(w);
		$(this).height(h);
	});

    

	$(".modal").on('shown.bs.modal', function () {
	    $(this).find("input:first").focus();

	    $(this).find(".autofocus:first").focus();
	});

	$(".modal").on('keypress', function (e) {
	    if (e.which === 13) {
	    //    $(this).find('.btn-primary:first').trigger('click');
	        return false;
	    }
	});
});

function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = (d + Math.random()*16)%16 | 0;
        d = Math.floor(d/16);
        return (c=='x' ? r : (r&0x3|0x8)).toString(16);
    });
    return uuid;
};
Array.prototype.remove = function (item, property) {
    var entry = this.find(item, property);
    if (entry)
        this.splice(entry[1], 1);
    return this;
}
Array.prototype.firstOrDefault = function (item, property) {
    return this.find(item, property)[0];
}
Array.prototype.find = function (item, property) {
    var r = [null, -1];
    this.filter(function (c, i) {
        if (property ? Object.isEqual(c[property], item[property]) : Object.isEqual(c, item))
            r = [c, i];
    });
    return r;
}
Array.prototype.where = function (condition) {
    return this.filter(function (c, index) {
        if (condition && condition(c))
            return c;
    });
}
Array.prototype.sync = function (source) {
    if (source.length < this.length)
        this.splice(0, this.length - source.length);
    for (var i = 0; i < source.length; i++) {
        if (i < this.length) {
            this.splice(i, 1, source[i]);
        } else {
            this.push(source[i]);
        }
    }
    return this;
}
Array.prototype.contains = function (item, property) {
    return this.filter(function (i, index) {
        if (property && $(i).hasOwnProperty(property) && $(item).hasOwnProperty(property))
            return i[property] == item[property];
        return i == item;
    }).length > 0;
}
Array.prototype.unique = function (property) {
    var dictionary = {};
    this.map(function (c, i, a) {
        if (!Object.keys(this).contains(c, property))
            this[c[property]] = c;
    }, dictionary);
    var values = Object.keys(dictionary).map(function (e) {
        return dictionary[e]
    });
    return this.sync(values);
}
Array.prototype.groupBy = function (property) {
    var dictionary = {};
    this.map(function (c, i, a) {
        if (!this[c[property]])
            this[c[property]] = [];
        this[c[property]].push(c);
    }, dictionary);
    return Object.keys(dictionary).map(function (c, i, a) {
        return { key: c, values: dictionary[c] };
    });
}
Object.equal = function (a, b) {
    var _a, _b;
    _a = Array.isArray(a) ? a['length'] : JSON.stringify(a);
    _b = Array.isArray(b) ? b['length'] : JSON.stringify(b);
    if (_a == _b)
        return 0;
    if (_a > _b)
        return 1;
    if (_a < _b)
        return -1;
}
Object.isEqual = function (a, b) {
    return Object.equal(a, b) == 0;
}
Array.prototype.orderBy = function (acc_desc, property) {
    this.sort(function (a, b) {
        return property ? Object.equal(a[property], b[property]) : Object.equal(a, b);
    });
    return acc_desc ? this.reverse() : this;
}
Array.prototype.take = function (count) {
    if (this.length <= count)
        return this;
    return this.sync(this.splice(0, count));
};
Array.prototype.clone = function () {
    var r = [];
    for (var i = 0; i < this.length; i++)
        r.push(i);
    return r;
}
Array.prototype.mapMany = function (property, callback) {
    var flat = [];
    this.map(function (c, i) {
        if (Array.isArray(c[property])) {
            for (var index = 0; index < c[property].length; index++) {
                this.push(c[property][index]);
            }
        } else {
            this.push(c[property]);
        }
    },flat);
    if (callback)
        flat.sync(flat.map(callback));
    return this.sync(flat);
};
