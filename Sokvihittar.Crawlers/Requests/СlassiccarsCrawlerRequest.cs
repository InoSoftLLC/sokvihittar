using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class СlassiccarsCrawlerRequest : ICrawlerRequest
    {
        public СlassiccarsCrawlerRequest(string productText, int limit)
        {
            ProductText = productText;
            Limit = limit;
        }

        public Dictionary<СlassiccarsCategory, string> Categories
        {
            get
            {
                return new Dictionary<СlassiccarsCategory, string>
                {
                    {СlassiccarsCategory.Cars, "Bilar"},
                    {СlassiccarsCategory.Parts, "Delar"},
                    {СlassiccarsCategory.Boats, "Båtar"},
                    {СlassiccarsCategory.MotorCycles, "MC"},
                    {СlassiccarsCategory.Mopeds, "Mopender"},
                    {СlassiccarsCategory.Trailers, "Husvagnar"},
                };
            }
        }

        public  int Id
        {
            get { return 10; }
        }

        public  string Domain
        {
            get { return "www.classiccars.se"; }
        }

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public  string SourceName
        {
            get { return "Сlassiccars"; }
        }

        public Encoding Encoding { get { return Encoding.UTF8; } }

        public ProductInfo[] ProceedSearchRequest()
        {
            var subRequests =
                Categories.Values.Select(category => new СlassiccarsCrawlerSubRequest(ProductText, Limit, category))
                    .ToList();
            var result = new List<ProductInfo>();
            Parallel.ForEach(subRequests, subRequest => result.AddRange(subRequest.ProceedSearchRequest()));
            return result.Take(Limit).ToArray();
        }
    }
}
