var AppDataModel = function () {
    var self = this;

    self.app = null;

    self.Validation = new ko.observable(self.validate);

    self.Validate = new ko.observable(self.validate);

    self.validate = new ko.observable(null);

    self.validation = new ko.observable(null);

    self.busy = new ko.observable(false);

    self.showloader = true;

    self.resetvalidation = function () {
        self.validate(null);
        self.validation(null);
        self.Validate(null);
        self.Validation(null);
    };

    self.clearvalidation = function () {
        self.resetvalidation();
    };

    self.unique = function (array, comparer) {
        return $.grep(array, function (el, index) {
            if (comparer) {
                var matches = $.grep(array, function (e, i) {
                    return comparer(el) == comparer(e);
                });

                if (matches.length > 0)
                    return el == matches[0];
                else
                    return false;
            }
            else
                return index == $.inArray(el, array);
        }); 00
    };

    self.ajaxrequests = 0;

    self.ajax = function (url, data, verb, dataType, cache, done) {
        self.busy(true);
        self.ajaxrequests++;

        var defaultDataType = "json";

        if (dataType == null)
            dataType = defaultDataType;

        if (url.indexOf('http') < 0) {
            url = window.location.origin + url;
        }

        var contentType = 'application/x-www-form-urlencoded; charset=UTF-8';

        if (dataType == defaultDataType && data && (verb != 'GET' && verb != 'get')) {
            contentType = "application/json; charset=UTF-8";
            data = ko.toJSON(data);
        }

        var ajax = $.ajax({
            url: url,
            data: data,
            type: verb,
            cache: cache || false,
            contentType: contentType,
            dataType: dataType,
            traditional: true
        });

        ajax.then(function (d, textStatus, xhr) {
            self.resetvalidation();

            if (done)
                done(d, textStatus, xhr);
        }, function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.responseJSON) {
                var json = jqXHR.responseJSON;
                if (json) {
                    self.validation(json);
                    self.validate(json);

                    self.Validation(json);
                    self.Validate(json);

                    if (json.Message)
                        app.message.error(json.Message);
                    else {
                        app.message.error("There are a few problems");
                    }
                }
            } else {
                app.message.error("An unknown error has occurred");
            }
        });

        if (self.showloader) {
            var loadingTimer = setTimeout(function () {
                $('.spinner').slideDown('fast');
                $('*').css('cursor', 'wait');
            }, 100);
        } else
            self.showloader = true;

        ajax.always(function () {
            self.ajaxrequests--;

            if (self.ajaxrequests == 0) {
                setTimeout(function () {
                    $('*').css('cursor', '');
                    window.clearTimeout(loadingTimer);
                    self.busy(false);
                    $('.spinner').fadeOut('fast');
                }, 200);
            }
        });
        return ajax;
    };

    self.actionurl = function (action, controller, area) {
        if (controller)
            return app.ResolveUrl('/' + (area == null ? '' : area + '/') + controller + (action == null ? '' : '/' + action));
        else
            return window.location.href;
    };

    self.redirect = function (action, controller, area, extra) {
        
        window.location = self.actionurl(action, controller, area) + extra;
    };

    self.loader = function (l) {
        self.showloader = l;
        return self;
    };

    // COMMON HTTP ACTIONS
    self.deletebyid = function (id, action, controller, area, done) {
        app.message.confirm('Are you sure?', function (ok) {
            if (ok) {
                self.ajax(self.actionurl(action, controller, area), { id: id }, "DELETE").done(done);
            }
        });
    };

    self.delete = function (data, action, controller, area, done) {
        app.message.confirm('Are you sure?', function (ok) {
            if (ok) {
                self.ajax(self.actionurl(action, controller, area), data, "DELETE").done(done);
            }
        });
    };

    self.post = function (data, action, controller, area, done) {
        return self.ajax(self.actionurl(action, controller, area), data, "POST", null, null, done);
    };

    self.patch = function (data, action, controller, area, done) {
        return self.ajax(self.actionurl(action, controller, area), data, "PATCH", null, null, done);
    };

    self.put = function (data, action, controller, area, done) {
        return self.ajax(self.actionurl(action, controller, area), data, "PUT", null, null, done);
    };

    self.get = function (data, action, controller, area, cache) {
        return self.ajax(self.actionurl(action, controller, area), data, "GET", null, cache);
    };

    self.list = function (data, action, controller, area) {
        return self.get(data, action, controller, area);
    };

    // SYSTEM ADMINISTRATION
    self.SystemAdministration = 'SystemAdministration';

    // USER
    self.AddUserToRole = function (data) {
        return self.post(data, 'role', 'user', self.SystemAdministration);
    };

    self.RemoveUserFromRole = function (data, done) {
        return self.delete(data, 'role', 'user', self.SystemAdministration, done);
    };

    self.AddUserToPermission = function (data) {
        return self.post(data, 'permission', 'user', self.SystemAdministration);
    };

    self.RemoveUserFromPermission = function (data, done) {
        return self.delete(data, 'permission', 'user', self.SystemAdministration, done);
    };

    // ROLE
    self.AddRole = function (data) {
        return self.post(data, null, 'role', self.SystemAdministration);
    };

    self.DeleteRole = function (id, done) {
        return self.deletebyid(id, null, 'role', self.SystemAdministration, done);
    };

    self.RenameRole = function (data) {
        return self.patch(data, null, 'role', self.SystemAdministration);
    };

    self.AddRoleToPermission = function (data) {
        return self.post(data, 'permission', 'role', self.SystemAdministration);
    };

    self.RemoveRoleFromPermission = function (data, done) {
        return self.delete(data, 'permission', 'role', self.SystemAdministration, done);
    };

    self.rampcontroller = function (data, links, mode, canSave, canEdit, canDelete, canSubmit, canClone) {
        if (data.hasOwnProperty('PublishStatus') && data.PublishStatus == null) data.PublishStatus = 0;
        
        var self = this;
        self.showLoading = function (bool) {
            var busy = true;

            if (bool) {
                $('#LoadingImageDiv').show();
            }
            else {
                $('#LoadingImageDiv').hide();
            }
        };
        self.busy = ko.computed(function () {
            return $('#LoadingImageDiv').css('display') !== 'none';
        });
        self.errors = ko.observable(null);
        self.saving = ko.observable(false);
        self.submitting = ko.observable(false);
        self.data = ko.mapping.fromJS(data);
        self.links = links;
        self.mode = ko.mapping.fromJS(mode);
        self.sorting = {
            getInverseColumnOrdering: function (type) {
                var specified = ko.unwrap(self.query.SortingOrder);
                return specified ? specified.split(':')[0] == type : false;
            },
            inverseColumnOrdering: function (column, event) {
                if (column && event && event.target) {
                    self.sorting.toggleInverseColumnOrderingDirection(event.target.name);
                }
            },
            toggleInverseColumnOrderingDirection: function (type) {
                if (!self.sorting.getInverseColumnOrdering(type))
                    self.query.SortingOrder(ko.unwrap(type) + ':Descending');
                else {
                    self.query.SortingOrder(ko.unwrap(type) + ':Ascending');
                }
            }
        };
        self.paging = {
            nextPage: function () {
                if (!ko.unwrap(self.query.IsLastPage)) {
                    self.query.Page(ko.unwrap(self.query.PageIndex) + 1);
                    self.paging.submit();
                }
            },
            previousPage: function () {
                if (!ko.unwrap(self.query.IsFirstPage)) {
                    self.query.Page(ko.unwrap(self.query.PageIndex) - 1);
                    self.paging.submit();
                }
            },
            goToPage: function () {
                self.query.Page(parseInt(this));
                self.paging.submit();
            },
            maxVisiblePages: 7,
            visiblePages: ko.pureComputed(function () {
                var current = self.query.PageIndex() + 1;
                var pageCount = self.query.Pages();
                var first = 1;

                var sectionLength = parseInt((self.paging.maxVisiblePages - 1) / 2);
                var upperLimit = current + sectionLength;
                var downLimit = current - sectionLength;

                while (upperLimit > pageCount) {
                    upperLimit--;
                    if (downLimit > first)
                        downLimit--;
                }

                while (downLimit < first) {
                    downLimit++;
                    if (upperLimit < pageCount)
                        upperLimit++;
                }

                var pages = [];
                for (var i = downLimit; i <= upperLimit; i++) {
                    pages.push(i);
                }
                return pages;
            }),
            submit: function () {
                self.submitting(true);
                $.post(self.links['index:post'], ko.mapping.toJS(self.query)).then(self.paging.syncPagedResult);
            },
            syncPagedResult: function (result) {
                self.submitting(false);
                var koResult = result;
                if (koResult.Items == 0) {
                    self.query.Page(0);
                    self.paging.submit();
                }
                app.data.utils.array.sync(ko.mapping.fromJS(koResult.Items)(), self.data.Items);
                self.query.HasNextPage(koResult.HasNextPage);
                self.query.HasPreviousPage(koResult.HasPreviousPage);
                self.query.IsFirstPage(koResult.IsFirstPage);
                self.query.IsLastPage(koResult.IsLastPage);
                self.query.PageIndex(koResult.PageIndex);
                //self.query.PageSize(koResult.PageSize);
                self.query.Pages(koResult.Pages);
                self.query.TotalItems(koResult.TotalItems);
            },
            initialize: function () {
                if (typeof self.data.HasNextPage != "undefined") {
                    self.query = {
                        HasNextPage: app.data.utils.observable.clone(self.data.HasNextPage),
                        HasPreviousPage: app.data.utils.observable.clone(self.data.HasPreviousPage),
                        IsFirstPage: app.data.utils.observable.clone(self.data.IsFirstPage),
                        IsLastPage: app.data.utils.observable.clone(self.data.IsLastPage),
                        PageIndex: app.data.utils.observable.clone(self.data.PageIndex),
                        PageSize: app.data.utils.observable.clone(self.data.PageSize),
                        Pages: app.data.utils.observable.clone(self.data.Pages),
                        TotalItems: app.data.utils.observable.clone(self.data.TotalItems),
                        Page: ko.observable(),
                        SortingOrder: ko.observable()
                    };
                    self.query.SortingOrder.subscribe(self.paging.submit);
                    self.query.PageSize.subscribe(function () {
                        self.query.Page(0);
                        self.paging.submit();
                    });
                }

            }
        };
        self.paging.initialize();
        self.redirect = function (location) {
            window.setTimeout(function () {
                if (ko.isObservable(self.saving))
                    self.saving(false);
                window.location.href = location;
            }, 500);
        };
        if (canSave) {
            self.save = function (redirect, messages, callback) {
                self.saving(true);

                if (vm.components().length > 0) {
                    vm.components().forEach(z => {
                        if (z.comName == "Policy-component") {
                            self.data.PolicyContent = z.params;
                        }
                    });
                }

                self.data.CreatedOn(((new Date()).toISOString()))
                $.ajax({
                    headers: {
                        "Accept": "application/json"
                    },
                    method: 'POST',
                    data: ko.mapping.toJS(self.data),
                    url: self.links.save
                }).then(function (response) {
                    notif({
                        msg: messages && $.isFunction(messages.success) ? messages.success() : 'Saved Successfully',
                        multiline: true,
                        type: 'success'
                    });
                    if (redirect) {
                        self.redirect(self.links.index);
                    } else {
                        self.saving(false);
                        if (callback && $.isFunction(callback))
                            callback();
                    }
                }, function (response) {
                    if (response.status === 400) {
                        self.saving(false);
                        self.errors(response.responseJSON);
                        notif({
                            msg: messages && $.isFunction(messages.failure) ? messages.failure() : 'Please fix the error/s and try again',
                            multiline: true,
                            type: 'error'
                        });
                    } else if (response.status === 500) {
                        self.saving(false);
                        notif({
                            msg: 'Something went wrong',
                            type: 'error'
                        });
                    }
                });
            };
        }
        if (canEdit) {
            self.edit = function (listModel) {
                if (listModel.Id)
                    self.redirect(ko.unwrap(self.links.edit) + '/' + ko.unwrap(listModel.Id));
                else
                    self.redirect(ko.unwrap(self.links.edit));
            }
        }
        if (canDelete) {
            self.delete = function (listModel) {
                self.saving(true);
                $.ajax({
                    type: "DELETE",
                    url: ko.unwrap(self.links.delete) + '/' + ko.unwrap(listModel.Id),
                    statusCode: {
                        400: function (xhr, code, error) {
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        },
                        202: function () {
                            notif({
                                msg: "<b>Success :</b> Deleted Successfully.",
                                type: "success",
                            });
                            self.redirect(ko.unwrap(self.links.index));
                        },
                        500: function () {
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        }
                    }
                });
            };
        }
        if (canClone) {
            self.clone = function (listModel) {
                self.saving(true);
                $.ajax({
                    type: "POST",
                    url: ko.unwrap(self.links.clone),
                    data: {
                        Id: ko.unwrap(listModel.Id)
                    },
                    statusCode: {
                        400: function (xhr, code, error) {
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Clone Failed.",
                                type: "warning",
                                multiline: true
                            });
                        },
                        202: function () {
                            notif({
                                msg: "<b>Success :</b> Cloned Successfully.",
                                type: "success",
                            });
                            self.redirect(ko.unwrap(self.links.index));
                        },
                        500: function () {
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Clone Failed.",
                                type: "warning",
                                multiline: true
                            });
                        }
                    }
                });
            };
        }
        if (canSubmit) {
            if (!ko.unwrap(self.data.FilteredResults))
                self.data.FilteredResults = new ko.observableArray();
            if (!ko.unwrap(self.data.result))
                self.data.result = new ko.observable();
            self.submit = function () {
                self.submitting(true);
                $.ajax({
                    headers: {
                        "Accept": "application/json"
                    },
                    method: 'POST',
                    data: ko.mapping.toJS(self.query),
                    url: self.links.post,
                }).then(function (response) {
                    var data = [];
                    if (response.FilteredResults)
                        app.data.utils.array.sync(ko.mapping.fromJS(response.FilteredResults), self.data.FilteredResults);
                    else {
                        self.data.result(ko.mapping.fromJS(response));
                    }
                    notif({
                        msg: 'Updated Successfully',
                        type: 'success'
                    });
                    self.submitting(false);
                    $('[data-toggle=tooltip]').tooltip();
                },
                    function (response) {
                        self.submitting(false);
                        if (response.status === 400) {
                            self.errors(response.responseJSON);
                            notif({
                                msg: 'Please fix the error/s and try again',
                                multiline: true,
                                type: 'error'
                            });
                        } else if (response.status == 500) {
                            notif({
                                msg: 'Something went wrong',
                                type: 'error'
                            });
                        } else {
                            notif({
                                msg: 'Please check your connection',
                                type: 'error'
                            });
                        }
                    });
            };
            self.download = {
                pdf: function () {
                    self.downloadReport(self.links.downloadPDF + '?' + $.param(ko.mapping.toJS(self.query), true));
                },
                excel: function () {
                    self.downloadReport(
                        self.links.downloadEXCEL + '?' + $.param(ko.mapping.toJS(self.query), true));
                }
            };
            self.downloadReport = function (url) {
                var request = new XMLHttpRequest();
                request.open("GET", url);
                request.responseType = 'blob';
                request.onload = function () {
                    var userAgent = window.navigator.userAgent;
                    var allowBlob = userAgent.indexOf('Chrome') > -1 || userAgent.indexOf('Firefox') > -1;
                    if (!allowBlob) {
                        window.navigator.msSaveBlob(this.response,
                            this.getResponseHeader('filename') || "download-" + $.now());
                    } else {
                        var url = window.URL.createObjectURL(this.response);
                        var a = document.createElement("a");
                        document.body.appendChild(a);
                        a.href = url;
                        a.download = this.getResponseHeader('filename') || "download-" + $.now();
                        a.click();
                        window.setTimeout(function () { document.body.removeChild(a); }, 500);
                    }
                }
                request.send();
            };
        }
        self.saving.subscribe(function (n, o) {
            if (n != o)
                self.showLoading(n);
        });
        self.submitting.subscribe(function (n, o) {

            if (n != o)
                self.showLoading(n);
            if (o == undefined)
                self.showLoading(false);

        });
    };

    self.pagedrampcontroller = function (data, links, mode, canSave, canEdit, canDelete, canSubmit, canClone, canPreview) {
        var self = this;
        self.showLoading = function (bool) {
            var busy = true;
            if (bool) {
                $('#LoadingImageDiv').show();
            }
            else {
                $('#LoadingImageDiv').hide();
            }
        }
        self.busy = ko.computed(function () {
            return $('#LoadingImageDiv').css('display') !== 'none';
        });
        self.errors = ko.observable(null);
        self.saving = ko.observable(false);
        self.submitting = ko.observable(false);
        self.data = ko.mapping.fromJS(data);
        self.links = links;
        self.mode = ko.mapping.fromJS(mode);
        self.redirect = function (location) {
            window.setTimeout(function () {
                if (ko.isObservable(self.saving))
                    self.saving(false);
                window.location.href = location;
            }, 500);
        };
        self.sorting = {
            getInverseColumnOrdering: function (type) {
                var specified = ko.unwrap(self.query.SortingOrder);
                return specified ? specified.split(':')[0] == type : false;
            },
            inverseColumnOrdering: function (column, event) {
                if (column && event && event.target) {
                    self.sorting.toggleInverseColumnOrderingDirection(event.target.name);
                }
            },
            toggleInverseColumnOrderingDirection: function (type) {
                if (!self.sorting.getInverseColumnOrdering(type))
                    self.query.SortingOrder(ko.unwrap(type) + ':Descending');
                else {
                    self.query.SortingOrder(ko.unwrap(type) + ':Ascending');
                }
            }
        };
        self.paging = {
            nextPage: function () {
                if (!ko.unwrap(self.query.IsLastPage)) {
                    self.query.Page(ko.unwrap(self.query.PageIndex) + 1);
                    self.paging.submit();
                }
            },
            previousPage: function () {
                if (!ko.unwrap(self.query.IsFirstPage)) {
                    self.query.Page(ko.unwrap(self.query.PageIndex) - 1);
                    self.paging.submit();
                }
            },
            goToPage: function () {
                self.query.Page(parseInt(this));
                self.paging.submit();
            },
            maxVisiblePages: 7,
            visiblePages: ko.pureComputed(function () {
                var current = self.query.PageIndex() + 1;
                var pageCount = self.query.Pages();
                var first = 1;

                var sectionLength = parseInt((self.paging.maxVisiblePages - 1) / 2);
                var upperLimit = current + sectionLength;
                var downLimit = current - sectionLength;

                while (upperLimit > pageCount) {
                    upperLimit--;
                    if (downLimit > first)
                        downLimit--;
                }

                while (downLimit < first) {
                    downLimit++;
                    if (upperLimit < pageCount)
                        upperLimit++;
                }

                var pages = [];
                for (var i = downLimit; i <= upperLimit; i++) {
                    pages.push(i);
                }
                return pages;
            }),
            submit: function () {
                self.submitting(false);
                $.post(self.links['index:post'], ko.mapping.toJS(self.query)).then(self.paging.syncPagedResult);
            },
            syncPagedResult: function (result) {
                self.submitting(false);
                var koResult = result;
                if (koResult.Items == 0) {
                    self.query.Page(0);
                    self.paging.submit();
                }
                app.data.utils.array.sync(ko.mapping.fromJS(koResult.Items)(), self.data.Items);
                self.query.HasNextPage(koResult.HasNextPage);
                self.query.HasPreviousPage(koResult.HasPreviousPage);
                self.query.IsFirstPage(koResult.IsFirstPage);
                self.query.IsLastPage(koResult.IsLastPage);
                self.query.PageIndex(koResult.PageIndex);
                //self.query.PageSize(koResult.PageSize);
                self.query.Pages(koResult.Pages);
                self.query.TotalItems(koResult.TotalItems);
            },
            initialize: function () {
                self.query = {
                    HasNextPage: app.data.utils.observable.clone(self.data.HasNextPage),
                    HasPreviousPage: app.data.utils.observable.clone(self.data.HasPreviousPage),
                    IsFirstPage: app.data.utils.observable.clone(self.data.IsFirstPage),
                    IsLastPage: app.data.utils.observable.clone(self.data.IsLastPage),
                    PageIndex: app.data.utils.observable.clone(self.data.PageIndex),
                    PageSize: app.data.utils.observable.clone(self.data.PageSize),
                    Pages: app.data.utils.observable.clone(self.data.Pages),
                    TotalItems: app.data.utils.observable.clone(self.data.TotalItems),
                    Page: ko.observable(),
                    SortingOrder: ko.observable()
                };
                self.query.SortingOrder.subscribe(self.paging.submit);
                self.query.PageSize.subscribe(function () {
                    self.query.Page(0);
                    self.paging.submit();
                });
            }
        };
        self.paging.initialize();
        if (canSave) {
            self.save = function (redirect) {
                self.saving(true);
                $.ajax({
                    method: 'POST',
                    data: ko.mapping.toJS(self.data),
                    url: self.links.save,
                    statusCode: {
                        400: function (xhr, code, error) {
                            self.saving(false);
                            self.errors(JSON.parse(error));
                            notif({
                                msg: 'Please fix the error/s and try again',
                                multiline: true,
                                type: 'error'
                            });
                        },
                        202: function () {
                            notif({
                                msg: 'Saved Successfully',
                                type: 'success'
                            });
                            if (redirect) {
                                self.redirect(self.links.index);
                            } else {
                                self.saving(false);
                            }
                        },
                        500: function () {
                            self.saving(false);
                            notif({
                                msg: 'Something went wrong',
                                type: 'error'
                            });
                        }
                    }
                });
            };
        }
        if (canEdit) {
            self.edit = function (listModel) {
                if (listModel.Id)
                    self.redirect(ko.unwrap(self.links.edit) + '/' + ko.unwrap(listModel.Id));
                else
                    self.redirect(ko.unwrap(self.links.edit));
            }
        }
        if (canDelete) {
            self.delete = function (listModel) {
                self.submitting(true);
                $.ajax({
                    type: "DELETE",
                    url: ko.unwrap(self.links.delete) + '/' + ko.unwrap(listModel.Id),
                    statusCode: {
                        400: function (xhr, code, error) {
                            self.submitting(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        },
                        202: function () {
                            notif({
                                msg: "<b>Success :</b> Deleted Successfully.",
                                type: "success",
                            });
                            self.redirect(ko.unwrap(self.links.index));
                        },
                        500: function () {
                            self.submitting(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        }
                    }
                });
            };
        }
        if (canClone) {
            self.clone = function (listModel, newVersion) {
                self.submitting(true);
                $.ajax({
                    type: "POST",
                    url: ko.unwrap(self.links.clone),
                    data: {
                        Id: ko.unwrap(listModel.Id),
                        NewVersion: !newVersion ? false : true
                    },
                    statusCode: {
                        400: function (xhr, code, error) {
                            self.submitting(false);
                            notif({
                                msg: "<b>Error :</b> " + (newVersion ? "Creating a new version failed." : "Duplication failed."),
                                type: "error",
                                multiline: true
                            });
                        },
                        202: function (data, statusCode, xhr) {
                            notif({
                                msg: "<b>Success :</b> " + (newVersion ? "Created a new version." : "Duplicated."),
                                type: "success",
                            });
                            return self.edit({ Id: data });
                        },
                        500: function () {
                            self.submitting(false);
                            notif({
                                msg: "<b>Error :</b> " + (newVersion ? "Creating a new version failed." : "Duplication failed."),
                                type: "error",
                                multiline: true
                            });
                        }
                    }
                });
            };
        }
        if (canSubmit) {
            if (!ko.unwrap(self.data.FilteredResults))
                self.data.FilteredResults = new ko.observableArray();
            if (!ko.unwrap(self.data.result))
                self.data.result = new ko.observable();
            self.submit = function () {
                self.submitting(true);
                $.ajax({
                    method: 'POST',
                    data: ko.mapping.toJS(self.query),
                    url: self.links.post,
                }).then(function (response) {
                    var data = [];
                    if (response.FilteredResults)
                        app.data.utils.array.sync(ko.mapping.fromJS(response.FilteredResults), self.data.FilteredResults);
                    else {
                        self.data.result(ko.mapping.fromJS(response));
                    }
                    notif({
                        msg: 'Updated Successfully',
                        type: 'success'
                    });
                    self.submitting(false);
                    $('[data-toggle=tooltip]').tooltip();
                },
                    function (xhr, code, error) {
                        self.submitting(false);

                        if (xhr.status == 400) {
                            self.errors(JSON.parse(error));
                            notif({
                                msg: 'Please fix the error/s and try again',
                                multiline: true,
                                type: 'error'
                            });
                        } else if (xhr.status == 500) {
                            notif({
                                msg: 'Something went wrong',
                                type: 'error'
                            });
                        } else {
                            notif({
                                msg: 'Please check your connection',
                                type: 'error'
                            });
                        }
                    });
            };
            self.download = {
                pdf: function () {
                    self.downloadReport(self.links.downloadPDF + '?' + $.param(ko.mapping.toJS(self.query), true));
                },
                excel: function () {
                    self.downloadReport(
                        self.links.downloadEXCEL + '?' + $.param(ko.mapping.toJS(self.query), true));
                }
            };
            self.downloadReport = function (url) {
                var request = new XMLHttpRequest();
                request.open("GET", url);
                request.responseType = 'blob';
                request.onload = function () {
                    var userAgent = window.navigator.userAgent;
                    var allowBlob = userAgent.indexOf('Chrome') > -1 || userAgent.indexOf('Firefox') > -1;
                    if (!allowBlob) {
                        window.navigator.msSaveBlob(this.response,
                            this.getResponseHeader('filename') || "download-" + $.now());
                    } else {
                        var url = window.URL.createObjectURL(this.response);
                        var a = document.createElement("a");
                        document.body.appendChild(a);
                        a.href = url;
                        a.download = this.getResponseHeader('filename') || "download-" + $.now();
                        a.click();
                        window.setTimeout(function () { document.body.removeChild(a); }, 500);
                    }
                }
                request.send();
            };
        }      
        if (canPreview) {
            self.preview = function (listModel) {
                localStorage.setItem("IsAdminReview", true);
                if (listModel.Id)
                    self.redirect(ko.unwrap(self.links.preview) + '/' + ko.unwrap(listModel.Id));
            }
        }
        self.saving.subscribe(function (n, o) {
            if (n != o)
                self.showLoading(n);
        });
        self.submitting.subscribe(function (n, o) {
            if (n != o)
                self.showLoading(n);
        });
    };

    self.listController = function (data, links, mode, canClone) {
        return new self.rampcontroller(data, links, mode, false, true, true, false, true);
    };

    self.pagedListController = function (data, links, mode, canClone, canPreview) {
        return new self.pagedrampcontroller(data, links, mode, false, true, true, false, canClone, canPreview);
    };

    self.crudController = function (data, links, mode) {
        return new self.rampcontroller(data, links, mode, true, false, false);
    };

    self.reportController = function (data, links, query) {
        var r = new self.rampcontroller(data, links, 'Report', false, false, false, true);
        r.query = ko.mapping.fromJS(query);
        return r;
    };

    self.documentController = function (data, links, mode, categories, contentModel, defaultContentTitle, acceptedUploadTypes, uploadModel, autoSaveInterval) {
        
        var r = new self.rampcontroller(data, links, mode, true);

        r.models = ko.mapping.fromJS({
            contentModel: contentModel,
            uploadModel: uploadModel
        });
        
        r.redirect = function (location) {
            window.setTimeout(function () {
                if (ko.isObservable(self.saving))
                    self.saving(false);
                if (r.contentTools)
                    r.contentTools.stop();
                window.location.href = location;
            }, 500);
        };

        function checkTitle() {
            var count = 1;
            var flag = false;
            var ansFlag = false;
            $(".chapterName").each(function () {

                $(".Question" + count).each(function () {
                    var ans = $(this).val();
                    if (ans === "" || typeof (ans) === "undefined" || ans === null) {
                        ansFlag = true;
                        $(this).addClass("errorClass");
                    } else {
                        $(this).removeClass("errorClass");
                    }
                });

                val = $(this).val();
                if (val === "" || typeof (val) === "undefined" || val === null) {
                    flag = true;
                    $(this).addClass("errorClass");
                }
            });
            if (flag || ansFlag) {

                $("#spnMessage").show();
                return false;
            } else {
                $("#spnMessage").hide();
                return true;
            }
        }

        r.publish = {

            delegate: function () {
                if (!checkTitle()) {
                    notif({
                        msg: r.messageDocumentType() !== 'test' ? '<b>Error:</b> Please ensure that you fill in all chapter title/s.' : '<b>Error:</b> Please fill in all required fields.',
                        type: 'error'
                    });

                    return false;
                }

                r.data.DocumentStatus(1);
                r.save(true,
                    {
                        success: function () {
                            return 'Your ' +
                                r.messageDocumentType() +
                                ' has been published successfully. Taking you back to the document library...';
                        },
                        failure: function () {
                            return 'Your ' +
                                r.messageDocumentType() +
                                ' could not be published, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                        }
                    });
            },
            message:
                'By clicking Publish your document will no longer be editable...<br/> Are you sure you want to continue?'
        };
       
        r.checkTitles = function () {

            $(".chapterName").each(function () {

                val = $(this).val();
                if (val === "" || typeof (val) === "undefined" || val === null) {

                    $(this).addClass("errorClass");
                } else {
                    $(this).removeClass("errorClass");
                }
            });

        };

        r.saveExit = function () {
            
            console.log("Save & Exit", r);

            if (!checkTitle()) {
                
                notif({
                    msg: r.messageDocumentType() !== 'test' ? '<b>Error:</b> Please ensure that you fill in all chapter title/s.' : '<b>Error:</b> Please fill in all required fields.',
                    type: 'error'
                });

                return false;
            }

            if (r.contentTools && $.isFunction(r.contentTools.save)) {
                r.contentTools.save();
            }

            r.save(true,{
                    success: function () {
                        return 'Your ' +
                            r.messageDocumentType() +
                            ' has been saved successfully. Taking you back to the document library...';
                    },
                    failure: function () {
                        return 'Your ' +
                            r.messageDocumentType() +
                            ' could not be saved, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                    }
                });
        };

        r.messageDocumentType = function () {
            switch (r.data.DocumentType()) {
                case 1:
                    return 'training manual';
                case 2:
                    return 'test';
                case 3:
                    return 'policy';
                case 4:
                    return 'memo';
                case 6:
                    return 'Activity Book';
                case 7:
                    return 'Custom Document';
                default: return 'CustomDocument'
            }
        };

        r.previewMode = {

            set: function () {
                
                if ($.isFunction(r.data.PreviewMode))
                    if ($.isNumeric(this))
                        r.data.PreviewMode(parseInt(this));
                
            }
        };

        r.printable = {
            set: function () {
                if ($.isFunction(r.data.Printable))
                    r.data.Printable(this == true);
            }
        };

        r.print = function () {
            if (!checkTitle()) {
                notif({
                    msg: r.messageDocumentType() !== 'test' ? '<b>Error:</b> Please fill all chapter first.' : '<b>Error:</b> Please fill required question and answer.',
                    type: 'error'
                });

                return false;
            }

            if (!r.saving()) {
                r.save(null, null, function () {
                    var url = ko.unwrap(r.links.print) + '/' + ko.unwrap(r.data.Id);
                    var request = new XMLHttpRequest();
                    request.open("GET", url);
                    request.responseType = 'blob';
                    request.onload = function () {
                        var userAgent = window.navigator.userAgent;
                        var allowBlob = userAgent.indexOf('Chrome') > -1 || userAgent.indexOf('Firefox') > -1;
                        if (!allowBlob) {
                            window.navigator.msSaveBlob(this.response,
                                this.getResponseHeader('filename') || "download-" + $.now());
                        } else {
                            var url = window.URL.createObjectURL(this.response);
                            var a = document.createElement("a");
                            document.body.appendChild(a);
                            a.href = url;
                            a.download = this.getResponseHeader('filename') || "download-" + $.now();
                            a.click();
                            window.setTimeout(function () { document.body.removeChild(a); }, 500);
                        }
                    }
                    request.send();
                });
            }
        };

        r.content = {

            defaultContentTitle: ko.observable(defaultContentTitle),

            reorder: function (data, event) {

                if (event.target.type !== 'button' && event.target.type !== 'text' && event.target.type !== 'select-one' && event.target.type !== 'submit') {
                    if (r.contentTools && $.isFunction(r.contentTools.stop)) {
                        r.contentTools.stop();
                    }
                    $.each(ko.unwrap(r.data.ContentModels) || [], function () {
                        var content = this;
                        focus($(event.target).parent());
                        if (content && content.state && ko.isObservable(content.state.minimized))
                            content.state.minimized(true);
                    });
                } else {
                    return true;
                }
            },

            restore: function (list) {

                r.contentTools.initialize();
            },

            toggle: function () {
                var content = this;
                if (content && content.state && ko.isObservable(content.state.minimized))
                    content.state.minimized(!ko.unwrap(content.state.minimized));
            },

            remove: function (type, docType, model) {
                
                var content = this;
                var collection = model;
                
                var myArray = vm.components();
                
                for (var i = 0; i <= (myArray.length - 1); i++) {
                    
                    if (docType == 3) {
                        
                        if (myArray[i].params.CustomDocumentOrder() == model.CustomDocumentOrder()) {
                            vm.components.splice(i, 1);
                        }
                    }
                    else {
                        
                        if (myArray[i].params.CustomDocumentOrder() == model.CustomDocumentOrder()) {
                            vm.components.splice(i, 1);
                        }
                    }
                }
                
                var params = { Type: docType, ID: model.Id };
                $.ajax({
                    type: "DELETE",
                    url: '../DeleteSection',
                    data: params,
                    statusCode: {
                        400: function (xhr, code, error) {
                            
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        },
                        202: function () {
                            
                            notif({
                                msg: "<b>Success :</b> Deleted Successfully.",
                                type: "success",
                            });

                            //  self.redirect(ko.unwrap(self.links.index));
                        },
                        500: function () {
                            
                            self.saving(false);
                            notif({
                                msg: "<b>Warning :</b> Delete Failed.",
                                type: "warning",
                                multiline: true
                            });
                        }
                    }
                });
            },

            Loadcomponents: function () {
                
                var recordModel = [];
                
                if (r.data.TMContentModels().length > 0) {
                    
                    r.data.TMContentModels().forEach(z => {
                        recordModel.push({ comName: 'TM-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 1 });
                    });
                }

                if (r.data.TestContentModels().length > 0) {
                    
                    r.data.TestContentModels().forEach(z => {                        
                        recordModel.push({ comName: 'Test-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 2 });
                    });
                }

                if (r.data.PolicyContentModels().length > 0) {
                    
                    r.data.PolicyContentModels().forEach(z => {
                        recordModel.push({ comName: 'Policy-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 3 });
                    });
                }

                if (r.data.MemoContentModels().length > 0) {
                    
                    r.data.MemoContentModels().forEach(z => {
                        recordModel.push({ comName: 'Memo-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 4 });
                    });

                }

                if (r.data.CLContentModels().length > 0) {
                    
                    r.data.CLContentModels().forEach(z => {
                        recordModel.push({ comName: 'CL-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 6 });
                    });
                }

                if (r.data.AcrobatFieldContentModels().length > 0) {
                    
                    r.data.AcrobatFieldContentModels().forEach(z => {
                        recordModel.push({ comName: 'Acrobat-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 7 });
                    });
                }
                
                if (r.data.FormContentModels().length > 0) {
                    
                    r.data.FormContentModels().forEach(z => {
                        recordModel.push({ comName: 'Form-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 9 });
                    });
                    
                }


                var datamod = recordModel.sort((a, b) => a.CustomDocOrder() - b.CustomDocOrder());
                
                datamod.forEach(z => {                    
                    r.content.fillQuestionies(z.params, z.type, true);
                    r.components.push(z);
                    
                });

            },

            OnChange_TestQuestion: function (obj, event) {
                var answerArr = [];
                var selectedQue = '';

                if (obj.data != undefined) {
                    selectedQue = obj.data.selectedTestQuestion();
                }
                else {
                    selectedQue = obj.selectedTestQuestion();
                }

                var AnswerList = r.data.TestContentModels().filter(z => z.Question() == selectedQue)[0].Answers()
                AnswerList.forEach(a => {
                    answerArr.push(a.Option());
                });

                if (obj.data != undefined) {
                    obj.data.TestAnswer(answerArr);
                }
                else {
                    obj.TestAnswer(answerArr);
                }

            },

            fillQuestionies: function (collection, docType, isEdit = false) {
                
                if (r.data.TestContentModels().length > 0) {
                    
                    var questionArr = [];
                    var answerArr = [];
                    
                    r.data.TestContentModels().forEach(z => {
                        console.log('z.Question()', z.Question());
                        if (z.Question() != null) {
                            questionArr.push(z.Question())
                        }
                    });
                    
                    if (!isEdit) {
                                               
                        if (docType == 5) {
                            
                            collection.TestQuestion(questionArr);
                            collection.TestAnswer(answerArr);
                            collection.IsConditionalLogic(true);
                        }                      
                        else {
                            
                            if (questionArr.length > 0 && docType == 2) {
                                
                                collection[collection.length - 1].TestQuestion(questionArr);
                                collection[collection.length - 1].TestAnswer(answerArr);
                              
                            }
                            else {
                                
                                collection[collection.length - 1].TestQuestion(questionArr);
                                collection[collection.length - 1].TestAnswer(answerArr);                             
                            }
                            
                        }
                    }
                    else {
                        
                        if (docType != 2) {
                            
                            try {
                                
                                collection.TestQuestion(questionArr);
                                if (collection.selectedTestQuestion() != undefined && collection.selectedTestQuestion().length > 0) {

                                    var AnswerList = r.data.TestContentModels().filter(z => z.Question() == collection.selectedTestQuestion())[0].Answers();
                                    AnswerList.forEach(a => {
                                        answerArr.push(a.Option());
                                    });

                                    collection.TestAnswer(answerArr);
                                }  
                            }
                            catch (ex) {
                                console.error("Error : fillQuestionies", ex);
                            }

                        }
                    }
                    return collection;
                }
            },

            add: function (type) {
                
                console.log("type ", type);
                console.log("add 1 ", r.data);
                

                if (type != undefined && type != null) {
                    
                    var collection = r.data.ContentModels;
                    
                    if (r.data.ContentModels == undefined) {
                        if (type == 1)
                            collection = r.data.TMContentModels;
                        if (type == 2)
                            collection = r.data.TestContentModels;
                        if (type == 3) {
                            //********************* This Block Has Been Modified By Softude *******************************
                            //  collection = r.data.PolicyContent;
                            //collection = r.data.PolicyContent;  /*commented by softude*/
                            //  r.content.generateNewSection(type, collection);
                            /*collection.IsShowPolicy(true);*/  /*commented by softude (giving error)*/


                            collection = r.data.PolicyContentModels;
                            //************************************ Ended **************************************************
                        }
                        if (type == 4) {
                            collection = r.data.MemoContentModels;
                        }
                        if (type == 6)
                            collection = r.data.CLContentModels;
                        if (type == 7) {
                            collection = r.data.AcrobatFieldContentModels;
                        }
                        if (type == 9) {
                            
                            collection = r.data.FormContentModels;
                        }
                        
                    }

                    //  if (r.models.contentModel) {
                    //  if (ko.isObservable(collection) && $.isArray(ko.unwrap(collection))) {
                    r.content.generateNewSection(type, collection);
                    //  }
                    //}
                }

            },

            generateNewSection: function (type, collection) {
                
                $.get(r.links.generateId).then(function (id) { 
                    var clone = type !== 5 ? self.utils.observable.clone(r.models.contentModel) : self.utils.observable.clone(collection);
                    
                    clone.TestQuestion = ko.observableArray();
                    clone.TestAnswer = ko.observableArray();
                    
                    clone.CustomDocumentOrder(r.components().length);
                    
                    if (ko.isObservable(clone.Title))
                        clone.Title(ko.unwrap(r.content.defaultContentTitle));
                    
                    if (type !== 5) {
                        clone.Number(ko.unwrap(collection).length + 1); // this line being added by Ashok
                        if (ko.isObservable(clone.Id))
                            clone.Id(id);
                        if (ko.isObservable(clone.New))
                            clone.New(true);
                        r.content.initialize(clone);
                        collection.push(clone);
                    }                    
                    if (type !== 5) { 
                        r.content.fillQuestionies(collection(), type);
                    }
                    
                    //********************************************** Ended ***************************************************************************

                    if (type == 1) {
                        r.components.push({ comName: 'TM-component', params: clone })
                    }
                    else if (type == 2)
                        r.components.push({ comName: 'Test-component', params: clone })
                    else if (type == 3 && r.content.isDuplicatePolicy()) {

                        r.components.push({ comName: 'Policy-component', params: clone })
                    }
                    else if (type == 4) {
                        r.components.push({ comName: 'Memo-component', params: clone })
                    }
                    else if (type == 6)
                        r.components.push({ comName: 'CL-component', params: clone })
                    else if (type == 7) {
                        r.components.push({ comName: 'Acrobat-component', params: clone })
                    }
                    else if (type == 9) {
                        
                        r.components.push({ comName: 'Form-component', params: clone })                        
                    }                    
                    var e = $('div[name=' + id + ']');
                    
                    if (e.length && ko.unwrap(collection).length > 1)
                        $('html,body').animate({ scrollTop: e.offset().top }, 1000);
                    
                });
            },

            isDuplicatePolicy: function () {
                var existsint = 0;
                if (r.components().length > 0) {

                    r.components().forEach(z => {
                        if (z.comName == "Policy-component") {
                            existsint++;
                        }
                    });
                    if (existsint >= 1) {
                        alert("Policy Chapter already exists");
                        return false;
                    }
                    else {
                        return true;
                    }
                }
                return true;
            },

            initialize: function (clone) {
                
                if (clone) {
                    
                    clone.state = {
                        minimized: ko.observable(false)
                    };
                    clone.Errors = new ko.observableArray();
                }
            },

            enableAttachmentRequired: function () {
                
            }
        };
        
        r.contentTools = {

            postFromContentToolsInitialUrl: r.links['contentTools:PostFromContentToolsInitial'],
            postFromContentToolsCommitUrl: r.links['contentTools:PostFromContentToolsCommit'],
            RotateImageUrl: r.links['contentTools:RotateImage'],
            initialize: function () {
                
                var timer = window.setInterval(function () {
                    var e = ContentTools.EditorApp.get();
                    if (e.isReady()) {
                        r.contentTools.edit();
                        window.clearInterval(timer);
                    }
                }, 5);
            },

            reinit: function () {
                
                ContentTools.EditorApp.get().init('*[data-editable]', 'name');
                r.contentTools.initialize();
                
            },
            stop: function () {
                try {
                    if (ContentTools && ContentTools.EditorApp && $.isFunction(ContentTools.EditorApp.get)) {
                        var e = ContentTools.EditorApp.get();
                        if ($.isFunction(e.stop))
                            e.stop(true);
                    }
                } catch (exeption) { }
            },
            destroy: function () {
                if (ContentTools && ContentTools.EditorApp && $.isFunction(ContentTools.EditorApp.get)) {
                    var e = ContentTools.EditorApp.get();
                    if ($.isFunction(e.stop))
                        e.stop(true);
                    if ($.isFunction(e.destroy))
                        e.destroy();
                }
            },
            edit: function () {
                
                if (ContentTools && ContentTools.EditorApp && $.isFunction(ContentTools.EditorApp.get)) {
                    
                    var e = ContentTools.EditorApp.get();
                    
                    if ($.isFunction(e.ignition))
                        if (e.ignition() && $.isFunction(e.ignition().edit))
                            e.ignition().edit();
                    
                }
            },
            getImages: function (content, contentModel) {
                
                var html = $(ko.unwrap(content));
                
                $.each(html || [],
                    function () {
                        
                        var tag = $(this);
                        var src = undefined;
                        if (ko.isObservable(contentModel.ContentToolsUploads) && $.isArray(ko.unwrap(contentModel.ContentToolsUploads))) {
                            if (tag.hasClass('ce-element--type-image') || tag.attr('src'))
                                src = tag.attr('src') || tag.css('background-image').replace('url("', '').replace('")', '');
                            if (self.utils.array.find(contentModel.ContentToolsUploads, function (e) { return e.url === src; })[0] === null)
                                contentModel.ContentToolsUploads.push({ url: src });
                        }
                    });
            },
            save: function () {
                
                if (ContentTools && ContentTools.EditorApp && $.isFunction(ContentTools.EditorApp.get))
                    ContentTools.EditorApp.get().save(true);
            }
        };

        r.initialize = function () {
            
            console.log("Initialization Started")
            var collection = r.data.ContentModels;  
            
            if (ko.isObservable(collection) && $.isArray(ko.unwrap(collection))) {
                
                $.each(ko.unwrap(collection), function () {
                    
                    var content = this;
                    r.content.initialize(content);
                    
                });

                for (var i = 0; i < (1 /* min content models */ - ko.unwrap(collection).length); i++) {
                    if (vm && vm.content && $.isFunction(vm.content.add))
                        vm.content.add();
                    else
                        r.content.add();
                }
            }
            if (!ko.unwrap(r.data.Category.Id)) {
                if (ko.unwrap(r.category.menu.options).length > 0) {
                    var first = ko.unwrap(r.category.menu.options)[0];
                    ko.unwrap(r.data.Category).Id(ko.unwrap(first.id));
                    ko.unwrap(r.data.Category).Name(ko.unwrap(first.text));
                }
            }

            if (autoSaveInterval && $.isNumeric(autoSaveInterval)) {
                
                window.AutoSave = function () {
                    r.save(false,
                        {
                            success: function () {
                                return 'Your ' +
                                    r.messageDocumentType() +
                                    ' has been autosaved successfully.';
                            },
                            failure: function () {
                                return 'Your ' +
                                    r.messageDocumentType() +
                                    ' could not be autosaved, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                            }
                        });
                }
                r.autosave = window.setInterval(window.AutoSave, autoSaveInterval);
            }

            if (r.contentTools) r.contentTools.initialize();

            console.log("Content Models:", r.models)
        };

        r.upload = {
            checkFileType: function (fileName, listOfAcceptedTypes, notifyException) {
                var valid = undefined;
                try {
                    var ext = fileName.substring(fileName.lastIndexOf('.') + 1);
                    var types = listOfAcceptedTypes || [];
                    if (types.length == 0)
                        if (ext && r.upload.options && r.upload.options.contentUpload && r.upload.options.contentUpload.acceptedTypes && r.upload.options.contentUpload.acceptedTypes.length > 0)
                            types = r.upload.options.contentUpload.acceptedTypes;
                    valid = self.utils.array.find(types, function (e) { return e.split(':')[1].toUpperCase() == ext.toUpperCase(); });
                } catch (exception) { }
                if (!valid && notifyException)
                    if (notif) {
                        notif({
                            msg: '<b>Error:</b> Please select a valid file type to upload.',
                            type: 'error'
                        });
                    }
                return valid;
            },

            concurrentUploads: ko.observableArray(),

            getScaledSize: function (total) {
                var totalString = "";
                var unit = { KB: ' KB', MB: ' MB', GB: ' GB' };
                totalString = (total / 1000).toPrecision(5) + unit.KB;
                if (total / 1000 > 1000) {
                    totalString = (total / (1000000)).toPrecision(5) + unit.MB;
                }
                if (total / (1000000) > 1000) {
                    totalString = (total / 1000000000).toPrecision(5) + unit.GB;
                }
                return totalString;
            },

            cancel: function (parent, upload) {
                var u = self.utils.array.find(parent.Attachments, function (i) {
                    return i == upload;
                });
                if (u) {
                    upload.submitEvent.data().jqxhr.abort();
                    parent.Attachments.remove(upload);
                }
            },

            delete: function (parent, upload) {
                parent.Attachments.remove(upload);
            },

            clearError: function (parent, upload) {
                parent.Errors.remove(upload);
            },

            options: {
                contentUpload: {
                    acceptedTypes: acceptedUploadTypes || [],
                    notifyInvalidFileType: true,
                    url: r.links['upload:posturl'],
                    autoUpload: true,
                    showProgress: false,
                    doNotAutoMap: true,
                    multiple: true,
                    before: function (element, fileName, valueProperty, container) {
                        
                        var valid = r.upload.checkFileType(fileName, [], r.upload.options.contentUpload.notifyInvalidFileType);
                        if (valid && !r.upload.options.contentUpload.multiple) {
                            if (container && ko.unwrap(container.Attachments) && $.isArray(ko.unwrap(container.Attachments))) {
                                
                            } else
                                valid = false;
                        }
                        if (valid) {
                            if (!ko.isObservable(valueProperty)) valueProperty = new ko.observable(valueProperty);
                            ko.unwrap(valueProperty).InProcess(true);
                            ko.unwrap(valueProperty).Name(fileName.replace('C:\\fakepath\\', ''));
                            ko.unwrap(valueProperty).Description(fileName.replace('C:\\fakepath\\', ''));
                            container.Attachments.push(ko.unwrap(valueProperty));
                            r.upload.concurrentUploads.push(ko.unwrap(valueProperty));
                            return true;
                        }
                        return false;
                    },
                    complete: function (valueProperty, container) {
                        container.Attachments.remove(ko.unwrap(valueProperty));
                        r.upload.concurrentUploads.remove(ko.unwrap(valueProperty));
                    },
                    success: function (data, textStatus, xhr, valueProperty, valueAccessor, container) {

                        if (data !== false) {
                            ko.unwrap(valueProperty).InProcess(false);
                            data.Index = ko.unwrap(container.Attachments).length;
                            var upload = self.utils.observable.clone(data);
                            container.Attachments.push(upload);
                            r.upload.options.contentUpload.complete(valueProperty, container);
                        } else {
                            r.upload.options.contentUpload.error(ko.unwrap(valueProperty), container);
                        }
                    },
                    error: function (valueProperty, container) {
                        ko.unwrap(valueProperty).Error = new ko.observable(true);
                        ko.unwrap(valueProperty).InProcess(false);
                        container.Errors.push(ko.unwrap(valueProperty));
                        r.upload.options.contentUpload.complete(valueProperty, container);
                    },
                    onProgress: function (event, position, total, percentage, fileName, valueProperty, submitEvent, valueAccessor, container) {
                        ko.unwrap(valueProperty).Progress(percentage + "%");
                        ko.unwrap(valueProperty).Size(total);
                        ko.unwrap(valueProperty).submitEvent = submitEvent;
                        ko.unwrap(valueProperty).container = new ko.observable(container);
                    }
                },
                coverPicture: {
                    showProgress: false,
                    url: r.links['upload:posturl'],
                    notifyInvalidFileType: true,
                    before: function (element, fileName, valueProperty, container) {
                        if (r.upload.checkFileType(fileName, ['image:png', 'image:jpeg', 'image:jpg', 'image:gif', 'image:bmp'], r.upload.options.coverPicture.notifyInvalidFileType)) {
                            r.upload.concurrentUploads.push(valueProperty);
                            return true;
                        }
                        return false;
                    },
                    complete: function (valueProperty, container) {
                        r.upload.concurrentUploads.remove(valueProperty);
                    },
                    success: function (data, textStatus, xhr, valueProperty, valueAccessor, container) {
                        if (data !== false) {
                            r.data.CoverPicture(self.utils.observable.clone(ko.unwrap(valueProperty)));
                        } else {
                            r.upload.options.coverPicture.error(valueProperty, container);
                        }

                    },
                    onProgress: null,
                    autoupload: true,
                    error: function (valueProperty, container) {
                        r.data.CoverPicture(null);
                        r.upload.options.coverPicture.complete(valueProperty, container);
                    }
                }
            },

            triggerUpload: function (data, event) {
                
                var newElement = $('<input data-bind="fileUpload:Upload,fileuploadOptions:vm.upload.options.contentUpload,valueProperty:new ko.observable(app.data.utils.observable.clone(vm.models.uploadModel)),container:$data" class="upload hidden" type="file" name="files[]" accept="Image/*" />');
                if (event.target.src.indexOf('imgDoc.png') > 0)
                    $(newElement).attr('accept', 'image/*');
                else if (event.target.src.indexOf('wordDoc.png') > 0)
                    $(newElement).attr('accept', '.doc,.docx');
                else if (event.target.src.indexOf('excelDoc.png') > 0)
                    $(newElement).attr('accept', '.xls,.xlsx');
                else if (event.target.src.indexOf('ppDoc.png') > 0)
                    $(newElement).attr('accept', '.ppt,.pptx,.pps,.ppsx');
                else if (event.target.src.indexOf('videoDoc.png') > 0)
                    $(newElement).attr('accept', 'video/mp4');
                else if (event.target.src.indexOf('otherDoc.png') > 0)
                    $(newElement).attr('accept', '.pdf');
                else if (event.target.src.indexOf('audDoc.png') > 0)
                    $(newElement).attr('accept', 'audio/mp3');
                else if (event.target.src.indexOf('pdfForm.png') > 0) {
                    $(newElement).attr('accept', '.pdf');
                }
                $(newElement).appendTo('#inputs');
                ko.applyBindingsToNode(newElement[0], null, data);
                $(newElement).click();
            }
        };

        r.category = {
            menu: {
                options: ko.observableArray(categories || []),
                toggle: function () {
                    r.category.menu.state.minimized(!ko.unwrap(r.category.menu.state.minimized));
                },
                state: {
                    minimized: ko.observable(true)
                }
            },
            handlers: {
                onChange: function (node, action) {
                    ko.unwrap(r.data.Category).Name(action.node.text);
                    r.category.menu.toggle();
                }
            }
        };

        r.jsTree = {
            options: {
                onChange: r.category.handlers.onChange,
                plugins: ['search', 'types'],
                types: {
                    default: {
                        icon: 'glyphicon glyphicon-plus-sign category-icon'
                    }
                }
            }
        };

        r.saving.subscribe(function (value) {
            if (value) {
                if (r.contentTools && $.isFunction(r.contentTools.save))
                    r.contentTools.save();
            }
        });

        return r;
    };

    self.previewController = function (data, links, usageOptions, contentModel, acceptedUploadTypes, uploadModel, categories, autoSaveInterval) {
        
        self.data = ko.mapping.fromJS(data);
        self.links = links;
        self.saving = ko.observable(false);
        self.errors = ko.observable(null);
        self.submitting = ko.observable(false);
        self.models = ko.mapping.fromJS({
            contentModel: contentModel,
            uploadModel: uploadModel
        });
        self.showLoading = function (bool) {
            var busy = true;
            if (bool) {
                $('#LoadingImageDiv').show();
            }
            else {
                $('#LoadingImageDiv').hide();
            }
        }
        self.save = function (redirect, messages, callback) {
            self.saving(true);
            $.ajax({
                headers: {
                    "Accept": "application/json"
                },
                method: 'POST',
                data: ko.mapping.toJS(self.data),
                url: self.links.save
            }).then(function (response) {
                notif({
                    msg: messages && $.isFunction(messages.success) ? messages.success() : 'Saved Successfully',
                    multiline: true,
                    type: 'success'
                });
                if (redirect) {
                    self.redirect(self.links.index);
                } else {
                    self.saving(false);
                    if (callback && $.isFunction(callback))
                        callback();
                }
            }, function (response) {
                if (response.status === 400) {
                    self.saving(false);
                    self.errors(response.responseJSON);
                    notif({
                        msg: messages && $.isFunction(messages.failure) ? messages.failure() : 'Please fix the error/s and try again',
                        multiline: true,
                        type: 'error'
                    });
                } else if (response.status === 500) {
                    self.saving(false);
                    notif({
                        msg: 'Something went wrong',
                        type: 'error'
                    });
                }
            });
        };
        self.updatePreviewData = function (redirect, messages, callback) {
            console.log(window.location.host);
            console.log(window.location.hostname);
            console.log(window.location.protocol);
            
            self.saving(true);
            
            $.ajax({
                headers: {
                    "Accept": "application/json"
                },
                method: 'POST',
                data: ko.mapping.toJS(self.data),
                url: self.links.preview
            }).then(function (response) {
                
                if (response = 'done') {
                    
                    notif({
                        msg: messages && $.isFunction(messages.success) ? messages.success() : 'Saved Successfully',
                        multiline: true,
                        type: 'success'
                    });                   

                    if (self.data.Status() == 4 || self.data.Status() == 5) {
                        
                        var currentprotocol = window.location.protocol;
                        var currenthost = window.location.host;
                        var redirecttopath = "Reporting/CustomDocumentSubmissionReport";
                        var url = currentprotocol + '//' + currenthost + '/' + redirecttopath;
                        window.location.href = url;                        
                    }
                    else {                        
                        self.redirect(self.links.index);                        
                    }                   

                } else {
                    self.saving(false);
                    if (callback && $.isFunction(callback))
                        callback();
                }
            }, function (response) {
                if (response.status === 400) {
                    self.saving(false);
                    self.errors(response.responseJSON);
                    notif({
                        msg: messages && $.isFunction(messages.failure) ? messages.failure() : 'Please fix the error/s and try again',
                        multiline: true,
                        type: 'error'
                    });
                } else if (response.status === 500) {
                    self.saving(false);
                    notif({
                        msg: 'Something went wrong',
                        type: 'error'
                    });
                }
            });
        };

        self.redirect = function (location) {
            window.setTimeout(function () {
                if (ko.isObservable(self.saving))
                    self.submitting(false);
                window.location.href = location;
            }, 500);
        };

        self.busy = ko.computed(function () {
            return $('#LoadingImageDiv').css('display') !== 'none';
        });



        function checkTitle() {
            var count = 1;
            var flag = false;
            var ansFlag = false;
            $(".chapterName").each(function () {

                $(".Question" + count).each(function () {
                    var ans = $(this).val();
                    if (ans === "" || typeof (ans) === "undefined" || ans === null) {
                        ansFlag = true;
                        $(this).addClass("errorClass");
                    } else {
                        $(this).removeClass("errorClass");
                    }
                });

                val = $(this).val();
                if (val === "" || typeof (val) === "undefined" || val === null) {
                    flag = true;
                    $(this).addClass("errorClass");
                }
            });
            if (flag || ansFlag) {

                $("#spnMessage").show();
                return false;
            } else {
                $("#spnMessage").hide();
                return true;
            }
        }

        self.publish = {

            delegate: function () {
                if (!checkTitle()) {
                    notif({
                        msg: self.messageDocumentType() !== 'test' ? '<b>Error:</b> Please ensure that you fill in all chapter title/s.' : '<b>Error:</b> Please fill in all required fields.',
                        type: 'error'
                    });

                    return false;
                }

                self.data.DocumentStatus(1);
                self.save(true,
                    {
                        success: function () {
                            return 'Your ' +
                                self.messageDocumentType() +
                                ' has been published successfully. Taking you back to the document library...';
                        },
                        failure: function () {
                            return 'Your ' +
                                self.messageDocumentType() +
                                ' could not be published, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                        }
                    });
            },
            message:
                'By clicking Publish your document will no longer be editable...<br/> Are you sure you want to continue?'
        };

        /*******call on title blur *******/
        self.checkTitles = function () {

            $(".chapterName").each(function () {

                val = $(this).val();
                if (val === "" || typeof (val) === "undefined" || val === null) {

                    $(this).addClass("errorClass");
                } else {
                    $(this).removeClass("errorClass");
                }
            });

        };

        self.saveExit = function () {
            
            console.log("saveExit 2 ", self);
            
            if (!checkTitle()) {
                notif({
                    msg: self.messageDocumentType() !== 'test' ? '<b>Error:</b> Please ensure that you fill in all chapter title/s.' : '<b>Error:</b> Please fill in all required fields.',
                    type: 'error'
                });

                return false;
            }
            
            if (self.contentTools && $.isFunction(self.contentTools.save))
                self.contentTools.save();
            self.save(true,
                {
                    success: function () {
                        return 'Your ' +
                            self.messageDocumentType() +
                            ' has been saved successfully. Taking you back to the document library...';
                    },
                    failure: function () {
                        return 'Your ' +
                            self.messageDocumentType() +
                            ' could not be saved, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                    }
                });
        };

        self.messageDocumentType = function () {
            switch (self.data.DocumentType()) {
                case 1:
                    return 'training manual';
                case 2:
                    return 'test';
                case 3:
                    return 'policy';
                case 4:
                    return 'memo';
                case 6:
                    return 'Workbook';
                case 7:
                    return 'AcrobatField';
                case 9:
                    return 'Form';

            }
        };

        self.previewMode = {
            set: function () {
                
                if ($.isFunction(self.data.PreviewMode))
                    if ($.isNumeric(this))
                        self.data.PreviewMode(parseInt(this));
                
            }
        };

        self.printable = {
            set: function () {
                if ($.isFunction(self.data.Printable))
                    self.data.Printable(this == true);
            }
        };

        self.print = function () {
            if (!checkTitle()) {
                notif({
                    msg: self.messageDocumentType() !== 'test' ? '<b>Error:</b> Please fill all chapter first.' : '<b>Error:</b> Please fill required question and answer.',
                    type: 'error'
                });

                return false;
            }

            if (!self.saving()) {
                self.save(null, null, function () {
                    var url = ko.unwrap(self.links.print) + '/' + ko.unwrap(self.data.Id);
                    var request = new XMLHttpRequest();
                    request.open("GET", url);
                    request.responseType = 'blob';
                    request.onload = function () {
                        var userAgent = window.navigator.userAgent;
                        var allowBlob = userAgent.indexOf('Chrome') > -1 || userAgent.indexOf('Firefox') > -1;
                        if (!allowBlob) {
                            window.navigator.msSaveBlob(this.response,
                                this.getResponseHeader('filename') || "download-" + $.now());
                        } else {
                            var url = window.URL.createObjectURL(this.response);
                            var a = document.createElement("a");
                            document.body.appendChild(a);
                            a.href = url;
                            a.download = this.getResponseHeader('filename') || "download-" + $.now();
                            a.click();
                            window.setTimeout(function () { document.body.removeChild(a); }, 500);
                        }
                    }
                    request.send();
                });
            }
        };

        self.content = {
            //defaultContentTitle: ko.observable(defaultContentTitle),
            reorder: function (data, event) {

                if (event.target.type !== 'button' && event.target.type !== 'text' && event.target.type !== 'select-one' && event.target.type !== 'submit') {
                    if (self.contentTools && $.isFunction(self.contentTools.stop)) {
                        self.contentTools.stop();
                    }
                    $.each(ko.unwrap(self.data.ContentModels) || [], function () {
                        var content = this;
                        focus($(event.target).parent());
                        if (content && content.state && ko.isObservable(content.state.minimized))
                            content.state.minimized(true);
                    });
                    console.log("r.models.contenmodel 4", r.models.ContentModels)
                } else {
                    return true;
                }
            },
            restore: function (list) {

                self.contentTools.initialize();
            },
            toggle: function () {
                var content = this;
                if (content && content.state && ko.isObservable(content.state.minimized))
                    content.state.minimized(!ko.unwrap(content.state.minimized));
            },
            remove: function () {

                var content = this;
                var collection = r.data.ContentModels;
                if (content)
                    self.utils.array.remove(content, collection, function (c) { return ko.unwrap(c.Id) == ko.unwrap(content.Id); });              

            },

            LoadTestData: function (selectedAnswer, data) {               
                var filterdAnswer = data.TestCollection.filter(x => x.params.selectedTestAnswer() == selectedAnswer());
                data.AnswerSelectedCollection([]);
             
                if (filterdAnswer != null && filterdAnswer != undefined) {
                    data.SelectedAnswer(selectedAnswer());
                    filterdAnswer.forEach(answer => {
                        
                        answer.params.IsConditionalLogic(false);
                        data.AnswerSelectedCollection.push({ compoName: answer.comName, parameters: answer.params });
                    });
                }
                
                setTimeout(() => {
                    $(".sig-canvas").each(function () {
                        InitializeCanvas($(this).attr("id"));
                    });
                }, 1000);

                data.Answers().forEach(a => {
                    
                    var id = a.Id();
                    
                    if (a.Option() == selectedAnswer()) {
                        
                        if (document.getElementById(id) != null) {
                            document.getElementById(id).style.padding = "2px";
                            document.getElementById(id).style.backgroundColor = "black";
                        }
                        
                    }
                    else {
                        
                        if (document.getElementById(id) != null) {
                            document.getElementById(id).style.padding = "1px";
                            document.getElementById(id).style.backgroundColor = "";
                        }
                        
                    }
                    
                });
                
            },

            Loadcomponents: function () {
                
                
                var recordModel = [];
                
                if (self.data.TMContentModels().length > 0) {
                    
                    self.data.TMContentModels().forEach(z => {
                        recordModel.push({ comName: 'TM-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 1 });
                    });
                }
                
                if (self.data.TestContentModels().length > 0) {                   
                    self.data.TestContentModels().forEach(z => {
                        recordModel.push({ comName: 'Test-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 2 });
                    });
                }                
               
                if (self.data.PolicyContentModels().length > 0) {
                    
                    self.data.PolicyContentModels().forEach(z => {
                        recordModel.push({ comName: 'Policy-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 3 });
                    });
                }                
              
                if (self.data.MemoContentModels().length > 0) {
                    self.data.MemoContentModels().forEach(z => {
                        recordModel.push({ comName: 'Memo-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 4 });
                    });

                }
                
                if (self.data.CLContentModels().length > 0) {
                    self.data.CLContentModels().forEach(z => {
                        recordModel.push({ comName: 'CL-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 6 });
                    });
                }
                
                if (self.data.AcrobatFieldContentModels().length > 0) {
                    self.data.AcrobatFieldContentModels().forEach(z => {
                        recordModel.push({ comName: 'Acrobat-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 7 });
                    });

                }
                
                if (self.data.FormContentModels().length > 0) {
                    
                    self.data.FormContentModels().forEach(z => {
                        recordModel.push({ comName: 'Form-preview-component', params: z, CustomDocOrder: z.CustomDocumentOrder, type: 9 });
                    });
                    
                }

                
                var datamod = recordModel.sort((a, b) => a.CustomDocOrder() - b.CustomDocOrder());
                
                var datamod = recordModel.sort((a, b) => a.CustomDocOrder() - b.CustomDocOrder());
                
                
                datamod.forEach((e) => {
                    if (e.type == 2) {
                        
                        var filterData = datamod.filter(z => ko.unwrap(z.params.TestQuestion) == ko.unwrap(e.params.Question()));
                        
                        e.params.TestCollection = filterData;
                        
                        e.params.AnswerSelectedCollection = ko.observableArray([]);
                        
                        console.log('filterData', filterData)
                        
                        datamod = datamod.filter(z => ko.unwrap(z.params.TestQuestion) != ko.unwrap(e.params.Question()));
                        
                    }
                    
                    var IsPreviewMode = localStorage.getItem("PreviewMode");
                    if (IsPreviewMode == "true") {
                        e.params.isDisabled = ko.observable(false);
                    }
                    else {
                        e.params.isDisabled = ko.observable(true);
                    }
                });
                
                datamod.forEach(z => {
                    self.components.push(z);                    
                });               

            },
            fillQuestionies: function (collection, docType, isEdit = false) {
                
                if (self.data.TestContentModels().length > 0) {
                    
                    var questionArr = [];
                    var answerArr = [];
                    
                    self.data.TestContentModels().forEach(z => {
                        
                        questionArr.push(z.Question())                       
                    });
                    if (!isEdit) {
                        
                        if (docType == 3) {
                            collection.TestQuestion(questionArr);
                            collection.TestAnswer(answerArr);
                            collection.IsConditionalLogic(true);
                        }
                        else {
                            collection.map(ele => {
                                ele.TestQuestion(questionArr);
                                ele.TestAnswer(answerArr);
                                ele.IsConditionalLogic(true);
                            });
                        }
                    }
                    else {
                        if (docType != 2) {
                            try {                               
                                
                                if (collection.selectedTestAnswer()) {
                                    collection.IsConditionalLogic(true);

                                }
                            }
                            catch (ex) {
                                console.error("Error : fillQuestionies", ex);
                            }
                        }
                    }
                    return collection;
                }
            },
            add: function () {
                console.log("add 2");
                var collection = self.data.ContentModels;
                if (self.models.contentModel)
                    if (ko.isObservable(collection) && $.isArray(ko.unwrap(collection))) {
                        $.get(self.links.generateId).then(function (id) {
                            var clone = self.utils.observable.clone(self.models.contentModel);
                            clone.Number(ko.unwrap(self.data.ContentModels).length + 1); // this line being added by Ashok
                            if (ko.isObservable(clone.Id))
                                clone.Id(id);
                            if (ko.isObservable(clone.New))
                                clone.New(true);
                            if (ko.isObservable(clone.Title))
                                clone.Title(ko.unwrap(self.content.defaultContentTitle));
                            self.content.initialize(clone);
                            collection.push(clone);
                            if (self.data.ContentModels !== undefined) {
                                self.data.ContentModels.push(clone);
                            }
                            var e = $('div[name=' + id + ']');
                            if (e.length && ko.unwrap(collection).length > 1)
                                $('html,body').animate({ scrollTop: e.offset().top }, 1000);
                        });
                    }
                // console.log("r.models.contenmodel ", r.models.ContentModels)
            },
            initialize: function (clone) {
                if (clone) {
                    clone.state = {
                        minimized: ko.observable(false)
                    };
                    clone.Errors = new ko.observableArray();
                }
            },
            find: function (content) {
                console.log("r.models.contenmodel 6", self.data.ContentModels)
                var temp = ko.unwrap(content);
                if (temp)
                    return self.utils.array.find(self.data.ContentModels, function (i) { return ko.unwrap(i.Id) == ko.unwrap(temp.Id); })[1];
                return -1;
            },
            generateHref: function (content) {
                return '#' + ko.unwrap(content.Id).toString();
            },
            selected: ko.observable(),
            isSelected: function (content) {
                
                if (ko.unwrap(self.content.selected) && content) {
                    
                    return ko.unwrap(ko.unwrap(self.content.selected).Id) == ko.unwrap(content.Id);
                }
                
                return false;
            },
            set: function (content) {
                if (!ko.unwrap(self.content.selected) || (ko.unwrap(self.content.selected) && (ko.unwrap(content.Id) != ko.unwrap(ko.unwrap(self.content.selected).Id)))) {
                    if (self.content.previewMode.isPortraitMode())
                        self.content.scroll(content);
                    self.content.selected(content);
                }
            },
            toggleAttachments: function (content) {
                console.log("content.state.showAttachments", content.state.showAttachments)
                content.state.showAttachments(!ko.unwrap(content.state.showAttachments));
            },
            initialize: function (content) {
                content.state = {
                    showAttachments: ko.observable()
                }
                console.log("content.state.showAttachments initialize", content.state.showAttachments)
            },
            previewMode: {
                current: ko.observable(ko.unwrap(self.data.PreviewMode)),
                set: function () {
                    var v = this;
                    if (ko.unwrap(self.content.previewMode.current) != ko.unwrap(v)) {
                        self.content.previewMode.current(v);
                        self.content.selected(null);
                        if (v == 1 && !self.paging.hasCover()) { // storybook
                            self.paging.next();
                        }
                    }
                },
                isPortraitMode: ko.pureComputed(function () {
                    return ko.unwrap(self.content.previewMode.current) == 0;
                }),
                isStorybookMode: ko.pureComputed(function () {
                    return ko.unwrap(self.content.previewMode.current) == 1;
                })
            },
            scroll: function (content) {
                $target = $(self.content.generateHref(content));
                if ($target.length > 0) {
                    event.preventDefault();
                    $('html, body').animate({
                        scrollTop: $target.offset().top - $('.navbar').height()
                    }, 1000);
                }
            },
            previewUpload: function (data) {
                var type = ko.unwrap(data.Type);
                if (!type)
                    return false;
                var applicable = type.indexOf('image') > -1 || type.indexOf('video') > -1 || type.indexOf('Youtube') > -1 || type.indexOf('Vimeo') > -1;
                if (!applicable) {
                    window.open(ko.unwrap(data.PreviewPath), '_blank');
                    return true;
                }
            }
        };

        self.upload = {
            checkFileType: function (fileName, listOfAcceptedTypes, notifyException) {
                var valid = undefined;
                try {
                    var ext = fileName.substring(fileName.lastIndexOf('.') + 1);
                    var types = listOfAcceptedTypes || [];
                    if (types.length == 0)
                        if (ext && self.upload.options && self.upload.options.contentUpload && self.upload.options.contentUpload.acceptedTypes && self.upload.options.contentUpload.acceptedTypes.length > 0)
                            types = self.upload.options.contentUpload.acceptedTypes;
                    valid = self.utils.array.find(types, function (e) { return e.split(':')[1].toUpperCase() == ext.toUpperCase(); });
                } catch (exception) { }
                if (!valid && notifyException)
                    if (notif) {
                        notif({
                            msg: '<b>Error:</b> Please select a valid file type to upload.',
                            type: 'error'
                        });
                    }
                return valid;
            },
            concurrentUploads: ko.observableArray(),
            getScaledSize: function (total) {
                var totalString = "";
                var unit = { KB: ' KB', MB: ' MB', GB: ' GB' };
                totalString = (total / 1000).toPrecision(5) + unit.KB;
                if (total / 1000 > 1000) {
                    totalString = (total / (1000000)).toPrecision(5) + unit.MB;
                }
                if (total / (1000000) > 1000) {
                    totalString = (total / 1000000000).toPrecision(5) + unit.GB;
                }
                return totalString;
            },
            cancel: function (parent, upload) {
                var u = self.utils.array.find(parent.Attachments, function (i) {
                    return i == upload;
                });
                if (u) {
                    upload.submitEvent.data().jqxhr.abort();
                    parent.Attachments.remove(upload);
                }
            },
            delete: function (parent, upload) {

                console.log("delete clicked ", parent);

                if (parent.IsStandardUserAttachements) {
                    parent.StandardUserAttachments.remove(upload);
                } else {
                    parent.Attachments.remove(upload);
                }

            },
            clearError: function (parent, upload) {
                parent.Errors.remove(upload);
            },
            options: {
                contentUpload: {
                    acceptedTypes: acceptedUploadTypes || [],
                    notifyInvalidFileType: true,
                    url: self.links['upload:posturl'],
                    autoUpload: true,
                    showProgress: false,
                    doNotAutoMap: true,
                    multiple: true,
                    before: function (element, fileName, valueProperty, container) {
                        var valid = self.upload.checkFileType(fileName, [], self.upload.options.contentUpload.notifyInvalidFileType);
                        if (valid && !self.upload.options.contentUpload.multiple) {
                            if (container && ko.unwrap(container.Attachments) && $.isArray(ko.unwrap(container.Attachments))) {
                                valid = self.utils.array.where(container.Attachments, function (i) {
                                    var match = self.upload.checkFileType(ko.unwrap(i.Name));
                                    return match[0].split(':')[0].toUpperCase() == valid[0].split(':')[0].toUpperCase();
                                }) == 0;
                                if (!valid && self.upload.options.contentUpload.notifyInvalidFileType)
                                    if (notif)
                                        notif({
                                            msg: '<b>Error:</b> Only one upload of type ' + self.upload.checkFileType(fileName, [], self.upload.options.contentUpload.notifyInvalidFileType)[0].split(':')[0].toLowerCase() + ' allowed.',
                                            type: 'error'
                                        });
                            } else
                                valid = false;
                        }
                        if (valid) {
                            if (!ko.isObservable(valueProperty))
                                valueProperty = new ko.observable(valueProperty);
                            ko.unwrap(valueProperty).InProcess(true);
                            ko.unwrap(valueProperty).Name(fileName.replace('C:\\fakepath\\', ''));
                            ko.unwrap(valueProperty).Description(fileName.replace('C:\\fakepath\\', ''));
                            if (container.IsStandardUserAttachements) {
                                container.StandardUserAttachments.push(ko.unwrap(valueProperty));
                            } else {
                                container.Attachments.push(ko.unwrap(valueProperty));
                            }
                            //container.Attachments.push(ko.unwrap(valueProperty));
                            self.upload.concurrentUploads.push(ko.unwrap(valueProperty));
                            return true;
                        }
                        return false;
                    },
                    complete: function (valueProperty, container) {
                        container.Attachments.remove(ko.unwrap(valueProperty));
                        container.StandardUserAttachments.remove(ko.unwrap(valueProperty));
                        self.upload.concurrentUploads.remove(ko.unwrap(valueProperty));
                    },
                    success: function (data, textStatus, xhr, valueProperty, valueAccessor, container) {
                        if (data !== false) {
                            ko.unwrap(valueProperty).InProcess(false);
                            data.Index = ko.unwrap(container.Attachments).length;
                            var upload = self.utils.observable.clone(data);
                            if (container.IsStandardUserAttachements) {
                                container.StandardUserAttachments.push(upload);
                            } else {
                                container.Attachments.push(upload);
                            }
                            self.upload.options.contentUpload.complete(valueProperty, container);
                        } else {
                            self.upload.options.contentUpload.error(ko.unwrap(valueProperty), container);
                        }
                    },
                    error: function (valueProperty, container) {
                        ko.unwrap(valueProperty).Error = new ko.observable(true);
                        ko.unwrap(valueProperty).InProcess(false);
                        container.Errors.push(ko.unwrap(valueProperty));
                        self.upload.options.contentUpload.complete(valueProperty, container);
                    },
                    onProgress: function (event, position, total, percentage, fileName, valueProperty, submitEvent, valueAccessor, container) {
                        ko.unwrap(valueProperty).Progress(percentage + "%");
                        ko.unwrap(valueProperty).Size(total);
                        ko.unwrap(valueProperty).submitEvent = submitEvent;
                        ko.unwrap(valueProperty).container = new ko.observable(container);
                    }
                },
                coverPicture: {
                    showProgress: false,
                    url: self.links['upload:posturl'],
                    notifyInvalidFileType: true,
                    before: function (element, fileName, valueProperty, container) {
                        if (self.upload.checkFileType(fileName, ['image:png', 'image:jpeg', 'image:jpg', 'image:gif', 'image:bmp'], self.upload.options.coverPicture.notifyInvalidFileType)) {
                            self.upload.concurrentUploads.push(valueProperty);
                            return true;
                        }
                        return false;
                    },
                    complete: function (valueProperty, container) {
                        self.upload.concurrentUploads.remove(valueProperty);
                    },
                    success: function (data, textStatus, xhr, valueProperty, valueAccessor, container) {
                        if (data !== false) {
                            self.data.CoverPicture(self.utils.observable.clone(ko.unwrap(valueProperty)));
                        } else {
                            self.upload.options.coverPicture.error(valueProperty, container);
                        }

                    },
                    onProgress: null,
                    autoupload: true,
                    error: function (valueProperty, container) {
                        self.data.CoverPicture(null);
                        self.upload.options.coverPicture.complete(valueProperty, container);
                    }
                }
            },
            dataURLtoFile: function (dataurl, filename) {

                var arr = dataurl.split(','),
                    mime = arr[0].match(/:(.*?);/)[1],
                    bstr = atob(arr[1]),
                    n = bstr.length,
                    u8arr = new Uint8Array(n);

                while (n--) {
                    u8arr[n] = bstr.charCodeAt(n);
                }

                return new File([u8arr], filename, { type: mime });
            },
            uploadSignature: {
                showProgress: false,
                url: self.links['upload:posturl'],
                notifyInvalidFileType: true,
                before: function (element, fileName, valueProperty, container) {
                    if (self.upload.checkFileType(fileName, ['image:png', 'image:jpeg', 'image:jpg', 'image:gif', 'image:bmp'], self.upload.options.coverPicture.notifyInvalidFileType)) {
                        self.upload.concurrentUploads.push(valueProperty);
                        return true;
                    }
                    return false;
                },
                complete: function (valueProperty, container) {
                    self.upload.concurrentUploads.remove(valueProperty);
                },
                success: function (data, textStatus, xhr, valueProperty, valueAccessor, container) {
                    if (data !== false) {
                        self.data.CoverPicture(self.utils.observable.clone(ko.unwrap(valueProperty)));
                    } else {
                        self.upload.options.coverPicture.error(valueProperty, container);
                    }

                },
                onProgress: null,
                autoupload: true,
                error: function (valueProperty, container) {
                    self.data.CoverPicture(null);
                    self.upload.options.coverPicture.complete(valueProperty, container);
                }

            },
            triggerUpload: function (data, event) {
                
                var newElement = $('<input data-bind="fileUpload:Upload,fileuploadOptions:vm.upload.options.contentUpload,valueProperty:new ko.observable(app.data.utils.observable.clone(vm.models.uploadModel)),container:$data" class="upload hidden" type="file" name="files[]" accept="Image/*" />');
                if (event.target.src.indexOf('imgDoc.png') > 0)
                    $(newElement).attr('accept', 'image/*');
                else if (event.target.src.indexOf('wordDoc.png') > 0)
                    $(newElement).attr('accept', '.doc,.docx');
                else if (event.target.src.indexOf('excelDoc.png') > 0)
                    $(newElement).attr('accept', '.xls,.xlsx');
                else if (event.target.src.indexOf('ppDoc.png') > 0)
                    $(newElement).attr('accept', '.ppt,.pptx,.pps,.ppsx');
                else if (event.target.src.indexOf('videoDoc.png') > 0)
                    $(newElement).attr('accept', 'video/mp4');
                else if (event.target.src.indexOf('otherDoc.png') > 0)
                    $(newElement).attr('accept', '.pdf');
                else if (event.target.src.indexOf('audDoc.png') > 0) {
                    $(newElement).attr('accept', 'audio/*');
                }
                
                $(newElement).appendTo('#inputs');
                
                ko.applyBindingsToNode(newElement[0], null, data);
                
                $(newElement).click();
                
            }
        };

        self.paging = {
            hasCover: function () {
                return ko.unwrap(self.data.CoverPicture) != null;
            },
            isCover: ko.unwrap(function () {
                if (self.paging.hasCover())
                    return !ko.unwrap(self.content.selected);
                return false;
            }),
            shouldShowCover: function () {
                return self.content.previewMode.isPortraitMode() || (self.content.previewMode.isStorybookMode() && self.paging.hasCover() && self.paging.isCover());
            },
            shouldShowContent: function (content) {
                return true; //self.content.previewMode.isPortraitMode() || (self.content.isSelected(content) && self.content.previewMode.isStorybookMode());
            },
            isFirst: ko.computed(function () {
                if (self.content.selected) {
                    var entry = self.content.find(self.content.selected);
                    return entry == 0;
                }
                return true;
            }),
            isLast: ko.computed(function () {
                if (ko.unwrap(self.content.selected)) {
                    var entry = self.content.find(self.content.selected);
                    return entry == ko.unwrap(self.data.ContentModels).length - 1;
                }
                return false;
            }),
            previous: function () {
                if (!self.paging.isFirst()) {
                    var entry = self.content.find(self.content.selected);
                    self.content.selected(ko.unwrap(self.data.ContentModels)[entry - 1]);
                } else {
                    self.content.selected(null);
                }
            },
            next: function () {
                if (!self.paging.isLast()) {
                    var entry = self.content.find(self.content.selected);
                    self.content.selected(ko.unwrap(self.data.ContentModels)[entry + 1]);
                }
            }
        };

        self.feedback = {
            validation: null,
            ContentType: ko.observable().extend({ required: true }),
            Content: ko.observable().extend({ required: true }),
            types: ko.observableArray(),
            contentTypes: ko.observableArray(),
            save: function () {
                if (self.feedback.validation().length > 0)
                    self.feedback.validation.showAllMessages();
                else {
                    self.submitting(true);
                    $.post(self.links['userFeedback:save'], {
                        ContentType: ko.unwrap(self.feedback.ContentType),
                        Content: ko.unwrap(self.feedback.Content),
                        DocumentType: ko.unwrap(self.data.DocumentType),
                        DocumentId: ko.unwrap(self.data.Id),
                        Type: 1
                    }).done(function (response) {
                        if (notif) {
                            notif({
                                type: 'success',
                                msg: 'Thank you , your feedback was submitted.'
                            });
                        }
                        self.feedback.reset();
                    }).fail(function () {
                        if (notif) {
                            notif({
                                type: 'error',
                                msg: 'Sorry, please try to submit your feedback again.'
                            });
                        }
                    }).always(function () {
                        self.submitting(false);
                    });
                }
            },
            reset: function () {
                self.feedback.Content(null);
                self.feedback.Content.isModified(false);
                self.feedback.ContentType(null);
                self.feedback.ContentType.isModified(false);
            }
        };

        self.feedback.initialize = function () {
            self.feedback.ContentType.extend({ required: true });
            self.feedback.Content.extend({ required: true });
            self.feedback.validation = ko.validation.group([self.feedback.ContentType, self.feedback.Content]);
            $.get(self.links['userFeedback:types']).then(function (data) {
                self.utils.array.sync(data, self.feedback.types);
            });
            $.get(self.links['userFeedback:contentTypes']).then(function (data) {
                self.utils.array.sync(data, self.feedback.contentTypes);
            });
        };

        self.options = {
            handlers: {
                ensureCenterPlacement: function () {
                    var heightChapter = $('.chapter').position().top + $('.chapter').height();
                    var heightFooter = $('footer').position().top;
                    var btnHeight = $('.bottom_nav').outerHeight();
                    var middle = (heightFooter - heightChapter - btnHeight) / 2;
                    $('.bottom_nav').css('padding-top', middle / 2);
                    $('.bottom_nav').css('padding-bottom', middle);
                },
                onscroll: function () {
                    var divStart = $('#divContent').parent().offset().top;
                    var divEnd = divStart + $('#divContent').outerHeight();
                    if ($('#divContent').parent().offset().top < $(window).scrollTop() + $('.navbar').height()) {
                        $("#divContent").css({ "top": $('.navbar').height(), "position": "fixed", "z-index": "0", "width": $("#divContent").parent().width() });
                        if ($('footer').height() + $('#page-wrapper').height() + $('.navbar').height() - $('body').scrollTop() < $('#divContent').height())
                            $("#divContent").css({ "top": $('#page-wrapper').height() - $('#divContent').height() - $('.navbar').height(), "position": "absolute", "z-index": "0", "width": $("#divContent").parent().width() });
                    }
                    else
                        $("#divContent").css({ "position": "relative", "width": "100%", "z-index": "0", "top": "auto" });
                }
            }
        };

        self.usage = {
            poll: null,
            timer: null,
            duration: 0,
            startTimer: function () {
                self.usage.timer = setInterval(function () {
                    self.usage.duration += 1;
                },
                    1000);
            },
            stopTimer: function () {
                clearInterval(self.usage.timer);
            },
            initialize: function () {
                if (usageOptions && usageOptions.startTime) {
                    self.usage.startTimer();
                    self.usage.poll = setInterval(function () {
                        $.ajax({
                            method: 'POST',
                            url: self.links['poll'],
                            data: {
                                duration: self.usage.duration,
                                startTime: usageOptions.startTime,
                                documentId: ko.unwrap(self.data.Id)
                            }
                        }).fail(function (xhr) {
                            if (xhr.status === 403) {
                                window.location = self.links['inProgress'];
                            }
                        });
                    },
                        parseInt(usageOptions.trackingInterval));

                    if (self.usage.hidden in document)
                        document.addEventListener('visibilitychange', self.usage.onchange);
                    else if ((self.usage.hidden = 'mozHidden') in document)
                        document.addEventListener('mozvisibilitychange', self.usage.onchange);
                    else if ((self.usage.hidden = 'webkitHidden') in document)
                        document.addEventListener('webkitvisibilitychange', self.usage.onchange);
                    else if ((self.usage.hidden = 'msHidden') in document)
                        document.addEventListener('msvisibilitychange', self.usage.onchange);
                    else if ('onfocusin' in document)
                        document.onfocusin = document.onfocusout = self.usage.onchange;
                    else
                        window.onpageshow = window.on4pagehide = window.onfocus = window.onblur = self.usage.onchange;
                }
            },
            hidden: 'hidden',
            onchange: function (evt) {
                var v = self.usage.startTimer, h = self.usage.stopTimer,
                    evtMap = {
                        focus: v, focusin: v, pageshow: v, blur: h, focusout: h, pagehide: h
                    };

                evt = evt || window.event;
                if (evt.type in evtMap)
                    evtMap[evt.type]();
                else
                    this[self.usage.hidden] ? self.usage.stopTimer() : self.usage.startTimer();
            }
        };

        self.initialize = function (type) {
            
            var collection = self.data.ContentModels;
            
            if (self.data.ContentModels == undefined) {
                if (type == 1)
                    collection = self.data.TMContentModels;
                if (type == 2)
                    collection = self.data.TestContentModels;
                if (type == 3)
                    collection = self.data.PolicyContentModels;
                if (type == 4)
                    collection = self.data.MemoContentModels;
                if (type == 6)
                    collection = self.data.CLContentModels;
                if (type == 7)
                    collection = self.data.AcrobatFieldContentModels;
                
            }

            if (ko.isObservable(collection) && $.isArray(ko.unwrap(collection))) {
                $.each(ko.unwrap(collection), function () {
                    var content = this;
                    self.content.initialize(content);
                });
                for (var i = 0; i < (1 /* min content models */ - ko.unwrap(collection).length); i++) {
                    if (vm && vm.content && $.isFunction(vm.content.add))
                        vm.content.add();
                    else
                        self.content.add();
                }
            }
            if (!ko.unwrap(self.data.Category.Id)) {
                if (ko.unwrap(self.category.menu.options).length > 0) {
                    var first = ko.unwrap(self.category.menu.options)[0];
                    ko.unwrap(self.data.Category).Id(ko.unwrap(first.id));
                    ko.unwrap(self.data.Category).Name(ko.unwrap(first.text));
                }
            }

            $.each(ko.unwrap(self.data.ContentModels) || [], function (i) {
                var c = this;
                self.content.initialize(c);
            });
            $('.content-tools a').on('click', function (event) {
                window.open($(this).attr('href'), '');
                return false;
            });
            $('#main-wrapper').css('padding-top', '120px');
            //window.addEventListener('scroll', self.options.handlers.ensureCenterPlacement);
            window.addEventListener('resize', self.options.handlers.onscroll);
            self.feedback.initialize();
            self.usage.initialize();
            if (autoSaveInterval && $.isNumeric(autoSaveInterval)) {
                window.AutoSave = function () {
                    self.save(false,
                        {
                            success: function () {
                                return 'Your ' +
                                    self.messageDocumentType() +
                                    ' has been autosaved successfully.';
                            },
                            failure: function () {
                                return 'Your ' +
                                    self.messageDocumentType() +
                                    ' could not be autosaved, please ensure that your content has been entered correctly into the required fields or you may lose your progress.';
                            }
                        });
                }
                self.autosave = window.setInterval(window.AutoSave, autoSaveInterval);
            }
            if (self.contentTools)
                self.contentTools.initialize();
        }

        self.submitting.subscribe(function (n, o) {
            if (n != o)
                self.showLoading(n);
        });

        self.category = {
            menu: {
                options: ko.observableArray(categories || []),
                toggle: function () {

                    self.category.menu.state.minimized(!ko.unwrap(self.category.menu.state.minimized));
                },
                state: {
                    minimized: ko.observable(true)
                }
            },
            handlers: {
                onChange: function (node, action) {

                    ko.unwrap(self.data.Category).Name(action.node.text);
                    self.category.menu.toggle();
                }
            }
        };

        self.jsTree = {

            options: {
                onChange: self.category.handlers.onChange,
                plugins: ['search', 'types'],
                types: {
                    default: {
                        icon: 'glyphicon glyphicon-plus-sign category-icon'
                    }
                }
            }
        };

        self.saving.subscribe(function (value) {
            if (value) {
                if (self.contentTools && $.isFunction(self.contentTools.save))
                    self.contentTools.save();
            }
        });

        //console.log("r.self.data 11", delf.data.ContentModels)
        return self;
    }

    self.plugins = {};

    self.plugins.blueimpGallary = {
        trigger: function (upload) {
            if ((ko.unwrap(upload.Type).indexOf('image') > -1) || (ko.unwrap(upload.Type).indexOf('video') > -1)) {
                self.plugins.blueimpGallary.addToGallery(upload);
                return false;
            }
            return true;
        },
        addToGallery: function (upload) {
            var uploads = [];
            uploads.push(new self.plugins.blueimpGallary.gallaryObject(upload));
            blueimp.Gallery(uploads);
        },
        gallaryObject: function (upload) {
            return {
                title: upload.Name(),
                href: upload.Url(),
                type: upload.Type()
            };
        }
    };

    self.typeahead = {};

    self.typeahead.makesource = function (action) {
        var bh = new Bloodhound({
            datumTokenizer: function (d) {
                return d.Tokens;
            },
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            prefetch: {
                ttl: 1,
                url: action
            },

            limit: 10
        });

        bh.initialize();

        return bh.ttAdapter();
    }

    self.typeahead.create = function (identifier, action, name, watchObservable) {
        var data;
        if (watchObservable)
            watchObservable.subscribe(refresh);
        refresh(true, false);
        function refresh(newValue, oldValue) {
            if (newValue !== oldValue) {
                data = self.typeahead.makesource(action);
                $(identifier).typeahead('destroy');
                c();
            }
        };
        function c() {
            $(identifier).typeahead({
                hint: true,
                highlight: true,
                minLength: 1
            }, {
                displayKey: 'Value',
                source: data,
                templates: {
                    header: '<div class="autocomplete heading">' + name + '</div>',
                    suggestion: function (item) { return '<p>' + ((item.Value == item.Extra) ? item.Value : '<strong>' + item.Value + '</strong> (' + item.Extra + ')') + '</p>'; }
                }
            }
            );
            $(identifier).addClass('typeahead');
        }
    }

    self.typeahead.tags = {};

    self.typeahead.tags.create = function (identfier, action, name, freeInput, watchObservable) {
        refresh(true, false);
        var data;
        function match(q, cb) {
            var matches, substrRegex;

            // an array that will be populated with substring matches
            matches = [];

            // regex used to determine if a string contains the substring `q`
            substrRegex = new RegExp(q, 'i');

            // iterate through the pool of strings and for any string that
            // contains the substring `q`, add it to the `matches` array
            $.each(data, function (i, str) {
                if (substrRegex.test(str)) {
                    // the typeahead jQuery plugin expects suggestions to a
                    // JavaScript object, refer to typeahead docs for more info
                    matches.push(str);
                }
            });

            cb(matches);
        }
        if (watchObservable)
            watchObservable.subscribe(refresh);
        function refresh(newValue, oldValue) {
            if ($(identfier).tagsinput()[0]) {
                var oldData = $(identfier).tagsinput()[0].itemsArray;
                $(identfier).tagsinput('destroy');
                $.get(action).then(function (response) { data = response; });
                c();
                $.each(oldData,
                    function () {
                        $(identfier).tagsinput()[0].add(this.toString());
                    });
            } else {
                c();
            }
        }
        function c() {
            $(identfier).tagsinput({
                typeaheadjs: {
                    source: match,
                },
                freeInput: freeInput ? true : false
            });
            $(identfier).addClass('typeahead');
        }
    }

    self.utils = {};

    self.utils.array = {};

    self.utils.array.find = function (list, comparer) {
        var r = [null, -1];
        self.utils.array.unwrap(list).filter(function (c, i) {
            if (comparer(c))
                r = [c, i];
        });
        return r;
    }

    self.utils.array.remove = function (entry, list, comparer) {

        var model = null;
        entry = ko.isObservable(entry) ? ko.unwrap(entry) : entry;
        $.each(ko.isObservable(list) ? ko.unwrap(list) : list, function () {
            if (comparer(ko.unwrap(this)))
                model = this;
        });
        if (!model)
            return;
        ko.isObservable(list) ? list.remove(model) : list.splice(list.indexOf(model), 1);
    }

    self.utils.array.sync = function (entries, array) {
        if ($.isFunction(array.removeAll))
            array.removeAll();
        var t = self.utils.array.unwrap(entries).map(function (c, i, a) {
            return ko.isObservable(c) ? self.utils.observable.clone(c) : c;
        });
        for (var i = 0; i < t.length; i++)
            array.push(t[i]);
    };

    self.utils.array.copy = function (entries, array) {
        return self.utils.array.unwrap(array).sync(
            self.utils.array.unwrap(enteries).map(function (c, i, a) {
                return ko.isObservable(c) ? self.utils.observable.clone(c) : c;
            }));
    }

    self.utils.array.where = function (list, comparer) {
        return self.utils.array.unwrap(list).where(comparer);
    }

    self.utils.array.unwrap = function (list) {
        if (!list || !Array.isArray(ko.unwrap(list)))
            return [];
        return ko.isObservable(list) ? ko.unwrap(list) : list;
    }

    self.utils.observable = {};

    self.utils.observable.clone = function (observable) {
        return ko.mapping.fromJS(ko.mapping.toJS(observable));
    }

    self.utils.date = {};

    self.utils.date.format = function (date, format) {
        if (!ko.unwrap(date))
            return null;
        var m = moment(ko.unwrap(date));
        return m.format(format);
    }

    self.utils.string = {};

    self.utils.string.replaceDeliminator = function (string, deliminator, newDeliminator) {
        string = ko.unwrap(string);
        deliminator = ko.unwrap(deliminator);
        newDeliminator = ko.unwrap(newDeliminator);
        if (string && deliminator && newDeliminator) {
            var m = string.split(deliminator);
            var ra = [];
            var r = '';
            if (m) {
                $.each(m,
                    function () {
                        ra.push(this.toString() + newDeliminator);
                    });
                $.each(ra,
                    function () {
                        r = r + this.toString();
                    });
                return r;
            }
        }
        return '';
    }

    self.utils.string.toDisplayString = function (value, maxCharacters) {
        var displayString = [];
        var string = '';
        var index = 0;
        if (!value)
            return value;
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
    };

    self.utils.string.contains = function (q, source) {
        return new RegExp(q, 'i').test(source);
    };

    self.utils.string.trim = function (string, length) {
        if (!string)
            return null;
        return ko.isObservable(string) ? ko.unwrap(string).substring(0, length) : string.substring(0, length);
    }

    self.utils.string.truncate = function (string, length) {
        var v = string;

        if (ko.isObservable(v))
            v = v();
        if (v)
            if (v.length && length && length > 0)
                if (v.length > length)
                    v = v.substring(0, length - 4) + '...';
        return v;
    }
}