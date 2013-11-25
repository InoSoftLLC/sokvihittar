using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests.Unused
{
    public class FyndiqCrawlerRequest : CrawlerRequest
    {
        public FyndiqCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override int Id
        {
            get { return 11; }
        }

        public override string Domain
        {
            get { return "fyndiq.se"; }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override string SourceName
        {
            get { return "Fyndiq"; }
        }

        protected override string FirstRequestUrl
        {
            get { return string.Format("http://fyndiq.se/sok/?q={0}&search=enter", HttpUtility.UrlEncode(ProductText)); }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return string.Format("http://fyndiq.se/sok/sida-2/?q={0}&search=enter", HttpUtility.UrlEncode(ProductText));
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productNode = node.SelectSingleNode(".//div[@class='prodbox']");
            var productUrl = productNode.SelectSingleNode(".//a").GetAttributeValue("href", "No url");
            if (productUrl== "No url") 
               throw new Exception("Invalid Product Data");
            productUrl = String.Format("http://fyndiq.se{0}", productUrl);
            var imageUrl = productNode.SelectSingleNode(".//img").GetAttributeValue("src", "No url");
            if (imageUrl == "No url")
                throw new Exception("Invalid Product Data");
            if (imageUrl.StartsWith("/"))
            {
                imageUrl = String.Format("http://fyndiq.se{0}", imageUrl);
            }
            var title = productNode.SelectSingleNode(".//div[@class='prodtitle']").InnerText;
            var priceNode =
                productNode.SelectSingleNode(".//div[@class='pricetag']").SelectNodes(".//div").First();
            var productId = node.ChildNodes.First(el => el.Id == "prod_id").InnerText.Split('-')[0];
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title).Trim().Replace("\t", "").Replace("\n", ""),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = HttpUtility.HtmlDecode("No location"),
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "no class") == "grid_1 prod")
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }
    }
}
