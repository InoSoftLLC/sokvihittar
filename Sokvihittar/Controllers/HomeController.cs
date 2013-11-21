using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sokvihittar.Crawlers;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Controllers
{
    public class HomeController : Controller
    {
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
            return View(Crawler.Search(searchText, limit.Value));
        }
    }
}
