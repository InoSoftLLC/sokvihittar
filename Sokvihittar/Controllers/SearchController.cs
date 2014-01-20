using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using Sokvihittar.Crawlers;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Controllers
{
    /// <summary>
    /// Search API
    /// </summary>
    public class SearchController : ApiController
    {
        private object _sync = new object();

        public string LogFileName
        {
            get { return Path.Combine(Path.GetTempPath(), "Sokvihittar", "Real.log"); }
        }
        /// <summary>
        /// Searches for items.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Returns an array of crawler crawlerResults.</returns>
        [HttpGet]
        public HttpResponseMessage Index(string text, int limit, string callBack)
        {
            callBack = callBack.Replace("?", "");
            var response = new HttpResponseMessage();
            var content = new StringBuilder();
            content.Append(callBack).Append(" && ").Append(callBack).Append("(");
            var result = JsonHelper.Serialize(Crawler.Search(text, limit));
            content.Append(result).Append(")");
            response.Content = new StringContent(content.ToString(), new UTF8Encoding());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") {CharSet = "utf-8"};
            return response;
        }


        /// <summary>
        /// Searches for items.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <param name="sources">crawlerSources</param>
        /// <returns>Returns an array of crawler crawlerResults.</returns>
        [HttpGet]
        public HttpResponseMessage Index(string text, int limit, string sources, string callBack)
        {
            callBack = callBack.Replace("?", "");
            var response = new HttpResponseMessage();
            var content = new StringBuilder();
            var sourceList = ParseSources(sources);
            if (sourceList.Count == 0)
            {
                response.Content = new StringContent(String.Format("Invalid crawler sources."), new UTF8Encoding());
                return response;
            }
            content.Append(callBack).Append(" && ").Append(callBack).Append("(");
            var result = JsonHelper.Serialize(Crawler.Search(text, limit, sourceList.ToArray()));
            content.Append(result).Append(")");
            response.Content = new StringContent(content.ToString(), new UTF8Encoding());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "utf-8" };
            return response;

        }

        /// <summary>
        /// Searches for items.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <param name="sources">crawlerSources</param>
        /// <returns>Returns an array of crawler crawlerResults.</returns>
        [HttpGet]
        public HttpResponseMessage Index(string text, int limit, string sources, string strict, string callBack)
        {
            callBack = callBack.Replace("?", "");
            var response = new HttpResponseMessage();
            var content = new StringBuilder();

            var sourceList = ParseSources(sources);
            if (sourceList.Count == 0)
            {
                response.Content = new StringContent(String.Format("Invalid crawler sources."), new UTF8Encoding());
                return response;
            }
            content.Append(callBack).Append(" && ").Append(callBack).Append("(");
            var strictSources = ParseSources(strict);
            var result = JsonHelper.Serialize(Crawler.Search(text, limit, sourceList.ToArray(), strictSources.Count > 0 ? strictSources.ToArray() : null));
            content.Append(result).Append(")");
            response.Content = new StringContent(content.ToString(), new UTF8Encoding());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "utf-8" };
            return response;

        }

        [HttpGet]
        public CrawlerResult[] Index(string text, int limit)
        {
            var response = Crawler.Search(text, limit);
            return response;
        }

        [HttpGet]
        public string Statistics()
        {
            lock (_sync)
            {
                return StatisticsHelper.ReadStatistics(LogFileName);
            }
        }

        private List<CrawlerSource> ParseSources(string sourceString) 
        {
            var sources = new List<CrawlerSource>();
            foreach (var item in sourceString.Split(','))
            {
                int sourceId;
                var flag = Int32.TryParse(item, out sourceId);
                if (!flag)
                {
                    continue;
                }
                var source = (CrawlerSource)sourceId;
                if (!Enum.IsDefined(typeof(CrawlerSource), source))
                {
                    continue;
                }
                sources.Add(source);
            }
            return sources;
        }
    }
}