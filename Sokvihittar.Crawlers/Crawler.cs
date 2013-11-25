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

        public static CrawlerResult[] Search(string text, int limit)
        {
            var crawlers = new List<Func<string, int, ICrawlerRequest>>
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
                
                
            };
 
            var result = new List<CrawlerResult>();
            Parallel.ForEach(crawlers, crawler =>
            {
                var watch = new Stopwatch();
                watch.Start();
                var crawlerRequest = crawler(text, limit);
                CrawlerResult res = null;
                try
                {
                    var crawlerResult = crawlerRequest.ProceedSearchRequest();
                    res = new CrawlerResult
                    {
                        Products = crawlerResult,
                        Count = crawlerResult.Length,
                        Name = crawlerRequest.SourceName,
                        State = CrawlerRequestState.Success,
                        Id = crawlerRequest.Id
                    };
                }
                catch (Exception ex)
                {
                    res = new CrawlerResult
                    {
                        Count = 0,
                        Name = crawlerRequest.SourceName,
                        Products = new ProductInfo[0],
                        State = CrawlerRequestState.Failure,
                        Exception = ex,
                        Id = crawlerRequest.Id
                    };
                }
                finally
                {
                    lock (result)
                    {
                        watch.Stop();
                        res.ExecutionTime = watch.ElapsedMilliseconds;
                        result.Add(res);
                    }
                }
            });
            CrawlerResult[] crawlerResults = result.ToArray();
            Array.Sort(crawlerResults, (x, y) => x.Id - y.Id);
            return crawlerResults;
        }

    }
}
