using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Mvc;
using Sokvihittar.Crawlers;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Controllers
{
    public class HomeController : Controller
    {
        public string LogName
        {
            get { return "Sokvihittar.Test.log"; }
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

            var watch = new Stopwatch();
            watch.Start();
            var searchResult = Crawler.Search(searchText, limit.Value);
            watch.Stop();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                lock (_sync)
                {
                    StatisticsHelper.WriteStatistics(new SearchRequestStatiscs
                    {
                        ExecutionTime = watch.ElapsedMilliseconds,
                        IsTest = true,
                        ProductText = searchText,
                        Limit = limit.Value,
                        Time = DateTime.UtcNow
                    }, LogName);
                }
            });
            return View(searchResult);
        }

        [HttpGet]
        public string Statistics()
        {
            lock (_sync)
            {
                return StatisticsHelper.ReadStatistics(LogName);
            }
        }
    }
}
