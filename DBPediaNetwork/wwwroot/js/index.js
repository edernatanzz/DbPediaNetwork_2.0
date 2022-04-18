var appIndex = {
    init: function () {
        //appIndex.constroiGrafico();
        appIndex.searchFunction();
    },
    constroiGrafico: () => {
        const nodes = [
            { id: 1, label: "UFBA" },
            { id: 2, label: "IC" },
            { id: 3, label: "DCC" },
            { id: 4, label: "Airton" },
            { id: 5, label: "Fred" },
            { id: 6, label: "João Carlos Salles" },
        ];

        const edges = [
            { from: 1, to: 2 },
            { from: 1, to: 6 },
            { from: 2, to: 3 },
            { from: 3, to: 4 },
            { from: 3, to: 5 },
        ];

        // create a network
        var container = document.getElementById("mynetwork");
        var data = {
            nodes: nodes,
            edges: edges,
        };
        var options = {};
        var network = new vis.Network(container, data, options);
    },
    searchFunction: () => {
        $("#btnSearch").click(function () {
            var pesquisa = $("#inpSearch").val().trim();

            const nodes = [];
            nodes.push({ id: 1, label: pesquisa });

            const edges = [];

            $.post("Home/Search",
                {
                    pesquisa: pesquisa
                },
                function (result) {
                    for (i = 0; i < result.length; i++) {
                        let id = (i + 2);
                        let label = result[i].split("?object = ")[1];
                        let node = { id: id, label: label }

                        nodes.push(node);

                        edges.push({ from: 1, to: id });
                    }

                    // create a network
                    var container = document.getElementById("mynetwork");
                    var data = {
                        nodes: nodes,
                        edges: edges,
                    };
                    var options = {};
                    var network = new vis.Network(container, data, options);
                })
                .fail(function (result) {
                    console.log("Ocorreu um erro ao pesquisar.")
                });

        });
    }

};


(function () {
    appIndex.init();
})();