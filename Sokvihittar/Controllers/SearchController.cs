using System.Web.Http;
using AllaannonserCrawler;

namespace Sokvihittar.Controllers
{
    /// <summary>
    /// Search API
    /// </summary>
    public class SearchController : ApiController
    {
        // GET /api/search/?text=bmv x5
        /// <summary>
        /// Searches for items on http://www.allaannonser.se
        /// </summary>
        /// <param name="text"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ProductInfo[] Get(string text, int limit)
        {
            // тут надо вызов твоей бибилиотеки вставить
            return AllaannonserCrawler.AllaannonserCrawler.Search(text, limit);
        }
    }
}