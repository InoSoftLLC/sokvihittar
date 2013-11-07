using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AllaannonserCrawler
{
    public static class AllaannonserCrawler 
    {
        public static ProductInfo[] Search(string text, int limit)
        {
            var searchRequest = new AllaannonserSearchRequest(text, limit);
            return searchRequest.ProceedSearchRequest();
        }
    }
}
