using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sokvihittar.Crawlers.Common;
using Sokvihittar.Crawlers.Requests.SubRequests;

namespace Sokvihittar.Crawlers.Requests
{
    class BooliCrawlerRequest : ICrawlerRequest
    {
        public BooliCrawlerRequest(string productText, int limit)
        {
            Limit = limit;
            ProductText = productText;

            PropertyTypes = new Dictionary<string, string>
            {
                {"lägenheter","lagenhet"},
                {"lagenheter","lagenhet"},
                {"lägenhet","lagenhet"},
                {"lagenhet","lagenhet"},
                {"villor","villa"},
                {"villa","villa"},
                {"parhus","parhus"},
                {"radhus","radhus"},
                {"kedjehus","kedjehus"},
                {"fritidshus", "fritidshus"},
                {"tomt","tomt-mark"},
                {"mark","tomt-mark"},
                {"Gårdar","gard"},
                {"par","parhus"},
                {"rad","radhus"}
            };
        }

        public string Domain
        {
            get { return "www.booli.se"; }
        }

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public int Id
        {
            get { return 12; }
        }
        public string SourceName
        {
            get { return "Booli"; }
        }

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public Dictionary<string, string> PropertyTypes { get; private set; }

        public ProductInfo[] ExecuteSearchRequest()
        {

            var searchWords = new List<string>();
            var types = new List<string>();
            foreach (var word in ProductText.ToLower().Split(' '))
            {
                if (PropertyTypes.Keys.Contains(word))
                {
                    types.Add(PropertyTypes[word]);
                }
                else
                {
                    searchWords.Add(word);
                }
            }

            var subRequests = searchWords.Select(searchWord => new BooliSubRequest(searchWord, Limit, types));
            var results = new List<ProductInfo>();
            Parallel.ForEach(subRequests, subRequest =>
            {
                try
                {
                    var result = subRequest.ExecuteSearchRequest();
                    lock (results)
                    {
                        results.AddRange(result);
                    }
                }
                catch (Exception)
                {
                }
                
                
            });
            return results.Take(Limit).ToArray();
        }
    }
}
