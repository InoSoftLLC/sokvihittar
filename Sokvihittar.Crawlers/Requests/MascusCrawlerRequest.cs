using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    public class MascusCrawlerRequest : CrawlerRequest
    {
        public MascusCrawlerRequest(string productText, int limit, bool isStrictResults)
            : base(productText, limit, isStrictResults)
        {
        }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName { get { return "Mascus"; } }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get { return GetNonFirstRequestUrl(1); }
        }

        /// <summary>
        /// Get html nodes conatinig information about products.
        /// </summary>
        /// <param name="node">Html node of search result page.</param>
        /// <param name="result">List of html nodes conatinig information about products.</param>
        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            var testNode = node.OwnerDocument.GetElementbyId("searchExalead");
            if (testNode != null)
            {
                if (testNode.ChildNodes.SingleOrDefault(el=>el.Name=="div" && el.GetAttributeValue("class","No class")=="box round_box message_box_not_found")!=null)
                    return;
            }
            HtmlNode table=null;
            GetResultTable(node, ref table);
            if (table != null)
            {
                result.AddRange(table.ChildNodes.Where(el => el.GetAttributeValue("class", "no class") == "row"));
            }
        }

        /// <summary>
        /// Gets result table from search result page.
        /// </summary>
        /// <param name="node">Node containing</param>
        /// <param name="table">List of html nodes conatinig information about product.</param>
        private void GetResultTable(HtmlNode node, ref HtmlNode table)
        {
            if (node.Name == "table" && node.GetAttributeValue("class", "No class") == "search_results")
            {
                table = node;
            }
            else
            {
                foreach (var child in node.ChildNodes)
                {
                    GetResultTable(child, ref table);
                }
            }
        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productIdNode = node.ChildNodes.First().SelectSingleNode(".//input[1]");
            var productId = productIdNode.GetAttributeValue("value", "no value");
            if (productId == "No value")
                throw new Exception("Invalid node data");
            var titleNode = node.SelectSingleNode(".//td[@class='column2']").ChildNodes.First();
            var title = titleNode.GetAttributeValue("title", "No title");
            if (title == "No title")
                throw new Exception("Invalid node data");
            title = title.Split(',')[0];
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (title == "No url")
                throw new Exception("Invalid node data");
            productUrl = String.Format("http://www.mascus.se{0}", productUrl);
            var imageNode = titleNode.SelectSingleNode(".//img");
            string imageUrl = imageNode == null ? "No image" : imageNode.GetAttributeValue("src", "No image");
            if (imageUrl.StartsWith("/"))
                imageUrl = String.Format("http://static.mascus.com{0}", imageUrl);
            var priceNode = node.SelectSingleNode(".//td[@class='column4']");
            var location = node.SelectSingleNode(".//td[@class='column3']").ChildNodes.Last(el=>el.Name=="#text");
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Replace("\t","").Replace("\n",""),
                Id = productId,
                Location = HttpUtility.HtmlDecode(location.InnerText),
                Domain = Domain

            };
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 3; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.mascus.se"; }
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return string.Format("http://www.mascus.se/{0}/+/+/{1},100,relevance,search.html", HttpUtility.UrlEncode(ProductText, Encoding), pageNum);
        }
    }
}
