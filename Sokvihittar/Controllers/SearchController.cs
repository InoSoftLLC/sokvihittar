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
        /// <summary>
        /// Searches for items.
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Returns an array of crawler results.</returns>
        [HttpGet]
        public CrawlerResult[] Search(string text, int limit)
        {
            return Crawler.Search(text, limit);
        }
    }
}