
var record = 0;
var recordgroup = 0;
$(document).ready(function () {
    $("#grid").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: "/umbraco/surface/backofficenewsletter/getnewsletter"
            },
            pageSize: 20
        },
        height: 550,
        groupable: false,
        sortable: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 5
        },
        columns: [{
            title: "No.",
            template: "#= ++record #",
            width: 40
        },
        {
            field: "Email",
            title: "Email"
        },
        {
            command: [
            {
                text: "Delete",
                click: deleteEmail
            }], title: "Delete",
            width: 150
        }]
    });
    $("#gridgroup").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: "/umbraco/surface/backofficenewsletter/getgroups"
            },
            pageSize: 20
        },
        height: 550,
        groupable: false,
        sortable: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 5
        },
        columns: [{
            title: "No.",
            template: "#= ++recordgroup #",
            width: 40
        },
        {
            field: "Name",
            title: "Name"
        },
        {
            command: [
            {
                text: "Edit",
                click: showGroupDetails
            },
            {
                text: "Delete",
                click: deleteGroup
            }], title: "Delete",
            width: 200
        }]
    });

    $("#window-sendmail").kendoWindow({
        title: "Send mail",
        modal: true,
        visible: false,
        resizable: false,
        width: "80%",
        position: {
            top: 50,
            left: "10%"
        }
    }).data("kendoWindow");

    $("#window-group").kendoWindow({
        title: "Group",
        modal: true,
        visible: false,
        resizable: false,
        width: "50%",
        position: {
            top: 50,
            left: "25%"
        }
    }).data("kendoWindow");

    $("#emails").kendoMultiSelect({
        placeholder: "Select email",
        dataTextField: "Email",
        dataValueField: "Id",
        autoBind: false,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/umbraco/surface/backofficenewsletter/getlistemailonsendmail",
                }
            }
        }
    });
    $("#groups").kendoMultiSelect({
        placeholder: "Select group",
        dataTextField: "Name",
        dataValueField: "Id",
        autoBind: false,
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/umbraco/surface/backofficenewsletter/getlistgrouponsendmail",
                }
            }
        }
    });
});

function deleteEmail(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    var windowWidget = $("#grid").data("kendoGrid");
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this email!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            kendo.ui.progress(windowWidget.element, true);
            $.ajax({
                type: "POST",
                url: "/umbraco/surface/backofficenewsletter/deletenewsletter",
                data: { id: dataItem.Id },
                success: function (res) {
                    kendo.ui.progress(windowWidget.element, false);
                    if (res.success) {
                        swal("Success!", res.message, "success");
                        windowWidget.dataSource.read();
                        kendo.ui.progress(windowWidget.element, false);
                    }
                    else {
                        swal("Error!", res.message, "error");
                        kendo.ui.progress(windowWidget.element, false);
                    }
                }
            });
        }
    });
}

function deleteGroup(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    var windowWidget = $("#gridgroup").data("kendoGrid");
    var grid = $("#grid").data("kendoGrid");
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this group!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            kendo.ui.progress(windowWidget.element, true);
            $.ajax({
                type: "POST",
                url: "/umbraco/surface/backofficenewsletter/deletegroup",
                data: { id: dataItem.Id },
                success: function (res) {
                    kendo.ui.progress(windowWidget.element, false);
                    if (res.success) {
                        swal("Success!", res.message, "success");
                        windowWidget.dataSource.read();
                        grid.dataSource.read();
                        kendo.ui.progress(windowWidget.element, false);
                    }
                    else {
                        swal("Error!", res.message, "error");
                        kendo.ui.progress(windowWidget.element, false);
                    }
                }
            });
        }
    });
}

function showGroupDetails(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    var dialog = $("#window-group").data("kendoWindow");
    dialog.refresh({
        url: "/umbraco/surface/backofficenewsletter/viewGroup/" + dataItem.Id
    });
    setTimeout(function () {
        dialog.open();
    }, 300)
}

$(".sendmain-btn").bind("click", function () {
    var dialog = $("#window-sendmail").data("kendoWindow");
    dialog.open();
});

$(".addgroup-btn").bind("click", function () {
    var dialog = $("#window-group").data("kendoWindow");
    dialog.refresh({
        url: "/umbraco/surface/backofficenewsletter/viewGroup/" + 0
    });
    dialog.open();
});