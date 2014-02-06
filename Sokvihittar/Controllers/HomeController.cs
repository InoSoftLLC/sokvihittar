using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Sokvihittar.Crawlers;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Controllers
{
    public class HomeController : Controller
    {
        public string LogFileName
        {
            get { return Path.Combine(Path.GetTempPath(), "Sokvihittar", "Test.log"); }
        }

        private object _sync = new object();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Test(string searchText, int? limit)
        {
            if (!limit.HasValue)
            {
                return View(new CrawlerResult[] { });
            }

            ViewBag.SearchText = searchText;
            ViewBag.Limit = limit;

            var sources = new List<CrawlerSource>()
            {
                CrawlerSource.Allaannonser,
                CrawlerSource.Annonsborsen,
                CrawlerSource.Barnebys,
                CrawlerSource.Blocket,
                CrawlerSource.Booli,
                CrawlerSource.Classiccars,
                CrawlerSource.Fyndtorget,
                CrawlerSource.Lokus,
                CrawlerSource.Mascus,
                CrawlerSource.Pricerunner,
                CrawlerSource.Tradera,
                CrawlerSource.Uddevallatorget,
                CrawlerSource.Hastnet,
            }.ToArray();

            var strict = new List<CrawlerSource>()
            {
                CrawlerSource.Allaannonser,
                CrawlerSource.Annonsborsen,
                CrawlerSource.Barnebys,
                CrawlerSource.Blocket,
                CrawlerSource.Booli,
                CrawlerSource.Classiccars,
                CrawlerSource.Fyndtorget,
                CrawlerSource.Lokus,
                CrawlerSource.Mascus,
                CrawlerSource.Pricerunner,
                CrawlerSource.Tradera,
                CrawlerSource.Uddevallatorget,
                CrawlerSource.Hastnet,
            }.ToArray();

            var searchResult = Crawler.Search(searchText, limit.Value, sources, strict);
            return View(searchResult);
        }
    }
}
