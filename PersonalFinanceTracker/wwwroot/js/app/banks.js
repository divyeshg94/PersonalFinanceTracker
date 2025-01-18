$(function () {
    var grid,
        controller = global.IMoneyPal.controller,
        selectors = {
            gridContainer: "#gridContainer",
            dateRangePicker:"#dateRangePicker",
            refresh: "#refresh",
            integrateButton: "#integrateButton"
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
        return global.IMoneyPal.helper.dateTimeFormat(value);
    }

    function _getActionsFormatter() {
        return '<div class="btn-group"><button class="editBtn btn btn-edit btn-action margin-2" title="edit"><span class="fa fa-pen-to-square"></span></button>' +
            '<button class="deleteBtn btn btn-delete btn-action margin-2" title="delete"><span class="fa fa-trash"></span></button></div>';
    }

    function integrateBank(token) {
        var handler = Plaid.create({
            token: token, 
            onSuccess: function (public_token, metadata) {
                fetch('/bank/ExchangeToken', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ public_token: public_token })
                }).then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                }).then(data => {
                    console.log('Token exchange successful:', data);
                }).catch(error => {
                    console.error('Error during token exchange:', error);
                });
            },
            onExit: function (err, metadata) {
                console.log(err);
            }
        });
    }

    async function exchangePublicToken(public_token) {
        var data = {};
        data["public_token"] = public_token;
        var promise = controller.exchangePublicToken(JSON.stringify(data));
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                console.log("Token exchanged successfully");
                reloadData();
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function getExchangeToken() {
        var promise = controller.createLinkToken();
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                // Initialize Plaid Link
                const handler = Plaid.create({
                    token: data,
                    onSuccess: async function (public_token, metadata) {
                        // Send the public token to the server to exchange it
                        await exchangePublicToken(public_token);
                    },
                    onExit: function (err, metadata) {
                        if (err) {
                            console.error("Plaid Link error:", err);
                            document.getElementById("message").textContent = "Error: " + err.message;
                        } else {
                            console.log("User exited Plaid Link.");
                        }
                    }
                });

                // Open Plaid Link
                handler.open();
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function renderTable() {
        var columns = [
            { title: "Name", data: "name", orderable: true },
            //{ title: "Account Number", data: "accountNumber" },
            //{ title: "Account Holder Name", data: "accountHolderName", orderable: false },
            //{ title: "IFSC", data: "ifsc", orderable: false },
            { title: "Currency", data: "currencyCode", orderable: false },
            { title: "Balance", data: "closingBalance", orderable: false },
            { title: "Created Date", data: "createdDateTime", orderable: false, render: _dateFormatter },
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
                url: "/Bank/GetBanks",
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
                $(".deleteBtn").off("click").on("click", function (i, j) {
                    var selectedIncome = grid.row($(this).parents('tr')).data();
                    if(!!selectedIncome)
                        window.location.href = 'Bank/Delete?id=' + selectedIncome.id;
                });
                $(".editBtn").off("click").on("click", function (i, j) {
                    var selectedIncome = grid.row($(this).parents('tr')).data();
                    if(!!selectedIncome)
                        window.location.href = 'Bank/Edit?id=' + selectedIncome.id;
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
            window.location.href = 'Bank/Add';
        });

        $(selectors.dateRangePicker).off("change").on("change", function () {
            helper.setDateRangePicker();
            $(selectors.gridContainer).DataTable().ajax.reload(null, false);
        });

        $(selectors.refresh).off("click").on("click", reloadData);
        $(selectors.integrateButton).off("click").on("click", getExchangeToken);
    }

    init();
    renderTable();

});