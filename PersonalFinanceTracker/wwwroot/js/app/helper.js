$(function () {
    var selectors = {
        dateRangePicker: '#dateRangePicker',
    },
        charts = {};

    function dateFormat(date) {
        const format2 = "YYYY-MM-DD"
        return moment(date).format(format2);
    }

    function getCurrency() {
        var userCurrencies = window.userCurrencies;
        if (userCurrencies) {
            let decodedString = userCurrencies.replace(/&quot;/g, '"');
            var currencies = JSON.parse(decodedString);
            if (currencies)
                return currencies[0];
        }
        return "USD";
    }

    function getUserLocale() {
        return navigator.language || navigator.userLanguage || "en-IN";
    }

    function getUserCurrencies(){
        return window.userCurrencies;
    }

    function getFormattedAmount(value) {
        if (!!value) {
            // Convert the string to a number
            let amountNumber = parseFloat(value);

            // Format the number as currency using toLocaleString()
            return amountNumber.toLocaleString('en-US', { style: 'currency', currency: getCurrency() });
        }

        return "-";
    }

    function getFormattedAmountCurrency(value, currency) {
        if (!!value) {
            // Convert the string to a number
            let amountNumber = parseFloat(value);

            if (!!currency)
                currency = getCurrency();

            // Format the number as currency using toLocaleString()
            return amountNumber.toLocaleString('en-US', { style: 'currency', currency: currency });
        }

        return "-";
    }


    function dateTimeFormat(date) {
        const format1 = "YYYY-MM-DD HH:mm:ss"
        return moment(date).format(format1);
    }

    function dateRangePicker(selector) {
        $(selector).daterangepicker({
            // Options for the date range picker
            startDate: moment().subtract(7, 'days'), // Initial start date
            endDate: moment(), // Initial end date
            ranges: {
                'Last 7 Days': [moment().subtract(7, 'days'), moment()],
                'Last 30 Days': [moment().subtract(30, 'days'), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                'This Quater': [moment().startOf('quarter'), moment().endOf('quarter')],
                'Last Quater': [moment().subtract(1, 'quarter').startOf('quarter'), moment().subtract(1, 'quarter').endOf('quarter')],
                'First Half Yearly': [moment().startOf('year'), moment().startOf('year').add(6, 'months').endOf('month')],
                'Second Half Yearly': [moment().startOf('year').add(7, 'months'), moment().endOf('year')],
                'This Year': [moment().startOf('year'), moment().endOf('year')],
                'Last Year': [moment().startOf('year').add(-1, 'year'), moment().endOf('year').add(-1, 'year')]
                // Define more ranges if needed
            }
        });

        var storedDateRange = localStorage.getItem('selectedDateRange');
        if (storedDateRange) {
            storedDateRange = JSON.parse(storedDateRange);
            var startDate = moment(storedDateRange.startDate, 'YYYY-MM-DD'); // Parse the stored date using the format
            var endDate = moment(storedDateRange.endDate, 'YYYY-MM-DD'); // Parse the stored date using the format

            // Set the date range picker's initial values from localStorage
            $(selector).data('daterangepicker').setStartDate(startDate);
            $(selector).data('daterangepicker').setEndDate(endDate);
        }
    }

    function changeChartType(chartId, type) {
        var chart = getChartInstance(chartId);

        if (chart) {
            chart.config.type = type;
            chart.update();
        }
    }

    function getChartInstance(chartId) {
        return charts[chartId] || null;
    }

    function generateChart(values, chartId, chartType, chartTitle, totalValue) {
        
        var brightColors = [
            'rgba(255, 99, 132, 0.7)',   // Bright Pink
            'rgba(54, 162, 235, 0.7)',   // Sky Blue
            'rgba(255, 206, 86, 0.7)',   // Light Yellow
            'rgba(75, 192, 192, 0.7)',   // Turquoise
            'rgba(153, 102, 255, 0.7)',  // Light Purple
            'rgba(255, 159, 64, 0.7)',   // Orange
            'rgba(50, 205, 50, 0.7)',    // Lime Green
            'rgba(255, 0, 0, 0.7)',      // Red
            'rgba(0, 255, 255, 0.7)',    // Cyan
            'rgba(128, 0, 128, 0.7)',    // Purple
            'rgba(255, 105, 180, 0.7)',  // Hot Pink
            'rgba(255, 69, 0, 0.7)',     // Orange Red
            'rgba(0, 128, 128, 0.7)',    // Teal
            'rgba(0, 255, 0, 0.7)',      // Bright Green
            'rgba(255, 20, 147, 0.7)',   // Deep Pink
            'rgba(75, 0, 130, 0.7)',     // Indigo
            'rgba(255, 140, 0, 0.7)',    // Dark Orange
            'rgba(255, 255, 0, 0.7)',    // Yellow
            'rgba(0, 0, 255, 0.7)',      // Blue
            'rgba(238, 130, 238, 0.7)',  // Violet
            'rgba(127, 255, 0, 0.7)',    // Chartreuse
            'rgba(0, 191, 255, 0.7)',    // Deep Sky Blue
            'rgba(255, 127, 80, 0.7)',   // Coral
            'rgba(240, 230, 140, 0.7)',  // Khaki
            'rgba(255, 218, 185, 0.7)'   // Peach Puff
            // Add more colors as needed
        ];

        var myChart = getChartInstance(chartId);
        if (myChart instanceof Chart) {
            myChart.destroy(); // Destroy the existing chart
        }
        charts[chartId] = new Chart(chartId, {
            type: chartType,
            data: {
                labels: values.xValues,
                datasets: [{
                    borderWidth: 1,
                    backgroundColor: brightColors,
                    data: values.yValues
                }]
            },
            options: {
                responsive: true,
                legend: {
                    display: true },
                title: {
                    display: !!chartTitle,
                    text: chartTitle
                }
            },
            //plugins: {
            //    colors: {
            //        enabled: true
            //    }
            //}
        });

        _initChartDropdownEvent();

        return charts[chartId];
    }

    function generateIncomeExpenseComparison(values, chartId, chartType, chartTitle) {
        var incomeData = values.incomeData;
        var expenseData = values.expenseData;

        var myChart = getChartInstance(chartId);
        if (myChart instanceof Chart) {
            myChart.destroy(); // Destroy the existing chart
        }

        var ctx = document.getElementById(chartId).getContext('2d');
        charts[chartId] = new Chart(ctx, {
            type: chartType, // You can use 'line' for a line chart
            data: {
                labels: values.xValues,
                datasets: [
                    {
                        label: 'Income',
                        data: incomeData,
                        backgroundColor: 'rgba(54, 162, 235, 0.5)' // Adjust color as needed
                    },
                    {
                        label: 'Expense',
                        data: expenseData,
                        backgroundColor: 'rgba(255, 99, 132, 0.5)' // Adjust color as needed
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        display: true,
                        title: {
                            display: true,
                            text: 'Timeframe'
                        }
                    },
                    y: {
                        display: true,
                        title: {
                            display: true,
                            text: 'Amount'
                        }
                    }
                },
                title: {
                    display: true,
                    text: chartTitle
                },
            }
        });

        _initChartDropdownEvent();

        return charts[chartId];

    }

    function setDateRangePicker() {
        var dateRangePickerValue = getDateRangePickerValues();
        localStorage.setItem('selectedDateRange', JSON.stringify({ startDate: dateRangePickerValue.StartDate, endDate: dateRangePickerValue.EndDate }));
    }

    function _initChartDropdownEvent() {
        $('.chart-dropdown').off('change').on('change', function () {
            var chartType = this.value;
            var chartId = this.dataset.chartId; // Retrieve the chart ID from the data attribute
            changeChartType(chartId, chartType)
        });
    }

    function getDateRangePickerValues() {
        var startDate = $(selectors.dateRangePicker).data('daterangepicker').startDate.format('YYYY-MM-DD');
        var endDate = $(selectors.dateRangePicker).data('daterangepicker').endDate.format('YYYY-MM-DD');

        return {
            StartDate: startDate,
            EndDate: endDate
        }
    }

    var helper = {
        dateRangePicker: dateRangePicker,
        dateFormat: dateFormat,
        dateTimeFormat: dateTimeFormat,
        getCurrency: getCurrency,
        getFormattedAmount: getFormattedAmount,
        getFormattedAmountCurrency: getFormattedAmountCurrency,
        generateChart: generateChart,
        getDateRangePickerValues: getDateRangePickerValues,
        changeChartType: changeChartType,
        getChartInstance: getChartInstance,
        setDateRangePicker: setDateRangePicker,
        generateIncomeExpenseComparison: generateIncomeExpenseComparison,
        getUserCurrencies: getUserCurrencies,
        getUserLocale: getUserLocale
    };

    global = global || {};
    global.IMoneyPal = global.IMoneyPal || {};
    global.IMoneyPal.helper = helper;
});