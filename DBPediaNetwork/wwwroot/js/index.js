var appIndex = {
    data: null,
    init: function () {
        appIndex.setCanvasHeight();
        appIndex.buttonFunctions();
        appIndex.hideRigthClickMenu();
    },
    setCanvasHeight: function () {
        $(window).resize(function () {
            $('#mynetwork').css("height", 0);
            let height = ($('#mynetwork').offset().top - $('footer').offset().top) * -1;
            $('#mynetwork').css("height", height - 1);
        }).trigger('resize');
    },
    buttonFunctions: () => {
        $("#btnSearch").click(function (e) {
            e.preventDefault();
            e.stopPropagation();

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

        $("#remove").click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            //TODO: Investigar o por que o preloader não fecha se chamando nesse contexto.

            let id = $(this).attr("nodeid");

            $.post("Home/RemoveNode",
                {
                    id: id
                },
                function (result) {
                    appIndex.buildChart(result);

                    $("#menu").css({
                        opacity: "0"
                    });
                    setTimeout(function () {
                        $("#menu").css({
                            visibility: "hidden"
                        });
                    }, 501);
                    app.preloader("off");
                })
                .fail(function (result) {
                    console.log("Ocorreu um erro ao Remover.");
                    app.preloader("off");
                });
        });
    },
    searchPost: function (pesquisa, endpoint = "Search") {
        app.preloader("on");

        $.post("Home/" + endpoint,
            {
                pesquisa: pesquisa
            },
            function (result) {
                appIndex.buildChart(result);
                app.preloader("off");
            })
            .fail(function (result) {
                console.log("Ocorreu um erro ao pesquisar.");
                app.preloader("off");
            });

    },
    buildChart: function (data) {
        if (data != null) {
            appIndex.data = data;

            var container = document.getElementById("mynetwork");
            var options = {};
            var network = new vis.Network(container, data, options);

            network.on('click', function (properties) {
                var nodeid = properties.nodes[0];
                if (nodeid > 0) {
                    clickedNode = appIndex.data.nodes.find(obj => { return obj.id === nodeid });
                    if (clickedNode.source.includes("resource/") && !clickedNode.clicked) {
                        clickedNode.clicked = true;
                        appIndex.searchPost(clickedNode.source, "ExpandChart");
                    }
                    console.log(clickedNode);
                }
            });

            network.on("oncontext", function (params) {
                params.event.preventDefault();
                let nodeid = network.getNodeAt(params.pointer.DOM);
                var clickedNode = appIndex.data.nodes.find(obj => { return obj.id === nodeid });
                if (clickedNode) {
                    if (clickedNode.source.includes("resource/")) {
                        $("#menu #redirectDbpedia").attr("href", clickedNode.source);
                        //$("#menu #redirectDbpedia").css({display: "block"});
                    }
                    else {
                        //$("#menu #redirectDbpedia").css({ display: "none" });
                    }

                    $("#menu #remove").attr("nodeid", clickedNode.id);

                    $("#menu").css({
                        top: params.event.pageY + "px",
                        left: params.event.pageX + "px",
                        visibility: "visible",
                        opacity: "1"
                    });
                }
                else {
                    $("#menu").css({
                        opacity: "0"
                    });
                    setTimeout(function () {
                        $("#menu").css({
                            visibility: "hidden"
                        });
                    }, 501);
                }
            });
        }
    },
    hideRigthClickMenu: function () {
        var i = document.getElementById("menu").style;
        var canvas = document.getElementById("mynetwork");

        if (document.addEventListener) {
            canvas.addEventListener('click', function (e) {
                i.opacity = "0";
                setTimeout(function () {
                    i.visibility = "hidden";
                }, 501);
            }, false);
        } else {
            canvas.attachEvent('onclick', function (e) {
                i.opacity = "0";
                setTimeout(function () {
                    i.visibility = "hidden";
                }, 501);
            });
        }
    }
};


(function () {
    appIndex.init();
})();