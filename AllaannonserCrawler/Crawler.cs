using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sokvihittar.Crawlers.Common;
using Sokvihittar.Crawlers.Requests;

namespace Sokvihittar.Crawlers
{
    public class Crawler
    {
        private List<Func<string, int, CrawlerRequest>> _crawlers;

        public Crawler()
        {
            _crawlers= new List<Func<string, int, CrawlerRequest>>
            {
                (t, l) => new AllaannonserCrawlerRequest(t, l),
                (t, l) => new MascusCrawlerRequest(t, l),
                (t, l) => new PriceRunnerCrawlerRequest(t, l),

            };
        }

        public CrawlerResult[] Search(string text, int limit)
        {
            var result = new List<CrawlerResult>();
            Parallel.ForEach(_crawlers, crawler =>
            {
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
                catch (Exception)
                {
                    res = new CrawlerResult
                    {
                        Count = 0,
                        Name = crawlerRequest.SourceName,
                        Products = new ProductInfo[0],
                        State = CrawlerRequestState.Failure
                    };
                }
                finally
                {
                    lock (result)
                    {
                        result.Add(res);
                    }
                }
            });
            return result.ToArray();
        }

    }
}
