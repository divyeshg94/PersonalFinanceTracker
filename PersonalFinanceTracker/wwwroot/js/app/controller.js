$(function () {

    var urls = {
        getDashboardData: "/home/GetDashboardData",
        getCategoryWiseSpending: "/home/GetCategoryWiseSpendingChart",
        getMonthlyBreakdown: "/home/GetMonthlyBreakdown",
        getIncomeExpenseComparison: "/home/GetIncomeExpenseComparison",
        getPaymentWiseSpendingChart: "/home/GetPaymentWiseSpendingChart",

        //Plaid
        createLinkToken: "/bank/CreateLinkToken",
        exchangeToken: "/bank/ExchangeToken",

        calculateFire: "/FireCalculator/Calculate",
    }

    function getDashboardData(startDate, endDate) {
        return makeAjaxGet(urls.getDashboardData, { startDate: startDate, endDate: endDate });
    }

    function getCategoryWiseSpending(startDate, endDate) {
        return makeAjaxGet(urls.getCategoryWiseSpending, { startDate: startDate, endDate: endDate });
    }

    function getMonthlyBreakdown(month) {
        return makeAjaxGet(urls.getMonthlyBreakdown, { month: month});
    }

    function getIncomeExpenseComparison(startDate, endDate, timeFrame) {
        return makeAjaxGet(urls.getIncomeExpenseComparison, { startDate: startDate, endDate: endDate, timeFrame: timeFrame });
    }

    function getPaymentWiseSpendingChart(startDate, endDate) {
        return makeAjaxGet(urls.getPaymentWiseSpendingChart, { startDate: startDate, endDate: endDate });
    }

    function calculateFire(input) {
        return makeAjaxPost(urls.calculateFire, input);
    }

    //Plaid

    function createLinkToken() {
        return makeAjaxGet(urls.createLinkToken);
    }

    function exchangePublicToken(publicToken) {
        return makeAjaxPost(urls.exchangeToken, publicToken);
    }

    //Plaid End


    function makeAjaxGet(url, data) {
        return makeAjaxCall(url, data, "GET", "json");
    }

    function makeAjaxPost(url, data) {
        return makeAjaxCall(url, data, "POST", "json");
    }

    function makeAjaxPut(url, data) {
        return makeAjaxCall(url, data, "PUT", "json");
    }

    function makeAjaxDelete(url, data) {
        return makeAjaxCall(url, data, "DELETE");
    }

    function makeAjaxCall(url, data, method, dataType) {
        return $.ajax({
            url: url,
            data: data,
            contentType: 'application/json; charset=utf-8',
            method: method,
            dataType: dataType
        });
    }

    var controller = {
        getDashboardData: getDashboardData,
        getCategoryWiseSpending: getCategoryWiseSpending,
        getPaymentWiseSpendingChart: getPaymentWiseSpendingChart,
        getIncomeExpenseComparison: getIncomeExpenseComparison,
        getMonthlyBreakdown: getMonthlyBreakdown,

        //PLAID
        createLinkToken: createLinkToken,
        exchangePublicToken: exchangePublicToken,

        calculateFire: calculateFire
    };

    global = global || {};
    global.IMoneyPal = global.IMoneyPal || {};
    global.IMoneyPal.controller = controller;
});