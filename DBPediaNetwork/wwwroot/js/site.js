var app = {
    init: function () {

    },
    preloader: function (action) {
        var display = $('.preloader').css('display');

        if (action == "on" && (display == "none" || display == undefined)) {
            $('.modal.preloader').modal('show');
        } else if (action == "off") {
            $('.modal.preloader').modal('hide');
        }
    }
};

(function () {
    app.init();
})();
