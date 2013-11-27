using System;
using System.Activities.Expressions;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Web.Http;
using Sokvihittar.Crawlers;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Controllers
{
    /// <summary>
    /// Search API
    /// </summary>
    public class SearchController : ApiController
    {
        public string LogName
        {
            get { return "Sokvihittar.Real.log"; }
        }

        private object _sync = new object();

        /// <summary>
        /// Searches for items.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Returns an array of crawler crawlerResults.</returns>
        [HttpGet]
        public HttpResponseMessage Index(string text, int limit, string callBack)
        {
            var watch = new Stopwatch();
            watch.Start();
            callBack = callBack.Replace("?", "");
            var response = new HttpResponseMessage();
            var content = new StringBuilder();
            content.Append(callBack).Append(" && ").Append(callBack).Append("(");
            var result = JsonHelper.Serialize(Crawler.Search(text, limit));
            content.Append(result).Append(")");
            response.Content = new StringContent(content.ToString(), new UTF8Encoding());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") {CharSet = "utf-8"};
            watch.Stop();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                lock (_sync)
                {
                    StatisticsHelper.WriteStatistics(new SearchRequestStatiscs
                    {
                        ExecutionTime = watch.ElapsedMilliseconds,
                        IsTest = true,
                        ProductText = text,
                        Limit = limit,
                        Time = DateTime.UtcNow
                    }, LogName);
                }
            });
            return response;

        }

        [HttpGet]
        public CrawlerResult[] Index(string text, int limit)
        {
            var watch = new Stopwatch();
            watch.Start();
            var response = Crawler.Search(text, limit);
            watch.Stop();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                lock (_sync)
                {
                    StatisticsHelper.WriteStatistics(new SearchRequestStatiscs
                    {
                        ExecutionTime = watch.ElapsedMilliseconds,
                        IsTest = true,
                        ProductText = text,
                        Limit = limit,
                        Time = DateTime.UtcNow
                    }, LogName);
                }
            });
            return response;
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