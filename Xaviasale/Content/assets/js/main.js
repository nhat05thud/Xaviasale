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
        groupable: true,
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
        }, {
            field: "Email",
            title: "Email"
        }, {
            field: "PaymentMethod",
            title: "Payment Method"
        },
        {
            title: "Create Date",
            template: "#= kendo.toString(kendo.parseDate(CreateDate), 'dd/MM/yyyy hh:mm') #"
        },
        {
            command: {
                text: "View Details",
                click: showDetails
            }, title: " ",
            width: "180px"
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
    dialog.center().open();
    dialog.maximize();
}
$("#window").kendoWindow({
    width: "95%",
    height: "90%",
    title: "Order details",
    visible: false,
    actions: ["Refresh", "Maximize", "Close"]
}).data("kendoWindow").center();