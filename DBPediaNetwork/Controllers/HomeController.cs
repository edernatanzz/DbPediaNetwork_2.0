using DBPediaNetwork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<HomeController> _logger;

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
            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

            string dbr = pesquisa.Split("resource/")[1];
            //string query = "SELECT  ?predicate ?object WHERE { dbr:" + dbr + " ?predicate ?object} LIMIT 5";

            string query = "select distinct ?value where { " +
                           "dbr:" + dbr + " ?property ?value . " +
                           "filter ( ?property not in ( rdf:type ) ) } ";

            List<string> resultString = new List<string>();



            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet(query);
            foreach (SparqlResult result in results.Where(w => w.ToString().Contains("resource/")).ToList().Take(5))
            {
                    resultString.Add(result.ToString());
            }

            return Json(resultString.Take(5));
        }
    }
}
