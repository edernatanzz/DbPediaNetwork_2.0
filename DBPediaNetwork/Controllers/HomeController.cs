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
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace DBPediaNetwork.Controllers
{
    public class HomeController : Controller
    {
        private const int EDGE_LENGTH = 300;
        private const string KEY_NETWORK_DATA = "networkData";
        private const string KEY_DATABASE = "DATABASE";
        private readonly ILogger<HomeController> _logger;
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            HttpContext.Session.Remove("arrColors");
            HttpContext.Session.Remove("arrColorsUsed");
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
        public ActionResult Search(SearchFilterViewModel filterModel)
        {
            Data netWorkData = new Data();
            string dbr = filterModel.pesquisa.Split("resource/")[1];

            filterModel.qtdRerouces = (filterModel.qtdRerouces < 10 && filterModel.qtdRerouces > 1) ? filterModel.qtdRerouces : 10;
            filterModel.qtdLiterais = (filterModel.qtdLiterais < 10 && filterModel.qtdLiterais > 1) ? filterModel.qtdLiterais : 10;


            // Adiciona o node principal das pesquisas
            Node nodePrincipal = new Node();
            nodePrincipal.id = netWorkData.getNodeId();
            nodePrincipal.label = GetResourceLabel(filterModel.pesquisa);
            nodePrincipal.source = filterModel.pesquisa;
            nodePrincipal.clicked = true;
            nodePrincipal.idDad = null;

            netWorkData.nodes.Add(nodePrincipal);

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal, filterModel);

            HttpContext.Session.SetString(KEY_NETWORK_DATA, JsonConvert.SerializeObject(netWorkData));
            return Json(netWorkData);
        }

        [HttpPost]
        public ActionResult ExpandChart(SearchFilterViewModel filterModel)
        {
            var str = HttpContext.Session.GetString(KEY_NETWORK_DATA);
            Data netWorkData = JsonConvert.DeserializeObject<Data>(str);
            string dbr = filterModel.pesquisa.Split("resource/")[1];

            var nodePrincipal = netWorkData.nodes.Where(w => w.source == filterModel.pesquisa).FirstOrDefault();
            nodePrincipal.clicked = true;

            PerformeQueryBuildData(dbr, ref netWorkData, nodePrincipal, filterModel);

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

            foreach (var item in netWorkData.edges.Where(w => w.to == node.id || w.from == node.id).ToList())
            {

                netWorkData.edges.Remove(item);
            }
        }

        private void PerformeQueryBuildData(string dbr, ref Data netWorkData, Node nodeDad, SearchFilterViewModel filterModel)
        {
            string color = getColor();
            string aux = string.Empty;
            string query = string.Empty;
            string value = string.Empty;
            string[] arrAux = new string[2];
            SparqlResultSet results = new SparqlResultSet();
            List<ResultMainQuerySparqlModel> strDataBase = new List<ResultMainQuerySparqlModel>();
            List<ResultMainQuerySparqlModel> lstResources = new List<ResultMainQuerySparqlModel>();
            List<ResultMainQuerySparqlModel> lstLiterais = new List<ResultMainQuerySparqlModel>();
            Node node = new Node();

            query = "select distinct ?property ?value where { " +
                    "dbr:" + dbr + " ?property ?value . " +
                    "filter ( ?property not in ( rdf:type ) ) } " +
                    "limit 1000";

            results = ExecutSPARQLQuery(query);

            if (results != null)
            {
                foreach (SparqlResult result in results)
                {
                    arrAux = result.ToString().Split(" , ");
                    strDataBase.Add(new ResultMainQuerySparqlModel { property = arrAux[0].Replace("?property =", ""), value = arrAux[1].Replace("?value =", "") });
                }

                lstResources = strDataBase.Where(w => w.value.Contains("resource/")).Take(filterModel.qtdRerouces).ToList();
                lstLiterais = strDataBase.Where(w => !w.value.Contains("http") && (w.value.Contains("@en") || !w.value.Contains("@")) && w.value.Length < 40 && w.property.Contains("property")).Take(filterModel.qtdRerouces).ToList();


                for (int i = 0; i < lstResources.Count(); i++)
                {
                    node = new Node();
                    node.id = netWorkData.getNodeId();
                    node.label = GetResourceLabel(lstResources[i].value);
                    node.source = lstResources[i].value;
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

                for (int i = 0; i < lstLiterais.Count(); i++)
                {
                    node = new Node();
                    node.id = netWorkData.getNodeId();
                    node.label = GetLiteralLabel(lstLiterais[i]);
                    node.source = lstLiterais[i].property;
                    node.color = color;
                    node.idDad = nodeDad.id;
                    node.shape = "box";
                    node.clicked = true;

                    netWorkData.nodes.Add(node);
                    netWorkData.edges.Add(new Edge
                    {
                        from = node.id,
                        to = nodeDad.id,
                        length = EDGE_LENGTH,
                        color = node.color
                    });
                }

                netWorkData.nodes.Where(w => w.id == nodeDad.id).FirstOrDefault().color = color;
            }
        }

        private string GetResourceLabel(string dbr)
        {
            string label = dbr;
            if (label.Contains("resource"))
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

        private string GetLiteralLabel(ResultMainQuerySparqlModel result)
        {
            var aux = result.property.Split("/property/")[1];
            var aux2 = result.value;
            if (aux2.Contains("@"))
            {
                aux2 = aux2.Split("@")[0];
            }

            return aux + ":" + aux2;
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
            List<string> arrColors = new List<string> { "#718baf", "#97C2FC", "#FB7E81", "#7BE141", "#6E6EFD", "#C2FABC", "#FFA807", "#fd6ee5" };
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
