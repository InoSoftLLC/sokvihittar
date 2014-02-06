using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sokvihittar.Crawlers.Common;
using Sokvihittar.Crawlers.Requests.SubRequests;

namespace Sokvihittar.Crawlers.Requests
{
    class HastnetCrawlerRequest : ICrawlerRequest
    {
        public HastnetCrawlerRequest(string productText, int limit, bool isStrictResults)
        {
            Limit = limit;
            ProductText = productText;
            IsStrictResults = isStrictResults;
            Categories = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"Alla", ""},
                //{"Ridhästar", 7},
                //{"Travhästar", 10},
                //{"Körning", 9},
                //{"Sadlar", 1},
                //{"Hästbussar", 8},
                //{"Hästtransporter", 2}
            });
        }
        public int Id
        {
            get { return 12; }
        }

        public string Domain
        {
            get  { return "hastnet.se"; }
        }

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public string SourceName
        {
            get { return "Hastner"; }
        }
        
        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public bool IsStrictResults { get; private set; }

        public ProductInfo[] ExecuteSearchRequest()
        {
            var subRequests =
            Categories.Values.Select(category => new HastnetCrawlerSubRequest(ProductText, Limit, category, IsStrictResults, ProductText))
        .ToList();
            var result = new List<ProductInfo>();
            Parallel.ForEach(subRequests, subRequest => result.AddRange(subRequest.ExecuteSearchRequest()));
            return result.Take(Limit).ToArray();
        }

        public ReadOnlyDictionary<string, string> Categories { get; private set; } 
    }
}
