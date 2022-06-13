var app = {
    init: function () {

    },
    preloader: function (action, ms = 500) {
        var display = $('.preloader').css('display');

        if (action == "on" && (display == "none" || display == undefined)) {
            $('.modal.preloader').modal('show');
        } else if (action == "off") {

            setTimeout(function () {
            $('.modal.preloader').modal('hide');
            }, ms);
        }
    }
};

(function () {
    app.init();
})();
