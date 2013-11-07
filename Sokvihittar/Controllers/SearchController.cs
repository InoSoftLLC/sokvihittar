using System.Web.Http;
using AllaannonserCrawler;

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
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public ProductInfo[] Allaannonser(string text, int limit)
        {
            return AllaannonserCrawler.AllaannonserCrawler.Search(text, limit);
        }

        /// <summary>
        /// Searches for items on http://www.pricerunner.se
        /// </summary>
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public ProductInfo[] Pricerunner(string text, int limit)
        {
            return new[]{new ProductInfo() };
        }
    }
}