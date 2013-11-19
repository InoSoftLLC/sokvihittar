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
                        State = CrawlerRequestState.Success
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
                        Exception = ex 
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
            return result.ToArray();
        }

    }
}
