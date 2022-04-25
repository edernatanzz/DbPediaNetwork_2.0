var appIndex = {
    data: {
        init: function () {
            appIndex.data.nodes = [];
            appIndex.data.edges = [];
        },
        nodes: [],
        edges: []
    },
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

                appIndex.data.init();
                appIndex.data.nodes.push({ id: 1, label: source, originalSource: pesquisa, clicked: true });
                appIndex.searchPost(pesquisa, 1);
            }
        });

        $("#inpSearch").on('keyup', function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                $("#btnSearch").click();
            }
        });
    },
    searchPost: function (pesquisa, idPai) {
        app.preloader("on");

        $.post("Home/Search",
            {
                pesquisa: pesquisa
            },
            function (result) {

                for (i = 0; i < result.length; i++) {
                    let id = (appIndex.data.nodes.length + 1);
                    let originalSource = result[i].replace("?value =", "");
                    let source = originalSource;
                    if (source.includes("resource/")) {
                        source = source.split("resource/")[1];
                    }

                    let node = { id: id, label: source, originalSource: originalSource, clicked: false }

                    appIndex.data.nodes.push(node);

                    appIndex.data.edges.push({ from: idPai, to: id, length: 300 });
                }

                // create a network
                var container = document.getElementById("mynetwork");
                var options = {
                    //physics: {
                    //    stabilization: {
                    //        iterations: 500
                    //    }
                    //}
                };
                var network = new vis.Network(container, appIndex.data, options);

                network.on('click', function (properties) {
                    var ids = properties.nodes;
                    if (ids > 0) {
                        clickedNode = appIndex.data.nodes[ids - 1];
                        if (clickedNode.originalSource.includes("resource/") && !clickedNode.clicked) {
                            clickedNode.clicked = true;
                            appIndex.searchPost(clickedNode.originalSource, clickedNode.id);
                        }
                        console.log(clickedNode);
                    }
                });

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