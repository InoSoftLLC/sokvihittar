using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AllaannonserCrawler
{
    public static class AllaannonserCrawler 
    {
        public static string Search(string text, int limit)
        {
            var searchRequest = new AllaannonserSearchRequest(text, limit);
            var products = searchRequest.ProceedSearchRequest().ToList();

            var serializer = new DataContractJsonSerializer(products.GetType());
            string result;
            using (var stream = new MemoryStream())
            {
            serializer.WriteObject(stream, products);
                var buf = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(buf, 0, buf.Length);
                result = Encoding.UTF8.GetString(buf);
            }
            return result;

        }
    }
}
