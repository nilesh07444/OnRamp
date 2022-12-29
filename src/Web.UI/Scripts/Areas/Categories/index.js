function isUnique(collection, name, position) {
    var unique = true;
    $.each(collection, function (index) {
        var currentName = this.innerText;
        currentName = currentName.toLowerCase().replace(' ', '').trim();
        if (currentName.indexOf('\n') > 0) {
            currentName = currentName.substring(0, currentName.indexOf('\n'));
        }
        if (index !== position) {
            if (name.toLowerCase().replace(' ', '').trim() === currentName) {
                unique = false;
            }
        }
    });
    return unique;
}

function CreateNode(data) {
    var node = $.extend(true, {}, data.node);
    var siblings = $("#" + data.parent).children().siblings("ul").children().children().siblings("a");
    if (isUnique(siblings, node.text, data.position)) {
        var id = generateUUID();
        $.ajax({
            type: "POST",
            url: "Categories/Create",
            data: { ABC: node.text, id: node.parent, newid: id }
        }).done(function (obj) {
            data.instance.set_id(node, id);
            data.instance.edit(id);
            notif({
                msg: "<b>Success :</b> Category Added sucessfully.",
                type: "success",
            });
        });
    } else {
        notif({
            msg: "<b>Error</b> : Duplicate category detected in node.",
            type: "error"
        });
        window.setTimeout(function () { window.location = window.location; }, 3000);
    }
}

function Refresh() {
    $.ajax({ url: "Categories/GetTree" })
    .done(function (result) {
        var obj = JSON.parse(result);
        $("#demoTree").jstree({
            "core": {
                "data": obj,
                "check_callback": true
            },
            "defaults": {
                "contextmenu": {
                    "items": function ($node) {
                        var tree = $("#demoTree").jstree(true);
                        return {
                            "AddCategory": {
                                "separator_before": false,
                                "separator_after": false,
                                "label": "Add Category",
                                "action": function (obj) {
                                    $node = tree.create_node($node);
                                    tree.edit($node);
                                }
                            },
                            "EditCategory": {
                                "separator_before": false,
                                "separator_after": false,
                                "label": "Edit Category",
                                "action": function (obj) {
                                    tree.edit($node);
                                }
                            },
                            "DeleteCategory": {
                                "separator_before": false,
                                "separator_after": false,
                                "label": "Delete Category",
                                "action": function (obj) {
                                    if ($(this._get_node(obj)).attr('id') == "00000000-0000-0000-0000-000000000000") {
                                        notif({
                                            msg: "<b>Error :</b> You can't delete root node of Category.",
                                            type: "error",
                                        });
                                    }
                                    else {
                                        bootbox.confirm("Do you really want to delete this Category?", function (result) {
                                            if (result) {
                                                tree.delete_node($node);
                                            }
                                        });
                                    }
                                }
                            }
                        };
                    }
                }
            },
            "plugins": ["themes", "json_data", "ui", "crrm", "contextmenu", "search"]
        });
    });
}

$(document).ready(function () {
    Refresh();

    $("#demoTree").on("move_node.jstree", function (event, data) {
        var node = $.extend(true, {}, data.node);
        $.ajax({
            type: "POST",
            url: "Categories/ChangeRootOfNode",
            data: { ParentId: node.parent, Id: node.id },
            success: function (obj) {
                notif({
                    msg: "<b>Success :</b> Category moved sucessfully.",
                    type: "success",
                });
                if (_adjustPageFooter)
                    _adjustPageFooter();
            },
            error: function (data) {
            }
        });
    });

    $("#demoTree").on("create_node.jstree", function (event, data) {
        CreateNode(data);
    });

    $("#demoTree").on("delete_node.jstree", function (event, data) {
        var certain = confirm("Are you sure you want to delete this category?");
        if (certain) {
            var node = $.extend(true, {}, data.node);
            $.ajax({
                type: "POST",
                url: "Categories/Delete",
                data: { id: node.id },
                success: function (obj) {
                    if (obj.Status == true) {
                        notif({
                            msg: "<b>Success :</b> Category Deleted sucessfully.",
                            type: "success",
                        });
                        if (_adjustPageFooter)
                            _adjustPageFooter();
                    } else {
                        notif({
                            msg: "<b>Error :</b> " + obj.Message,
                            type: "error"
                        });
                        window.setTimeout(function () { window.location = window.location; }, 3000);
                    }
                },
                error: function (data) {
                    window.location = window.location;
                }
            });
        }
        else {
            window.location = window.location;
        }
    });
    function myTimer() {
        location.reload();
    }
    $("#demoTree").on("copy_node.jstree", function (event, data) {
        CreateNode(data);
    });

    $("#demoTree").on("rename_node.jstree", function (event, data) {
        var node = $.extend(true, {}, data.node);
        var siblings = $("#" + node.parent).children().siblings("ul").children();
        var position = $.inArray(node.id, $.map(siblings, function (val, i) {
            var ret = val.id;
            return ret;
        }));
        if (isUnique(siblings, node.text, position)) {
            if (node.id != "00000000-0000-0000-0000-000000000000") {
                $.ajax({
                    type: "POST",
                    url: "Categories/Edit",
                    data: { ABC: node.text, id: node.id, parentId: node.parent }
                }).done(function (obj) {
                    notif({
                        msg: "<b>Success :</b> Category Edited sucessfully.",
                        type: "success",
                    });
                    if (_adjustPageFooter)
                        _adjustPageFooter();
                });
            } else {
                notif({
                    msg: "<b>Error :</b> You can't update root node of Category.",
                    type: "error"
                });
            }
        } else {
            notif({
                msg: "<b>Error</b> : Duplicate category detected in node.",
                type: "error"
            });
            window.setTimeout(function () { window.location = window.location; }, 3000);
        }
    });

    $("#demoTree").bind("open_node.jstree", function (event, data) {
        if (_adjustPageFooter)
            _adjustPageFooter();
    });
});