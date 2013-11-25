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
        public MascusCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override string SourceName { get { return "Mascus"; } }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        protected override string FirstRequestUrl
        {
            get { return GetNonFirstRequestUrl(1); }
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {

            HtmlNode table=null;
            GetResultTable(node, ref table);
            if (table != null)
            {
                result.AddRange(table.ChildNodes.Where(el => el.GetAttributeValue("class", "no class") == "row"));
            }
        }

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
                Name = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Replace("\t","").Replace("\n",""),
                Id = productId,
                Location = HttpUtility.HtmlDecode(location.InnerText),
                Domain = Domain

            };
        }

        public override int Id
        {
            get { return 3; }
        }

        public override string Domain
        {
            get { return "www.mascus.se"; }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return string.Format("http://www.mascus.se/{0}/+/+/{1},100,relevance,search.html", HttpUtility.UrlEncode(ProductText), pageNum);
        }
    }
}
