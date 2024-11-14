var dataTable;

$(document).ready(function () {

    //can be used to specifically check only the URL query parameter
    //we check to see what the current order status has been selected
    var url = new URLSearchParams(window.location.search);
    let statusValue = url.get("status");

    //we pass this status value to the datatable to load the list
    loadDataTable(statusValue);
});

function loadDataTable(statusValue) {
    dataTable = $('#tblOrderData').DataTable({
        order:[[0, 'desc']],
        "ajax": {
            "url": '/Order/GetAllOrders?status=' + statusValue,
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
                    <a href="/order/OrderDetails?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>`
                }
            },
        ]
    });
}