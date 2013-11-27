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

        /// <summary>
        /// Dictionary of search categories. Key is category, vaiue is its sweden name.
        /// </summary>
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

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public  int Id
        {
            get { return 10; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public  string Domain
        {
            get { return "www.classiccars.se"; }
        }

        /// <summary>
        /// Needed product count.
        /// </summary>
        public int Limit { get; private set; }

        /// <summary>
        /// Search text.
        /// </summary>
        public string ProductText { get; private set; }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public  string SourceName
        {
            get { return "Сlassiccars"; }
        }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public Encoding Encoding { get { return Encoding.UTF8; } }

        /// <summary>
        /// Creates subrequest for each category. executes them, returns results.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        public ProductInfo[] ExecuteSearchRequest()
        {
            var subRequests =
                Categories.Values.Select(category => new СlassiccarsCrawlerSubRequest(ProductText, Limit, category))
                    .ToList();
            var result = new List<ProductInfo>();
            Parallel.ForEach(subRequests, subRequest => result.AddRange(subRequest.ExecuteSearchRequest()));
            return result.Take(Limit).ToArray();
        }
    }
}
