


$(document).ready(function () {
    $("#demoTree").jstree({
        json_data: {
            data: treeModel,

        },

        "core": { "initially_open": ["#00000000-0000-0000-0000-000000000000"] },
        "plugins": ["themes", "json_data", "ui", "crrm", "dnd", "search"],

    });

    //-----------------new End------------------------------

    $("#demoTree").bind("select_node.jstree", function (evt, data) {

        $('#LoadingImageDiv').show();
        var catId = data.rslt.obj[0].id;
        $.ajax({
            type: "POST",
            url: methodUrlTree,

            data: { catId: catId },
            success: function (result) {

                var catName = $($('#' + catId).find('a')[0]).text();
                if (catName.trim() == "All Categories") {
                    $("#playbookCategory").text("Currently viewing playbooks in all categories");
                }
                else {
                    $("#playbookCategory").text("Currently viewing playbooks in category: " + catName.trim());
                }
                $('#dataTables-example_wrapper').empty();
                $('#dataTables-example1_wrapper').empty();
                $('.PlaybookData').html(result);


                $('#LoadingImageDiv').hide();
            },
            error: function (data) {
            }
        });

    }
);

    $("#demoTree").bind("open_node.jstree", function (event, data) {
        data.rslt.obj.siblings(".jstree-open").each(function () {
            data.inst.close_node(this, true);
        });
    });


    $("#demoTree").bind("hover_node.jstree", function (event, data) {

        var temp = $(data.rslt.obj[0]).find('a:first').text();
        $(data.rslt.obj[0]).find('a').attr('title', temp);
    });

});



