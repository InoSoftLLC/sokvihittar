using System.Web.Http;
using AllaannonserCrawler;

namespace Sokvihittar.Controllers
{
    public class SearchController : ApiController
    {
        // GET /api/search/?text=bmv x5
        public ProductInfo[] Get(string text, int limit)
        {
            // тут надо вызов твоей бибилиотеки вставить
            return AllaannonserCrawler.AllaannonserCrawler.Search(text, limit);
        }
    }
}