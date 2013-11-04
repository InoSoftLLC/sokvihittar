using System.Web.Http;

namespace Sokvihittar.Controllers
{
    public class SearchController : ApiController
    {
        // GET /api/search/?text=bmv x5
        public string Get(string text, int limit)
        {
            // тут надо вызов твоей бибилиотеки вставить
            return AllaannonserCrawler.AllaannonserCrawler.Search(text, limit);
        }
    }
}