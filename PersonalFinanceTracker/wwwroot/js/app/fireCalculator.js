$(function () {
    var controller = global.IMoneyPal.controller,
        helpers = global.IMoneyPal.helper,
        selectors = {
            fireForm: "#fireForm",
            calculateBtn: ".calculateBtn"
        };

    function calculateFire(input) {
        var formData = $("#fireForm").serializeArray();
        var data = {};
        $(formData).each(function (index, obj) {
            data[obj.name] = obj.value;
        });

        var promise = controller.calculateFire(JSON.stringify(data));
        promise.done(function (response, textStatus, jqXHR) {
            if (response) {
                var currency = helpers.getCurrency();
                var userLocale = helpers.getUserLocale();

                $('#expenseToday').text(response.expenseToday.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#expenseAtRetirement').text(response.expenseAtRetirement.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#leanFire').text(response.leanFire.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#fire').text(response.fire.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#fatFire').text(response.fatFire.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#coastFire').text(response.coastFire.toLocaleString(userLocale, { style: 'currency', currency: currency }));
                $('#fireResults').show();
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(textStatus);
        });
    }

    function init() {
        $(selectors.calculateBtn).off("click").on("click", function (i, j) {
            calculateFire(this);
        });
    }

    init();
});