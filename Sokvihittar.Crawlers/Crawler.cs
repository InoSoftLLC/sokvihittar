using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// <param name="sources">Array of crawler sources.</param>
        /// <returns></returns>
        public static CrawlerResult[] Search(string text, int limit, CrawlerSource[] sources = null)
        {
            var crawlerRequests = new List<Func<string, int, ICrawlerRequest>>();
            if (sources != null)
            {
                crawlerRequests.AddRange(sources.Select(crawlerSource => _crawlerRequests[crawlerSource]));
            }
            else
            {
                crawlerRequests = _crawlerRequests.Values.ToList();
            }

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

        private static Dictionary<CrawlerSource, Func<string, int, ICrawlerRequest>> _crawlerRequests
        {
           get
            {
                return new Dictionary<CrawlerSource, Func<string, int, ICrawlerRequest>>
                {
                    //Add crawlers here.
                    {
                        CrawlerSource.Allaannonser,
                        (t, l) => new AllaannonserCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Pricerunner,
                        (t, l) => new PriceRunnerCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Mascus,
                        (t, l) => new MascusCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Tradera,
                        (t, l) => new TraderaCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Blocket,
                        (t, l) => new BlocketCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Barnebys,
                        (t, l) => new BarnebysCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Annonsborsen,
                        (t, l) => new AnnonsborsenCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Lokus,
                        (t, l) => new LokusCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Fyndtorget,
                        (t, l) => new FyndtorgetCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Classiccars,
                        (t, l) => new СlassiccarsCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Uddevallatorget,
                        (t, l) => new UddevallatorgetCrawlerRequest(t, l)
                    },
                    {
                        CrawlerSource.Booli,
                        (t, l) => new BooliCrawlerRequest(t, l)
                    },
                };
            }
        }

    }
}
