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
        /// Searches for items on http://www.allaannonser.se
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Array of product info models.</returns>
        [HttpGet]        
        public ProductInfo[] Allaannonser(string text, int limit)
        {
            var request = new AllaannonserCrawlerRequest(text, limit);
            return request.ProceedSearchRequest();
        }

        /// <summary>
        /// Searches for product items on http://www.pricerunner.se/
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Array of product info models.</returns>
        [HttpGet]
        public ProductInfo[] Pricerunner(string text, int limit)
        {
            var request = new PriceRunnerCrawlerRequest(text, limit);
            return request.ProceedSearchRequest();
        }

        /// <summary>
        /// Searches for product items on http://www.mascus.se/
        /// </summary>
        /// <param name="text">Search text.</param>
        /// <param name="limit">Product limit count.</param>
        /// <returns>Array of product info models.</returns>
        [HttpGet]
        public ProductInfo[] Mascus(string text, int limit)
        {
            var request = new MascusCrawlerRequest(text, limit);
            return request.ProceedSearchRequest();
        }
    }
}