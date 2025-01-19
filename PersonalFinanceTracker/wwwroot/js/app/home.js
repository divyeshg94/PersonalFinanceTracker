$(function () {
    var grid,
        helper = global.IMoneyPal.helper,
        controller = global.IMoneyPal.controller,
        selectors = {
            gridContainer: "#gridContainer",
            dateRangePicker: '#dateRangePicker',
            refresh: "#refresh",
            totalIncome: "#totalIncome",
            totalExpense: "#totalExpense",
            totalBalance: "#totalBalance",
            totalInvestment: "#totalInvestment",
            netIncome: "#netIncome",
            categoryWiseChart: "chart1",
            paidViaChart: "chart2",
            incomeExpenseChart: "chart3",
            chart3TimeFrame: "#chart3-timeframe",
            chart3Type: "#chart3-type",
            monthSelect : "#month-select",
            breakdownBody: "breakdown-body"
        };

    function renderChart() {
        renderCategoryChart();
        renderPaymentChart();
        renderMonthlyBreakdown();
        renderIncomeExpenseComparisonChart();
    }

    function renderCategoryChart() {
        var dateRange = helper.getDateRangePickerValues();
        var promise = controller.getCategoryWiseSpending(dateRange.StartDate, dateRange.EndDate);
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                helper.generateChart(data, selectors.categoryWiseChart, "doughnut", data.totalValue);
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function renderMonthlyBreakdown() {
        var month = $(selectors.monthSelect).val();
        var promise = controller.getMonthlyBreakdown(month);
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                // Populate the breakdown table
                const breakdownBody = document.getElementById(selectors.breakdownBody);

                // Add Total Income
                const incomeRow = document.createElement('tr');
                incomeRow.innerHTML = `<td>Total Income</td><td>$${data.totalIncome.toFixed(2)}</td>`;
                breakdownBody.appendChild(incomeRow);

                // Add Expenses
                data.expenses.xValues.forEach((category, index) => {
                    const amount = data.expenses.yValues[index];
                    const row = document.createElement('tr');
                    row.innerHTML = `<td>${category}</td><td>$${amount.toFixed(2)}</td>`;
                    breakdownBody.appendChild(row);
                });

                // Add Net Savings
                const savingsRow = document.createElement('tr');
                savingsRow.innerHTML = `<td>Net Savings</td><td>$${data.netSavings.toFixed(2)}</td>`;
                breakdownBody.appendChild(savingsRow);
            }

            $(selectors.monthSelect).off("change").on("change", function () {
                renderMonthlyBreakdown();
            });
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function renderPaymentChart() {
        var dateRange = helper.getDateRangePickerValues();
        var promise = controller.getPaymentWiseSpendingChart(dateRange.StartDate, dateRange.EndDate);
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                helper.generateChart(data, selectors.paidViaChart, "doughnut", data.totalValue);
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function renderIncomeExpenseComparisonChart() {
        var dateRange = helper.getDateRangePickerValues();
        var selectedTimeframe = $(selectors.chart3TimeFrame).val();
        var promise = controller.getIncomeExpenseComparison(dateRange.StartDate, dateRange.EndDate, selectedTimeframe);
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                var selectedType = $(selectors.chart3Type).val();
                helper.generateIncomeExpenseComparison(data, selectors.incomeExpenseChart, selectedType);
            }

            $(selectors.chart3TimeFrame).off("change").on("change", function () {
                renderIncomeExpenseComparisonChart();
            });
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function renderData() {
        var dateRange = helper.getDateRangePickerValues();
        var promise = controller.getDashboardData(dateRange.StartDate, dateRange.EndDate);
        promise.done(function (data, textStatus, jqXHR) {
            if (data) {
                $(selectors.totalExpense).html(helper.getFormattedAmount(data.totalExpense));
                $(selectors.totalIncome).html(helper.getFormattedAmount(data.totalIncome));
                $(selectors.netIncome).html(helper.getFormattedAmount(data.totalIncome - data.totalExpense));
                $(selectors.totalBalance).html(helper.getFormattedAmount(data.totalAvailableBalance));
                $(selectors.totalInvestment).html(helper.getFormattedAmount(data.totalInvestment));
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function init() {
        global.IMoneyPal.helper.dateRangePicker(selectors.dateRangePicker);

        $(selectors.dateRangePicker).off("change").on("change", function () {
            helper.setDateRangePicker();
            renderData();
            renderChart();
        });
    }

    init();
    renderChart();
    renderData();
});