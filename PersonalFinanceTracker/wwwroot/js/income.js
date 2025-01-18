$(function () {
    var grid,
        helper = global.IMoneyPal.helper,
        selectors = {
            gridContainer: "#gridContainer",
            dateRangePicker: "#dateRangePicker",
            download: "#download",
            refresh: "#refresh",
            deleteBtn: ".deleteBtn",
            editBtn: ".editBtn"
        };

    function onBeforeAjax(data) {
        data.startDate = $(selectors.dateRangePicker).data('daterangepicker').startDate.format('YYYY-MM-DD');
        data.endDate = $(selectors.dateRangePicker).data('daterangepicker').endDate.format('YYYY-MM-DD');
        data.query = {};
        data.Skip = data.start;
        data.Take = data.length;
        data.Filter = data.search?.value;
        delete data.columns;
    }

    function _dateFormatter(value) {
        return global.IMoneyPal.helper.dateFormat(value);
    }

    function _bankNameFormatter(value) {
        if (!value || !value.name)
            return "-";

        return value.name;
    }

    function downloadExcel() {
        window.location.href = 'Income\\DownloadExcel';
    }

    function _amountFormatter(value, type, row) {
        return helper.getFormattedAmount(value, row.currencyCode)
    }

    function _getActionsFormatter() {
        return '<div class="btn-group"><button class="editBtn btn btn-edit btn-action margin-2" title="edit"><span class="fa fa-pen-to-square"></span></button>' +
            '<button class="deleteBtn btn btn-delete btn-action margin-2" title="delete"><span class="fa fa-trash"></span></button></div>';
    }

    function renderTable() {
        var columns = [
            { title: "Name", data: "name", orderable: true },
            { title: "Amount", data: "amount", orderable: false, render: _amountFormatter },
            { title: "Category", data: "category", orderable: true },
            { title: "Bank Name", data: "bank", orderable: false, render: _bankNameFormatter },
            { title: "Date", data: "incomeDate", orderable: false, render: _dateFormatter },
            { title: "Notes", data: "notes" },
            { title: "Actions", data: "id", orderable: false, render: _getActionsFormatter }
        ];

        grid = $(selectors.gridContainer).DataTable({
            columns: columns,
            processing: true,
            serverSide: true,
            responsive: true,
            paging: true,
            scrollX: false,
            autoWidth: true,
            searching: true,
            stateSave: true,
            ajax: {
                url: "/Income/GetIncome",
                type: "GET",
                cache: false,
                data: onBeforeAjax,
                dataFilter: function (data) {
                    var json = JSON.parse(data);
                    json.recordsTotal = json.count;
                    json.recordsFiltered = json.count;
                    json.data = json.entities;
                    return JSON.stringify(json); // return JSON string
                }
            },
            drawCallback: function () {
                $(selectors.deleteBtn).off("click").on("click", function (i, j) {
                    var selectedIncome = grid.row($(this).parents('tr')).data();
                    if (!!selectedIncome)
                        window.location.href = 'Income/Delete?id=' + selectedIncome.id;
                });
                $(selectors.editBtn).off("click").on("click", function (i, j) {
                    var selectedIncome = grid.row($(this).parents('tr')).data();
                    if (!!selectedIncome)
                        window.location.href = 'Income/Edit?id=' + selectedIncome.id;
                });
            }
        });
    }

    function reloadData() {
        $(selectors.gridContainer).DataTable().ajax.reload(null, false);
    }

    function init() {

        global.IMoneyPal.helper.dateRangePicker(selectors.dateRangePicker);

        document.getElementById('createButton').addEventListener('click', function () {
            window.location.href = 'Income/Add';
        });

        $(selectors.dateRangePicker).off("change").on("change", function () {
            helper.setDateRangePicker();
            $(selectors.gridContainer).DataTable().ajax.reload(null, false);
        });

        $(selectors.refresh).off("click").on("click", reloadData);
        $(selectors.download).off("click").on("click", downloadExcel);
    }

    init();
    renderTable();

});