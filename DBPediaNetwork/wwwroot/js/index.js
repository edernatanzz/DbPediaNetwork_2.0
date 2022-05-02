var appIndex = {
    data: null,
    init: function () {
        appIndex.setCanvasHeight();
        appIndex.buttonFunctions();
    },
    setCanvasHeight: function () {
        $(window).resize(function () {
            $('#mynetwork').css("height", 0);
            let height = ($('#mynetwork').offset().top - $('footer').offset().top) * -1;
            $('#mynetwork').css("height", height - 1);
        }).trigger('resize');
    },
    buttonFunctions: () => {
        $("#btnSearch").click(function () {
            var source = $("#inpSearch").val().trim();

            if (source !== "") {
                var pesquisa = "http://dbpedia.org/resource/" + source;
                appIndex.searchPost(pesquisa);
            }
        });

        $("#inpSearch").on('keyup', function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                $("#btnSearch").click();
            }
        });
    },
    searchPost: function (pesquisa, endpoint = "Search") {
        app.preloader("on");

        $.post("Home/" + endpoint,
            {
                pesquisa: pesquisa
            },
            function (result) {

                if (result != null) {

                    // create a network
                    appIndex.data = result;
                    var container = document.getElementById("mynetwork");
                    var options = {};
                    var network = new vis.Network(container, result, options);

                    network.on('click', function (properties) {
                        debugger;
                        var ids = properties.nodes;
                        if (ids > 0) {
                            clickedNode = appIndex.data.nodes[ids - 1];
                            if (clickedNode.source.includes("resource/") && !clickedNode.clicked) {
                                clickedNode.clicked = true;
                                appIndex.searchPost(clickedNode.source, "ExpandChart");
                            }
                            console.log(clickedNode);
                        }
                    });
                }

                app.preloader("off");
            })
            .fail(function (result) {
                console.log("Ocorreu um erro ao pesquisar.");
                app.preloader("off");
            });

    }
};


(function () {
    appIndex.init();
})();