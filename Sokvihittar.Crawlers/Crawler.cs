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
        public static CrawlerResult[] Search(string text, int limit, CrawlerSource[] sources = null, CrawlerSource[] strictSearchSources = null)
        {
            CrawlerSource[] crawlerSources = sources ?? DefaultCrawlerSources;
            var crawlerRequests = crawlerSources.ToDictionary(crawlerSource => crawlerSource,
                crawlerSource => CrawlerRequests[crawlerSource]);
            var results = new Dictionary<CrawlerSource, CrawlerResult>();
            Parallel.ForEach(crawlerRequests, request =>
            {
                var watch = new Stopwatch();
                watch.Start();
                bool isStrictSearch = strictSearchSources != null && strictSearchSources.Contains(request.Key);
                var crawlerRequest = request.Value(text, limit, isStrictSearch);
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
                        results.Add(request.Key, result);
                    }
                }
            });
            var crawlerResults = new List<CrawlerResult>();
            crawlerResults.AddRange(crawlerSources.Select(source => results[source]));
            return crawlerResults.ToArray();
        }

        private static Dictionary<CrawlerSource, Func<string, int, bool, ICrawlerRequest>> CrawlerRequests
        {
            get
            {
                return new Dictionary<CrawlerSource, Func<string, int, bool, ICrawlerRequest>>
                {
                    {
                        CrawlerSource.Allaannonser,
                        (t, l, s) => new AllaannonserCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Pricerunner,
                        (t, l, s) => new PriceRunnerCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Mascus,
                        (t, l, s) => new MascusCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Tradera,
                        (t, l, s) => new TraderaCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Blocket,
                        (t, l, s) => new BlocketCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Barnebys,
                        (t, l, s) => new BarnebysCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Annonsborsen,
                        (t, l, s) => new AnnonsborsenCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Lokus,
                        (t, l, s) => new LokusCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Fyndtorget,
                        (t, l, s) => new FyndtorgetCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Classiccars,
                        (t, l, s) => new СlassiccarsCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Uddevallatorget,
                        (t, l, s) => new UddevallatorgetCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Booli,
                        (t, l, s) => new BooliCrawlerRequest(t, l, s)
                    },
                    {
                        CrawlerSource.Hastnet,
                        (t, l, s) => new HastnetCrawlerRequest(t, l, s) 
                    },
                };
            }
        }

        private static CrawlerSource[] DefaultCrawlerSources
        {
            get
            {
                return new[]
                {
                    CrawlerSource.Allaannonser,
                    CrawlerSource.Pricerunner,
                    CrawlerSource.Mascus,
                    CrawlerSource.Tradera,
                    CrawlerSource.Blocket,
                    CrawlerSource.Barnebys,
                    CrawlerSource.Annonsborsen,
                    CrawlerSource.Lokus,
                    CrawlerSource.Fyndtorget,
                    CrawlerSource.Classiccars,
                    CrawlerSource.Uddevallatorget,
                    CrawlerSource.Booli,
                    CrawlerSource.Hastnet, 
                };
            }
        }
    }
}
