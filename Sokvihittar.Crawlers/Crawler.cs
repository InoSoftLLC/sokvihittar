using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Sokvihittar.Crawlers.Common;
using Sokvihittar.Crawlers.Requests;

namespace Sokvihittar.Crawlers
{
    public static class Crawler
    {
        /// <summary>
        /// Creates, executes crawler requests. Forms and returns results.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Limit of products per site.</param>
        /// <returns></returns>
        public static CrawlerResult[] Search(string text, int limit)
        {
            var crawlerRequests = new List<Func<string, int, ICrawlerRequest>>
            {
                //Add crawlers here.
                (t, l) => new AllaannonserCrawlerRequest(t, l),
                (t, l) => new MascusCrawlerRequest(t, l),
                (t, l) => new PriceRunnerCrawlerRequest(t, l),
                (t, l) => new BlocketCrawlerRequest(t, l),
                (t, l) => new TraderaCrawlerRequest(t, l),
                (t, l) => new BarnebysCrawlerRequest(t, l),
                (t, l) => new AnnonsborsenCrawlerRequest(t, l),
                (t, l) => new LokusCrawlerRequest(t, l),
                (t, l) => new FyndtorgetCrawlerRequest(t, l),
                (t, l) => new СlassiccarsCrawlerRequest(t, l),
                (t, l) => new UddevallatorgetCrawlerRequest(t, l),
                (t, l) => new BooliCrawlerRequest(t, l)
            };
 
            var results = new List<CrawlerResult>();
            Parallel.ForEach(crawlerRequests, request =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var crawlerRequest = request(text, limit);
                CrawlerResult result = null;
                try
                {
                    var crawlerResult = crawlerRequest.ExecuteSearchRequest();
                    result = new CrawlerResult
                    {
                        Products = crawlerResult,
                        Count = crawlerResult.Length,
                        Name = crawlerRequest.SourceName,
                        State = SearchResultStatus.Success,
                        Id = crawlerRequest.Id
                    };
                }
                catch (Exception ex)
                {
                    result = new CrawlerResult
                    {
                        Count = 0,
                        Name = crawlerRequest.SourceName,
                        Products = new ProductInfo[0],
                        State = SearchResultStatus.Failure,
                        Exception = ex,
                        Id = crawlerRequest.Id
                    };
                }
                finally
                {
                    lock (results)
                    {
                        watch.Stop();
                        result.ExecutionTime = watch.ElapsedMilliseconds;
                        results.Add(result);
                    }
                }
            });
            CrawlerResult[] crawlerResults = results.ToArray();
            Array.Sort(crawlerResults, (x, y) => x.Id - y.Id);
            return crawlerResults;
        }

    }
}
