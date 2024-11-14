var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    debugger;
    dataTable = $('#tblOrderData').DataTable({
        "ajax": {
            "url": '/Order/GetAllOrders',
            "dataSrc": 'data',
        },
        "columns": [
            { data: 'orderHeaderId', "width": "5%" },
            { data: 'email', "width": "15%" },
            {
                data: null,
                "width": "15%",
                render: function (data, type, row) {
                    return `${row.firstName} ${row.lastName}`;
                } },
            { data: 'phone', "width": "15%" },
            { data: 'status', "width": "15%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'orderHeaderId',
                "width": "10%",
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href=""/order/orderDetails?orderId=${data} class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>  `
                }
            },
        ]
    });
}