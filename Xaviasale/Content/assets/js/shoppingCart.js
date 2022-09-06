var record = 0;
$(document).ready(function () {
    $("#grid").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: "/umbraco/surface/backofficeshoppingcart/getdata"
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
            width: 50
        },
        {
            field: "FirstName",
            title: "First Name"
        },
        {
            field: "LastName",
            title: "Last Name"
        },
        {
            field: "Email",
            title: "Email"
        },
        {
            title: "Create Date",
            template: "#= kendo.toString(kendo.parseDate(CreateDate), 'dd/MM/yyyy hh:mm') #"
        },
        {
            command:
                [
                    {
                        text: "View Details",
                        click: showDetails
                    },
                    {
                        text: "Delete",
                        click: deleteOrder
                    }
                ], title: " ",
            width: "200px"
        }]
    });
});
function showDetails(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    var dialog = $("#window").data("kendoWindow");
    dialog.refresh({
        url: "/umbraco/surface/backofficeshoppingcart/vieworder/" + dataItem.Id
    });
    dialog.open();
}
function deleteOrder(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    var windowWidget = $("#grid").data("kendoGrid");
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this order!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            kendo.ui.progress(windowWidget.element, true);
            $.ajax({
                type: "POST",
                url: "/umbraco/surface/backofficeshoppingcart/deleteorder",
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

$("#window").kendoWindow({
    title: "Order details",
    modal: true,
    visible: false,
    resizable: false,
    width: "90%",
    position: {
        top: 50,
        left: "5%"
    }
}).data("kendoWindow");