using DBPediaNetwork.Models;
using DBPediaNetwork.Models.vis.js;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace DBPediaNetwork.Controllers
{
    public class HomeController : Controller
    {
        private const int EDGE_LENGTH = 300;
        private const string KEY_NETWORK_DATA = "networkData";
        private readonly ILogger<HomeController> _logger;
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult Search(string pesquisa)
        {
            Data netWorkData = new Data();
            string dbr = pesquisa.Split("resource/")[1];

            // Adiciona o node principal das pesquisas
            Node nodePrincipal = new Node();
            nodePrincipal.id = netWorkData.getNodeId();
            nodePrincipal.label = GetResourceLabel(pesquisa);
            nodePrincipal.source = pesquisa;
            nodePrincipal.clicked = true;
            nodePrincipal.idDad = null;

            netWorkData.nodes.Add(nodePrincipal);

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpPost]
        public ActionResult ExpandChart(string pesquisa)
        {
            var str = HttpContext.Session.GetString(KEY_NETWORK_DATA);
            Data netWorkData = JsonConvert.DeserializeObject<Data>(str);
            string dbr = pesquisa.Split("resource/")[1];

            var nodePrincipal = netWorkData.nodes.Where(w => w.source == pesquisa).FirstOrDefault();
            nodePrincipal.clicked = true;

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpPost]
        public ActionResult RemoveNode(int id)
        {
            var str = HttpContext.Session.GetString(KEY_NETWORK_DATA);
            Data netWorkData = JsonConvert.DeserializeObject<Data>(str);

            var nodePrincipal = netWorkData.nodes.Where(w => w.id == id).FirstOrDefault();


            removeNode(nodePrincipal, ref netWorkData);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        private void removeNode(Node node, ref Data netWorkData)
        {
            foreach (var item in netWorkData.nodes.Where(w => w.idDad == node.id).ToList())
            {
                removeNode(item, ref netWorkData);
            }

            netWorkData.nodes.Remove(node);

            foreach (var item in netWorkData.edges.Where(w=> w.to == node.id || w.from == node.id).ToList())
            {

                netWorkData.edges.Remove(item);
            }
        }

        private void PerformeQueryBuildData(string dbr, ref Data netWorkData, Node nodeDad)
        {

            string color = getColor();
            string query = "select distinct ?value where { " +
                           "dbr:" + dbr + " ?property ?value . " +
                           "filter ( ?property not in ( rdf:type ) ) } " +
                           "limit 200";

            List<string> resultString = new List<string>();

            SparqlResultSet results = ExecutSPARQLQuery(query);
            foreach (SparqlResult result in results.Where(w => w.ToString().Contains("resource/")).ToList().Take(5))
            {
                resultString.Add(result.ToString());
            }

            string value = string.Empty;
            for (int i = 0; i < resultString.Count(); i++)
            {
                value = resultString[i].Replace("?value =", "");

                Node node = new Node();
                node.id = netWorkData.getNodeId();
                node.label = GetResourceLabel(value);
                node.source = value;
                node.color = color;
                node.idDad = nodeDad.id;

                netWorkData.nodes.Add(node);
                netWorkData.edges.Add(new Edge
                {
                    from = node.id,
                    to = nodeDad.id,
                    length = EDGE_LENGTH,
                    color = node.color
                });
            }

            // Pinta o node do para para a nova cor
            netWorkData.nodes.Where(w => w.id == nodeDad.id).FirstOrDefault().color = color;
        }

        private string GetResourceLabel(string dbr)
        {
            string label = string.Empty;
            if (dbr.Contains("resource"))
            {
                string query = "select ?label " +
                               "where { " +
                               "dbr:" + dbr.Split("resource/")[1] + " rdfs:label ?label . " +
                               "}";

                SparqlResultSet results = ExecutSPARQLQuery(query);
                if (results != null && (results?.Where(w => w.ToString().Contains("@en")).Count() ?? 0) > 0)
                {
                    var labelResult = results.Where(w => w.ToString().Contains("@en")).FirstOrDefault();
                    label = labelResult.ToString().Replace("?label =", "").Replace("@en", "");
                }
                else
                {
                    label = dbr.Split("resource/")[1];
                }

            }
            return label;
        }

        private SparqlResultSet ExecutSPARQLQuery(string query)
        {
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlResultSet results = null;
            try
            {
                results = endpoint.QueryWithResultSet(NormalizaQuery(query));
            }
            catch (Exception e)
            {
                var teste = e.Message;
            }
            return results;
        }

        private string getColor()
        {
            string color = string.Empty;
            List<string> arrColors = new List<string> { "#97C2FC", "#FFFF00", "#FB7E81", "#7BE141", "#6E6EFD", "#C2FABC", "#FFA807", "#6E6EFD" };
            List<string> arrColorsUsed = new List<string>();

            var arr1 = HttpContext.Session.GetString("arrColors");
            var arr2 = HttpContext.Session.GetString("arrColorsUsed");

            if (arr1 != null && arr2 != null)
            {
                arrColors = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("arrColors"));
                arrColorsUsed = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("arrColorsUsed"));
            }

            if (arrColors.Count == 0)
            {
                arrColors = arrColorsUsed.GetRange(0, arrColorsUsed.Count);
                arrColorsUsed.Clear();
            }
            Random rnd = new Random();
            int index = rnd.Next(arrColors.Count());
            color = arrColors[index];
            arrColors.Remove(color);
            arrColorsUsed.Add(color);

            HttpContext.Session.SetString("arrColors", JsonConvert.SerializeObject(arrColors));
            HttpContext.Session.SetString("arrColorsUsed", JsonConvert.SerializeObject(arrColorsUsed));

            return color;
        }

        private string NormalizaQuery(string query)
        {
            return query.Replace(",", "\\,");
        }
    }
}
