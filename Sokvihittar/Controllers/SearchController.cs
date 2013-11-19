using System.Activities.Expressions;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http;
using dotless.Core.Parser.Tree;
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
        /// <returns>Returns an array of crawler crawlerResults.</returns>
        [HttpGet]
        public HttpResponseMessage Index(string text, int limit, string callBack)
        {
            callBack = callBack.Replace("?", "");
            var response = new HttpResponseMessage();
            var content = new StringBuilder();
            content.Append(callBack).Append(" && ").Append(callBack).Append("(");
            var result = Serialize(Crawler.Search(text, limit));
            content.Append(result).Append(")");
            response.Content = new StringContent(content.ToString(), new UTF8Encoding());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "utf-8" };
            return response;
        }

        private string Serialize(CrawlerResult[] crawlerResults)
        {
            var serializer = new DataContractJsonSerializer(crawlerResults.GetType());
            string result;
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, crawlerResults);
                var buf = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(buf, 0, buf.Length);
                result = Encoding.UTF8.GetString(buf);
            }
            return result;
        }
    }
}