$(function () {
    var controller = global.IMoneyPal.controller,
        helpers = global.IMoneyPal.helper,
        selectors = {
            fireForm: "#fireForm",
            calculateBtn: ".calculateBtn"
        };

    function calculateRetirement() {
        const annualIncome = parseFloat(document.getElementById('annualIncome').value);
        const annualExpenses = parseFloat(document.getElementById('annualExpenses').value);
        const currentSavings = parseFloat(document.getElementById('currentSavings').value);
        const savingsRate = parseFloat(document.getElementById('savingsRate').value) / 100;
        const roi = parseFloat(document.getElementById('roi').value) / 100;
        const withdrawalRate = parseFloat(document.getElementById('withdrawalRate').value) / 100;

        const annualSavings = annualIncome * savingsRate;
        let years = 0;
        let totalSavings = currentSavings;
        let yearlyData = [];
        let chartData = {
            labels: [],
            data: []
        };

        document.getElementById('yearlyData').innerHTML = '';
        document.getElementById('table-section').style.display = 'none';
        document.getElementById('savingsChart').style.display = 'none';

        while (totalSavings * withdrawalRate < annualExpenses) {
            let investmentGrowth = totalSavings * roi;
            totalSavings += annualSavings + investmentGrowth;

            yearlyData.push({
                year: years + 1,
                savingsStart: totalSavings - annualSavings - investmentGrowth,
                annualSavings: annualSavings,
                investmentGrowth: investmentGrowth,
                totalSavings: totalSavings
            });

            chartData.labels.push(`Year ${years + 1}`);
            chartData.data.push(totalSavings);

            const row = `
                            <tr>
                                <td>${years + 1}</td>
                                <td>${(totalSavings - annualSavings - investmentGrowth).toFixed(2)}</td>
                                <td>${annualSavings.toFixed(2)}</td>
                                <td>${investmentGrowth.toFixed(2)}</td>
                                <td>${totalSavings.toFixed(2)}</td>
                            </tr>
                        `;
            document.getElementById('yearlyData').innerHTML += row;
            years++;
        }

        document.getElementById('yearsToRetire').innerText = years + " Years";
        document.getElementById('totalSavings').innerText = helpers.getFormattedAmount(totalSavings);
        document.getElementById('table-section').style.display = 'block';
        document.getElementById('savingsChart').style.display = 'block';

        renderChart(chartData);
    }

    function renderChart(chartData) {
        const ctx = document.getElementById('savingsChart').getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: chartData.labels,
                datasets: [{
                    label: 'Total Savings Over Time',
                    data: chartData.data,
                    borderColor: '#007bff',
                    fill: false
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Total Savings ($)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Years'
                        }
                    }
                }
            }
        });
    }

    function init() {
        $(selectors.calculateBtn).off("click").on("click", function (i, j) {
            calculateRetirement();
        });
    }

    init();
});