using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class BarnebysCrawlerRequest : CrawlerRequest
    {
        public BarnebysCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override int Id
        {
            get { return 6; }
        }

        public override string Domain
        {
            get { return "www.barnebys.se"; }
        }

        public override string SourceName
        {
            get { return "Barnebys"; }
        }

        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("http://www.barnebys.se/a/{0}.html",
                    HttpUtility.UrlEncode(ProductText, Encoding));
            }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("{0}?&o={1}", FirstRequestUrl, (pageNum - 1)*30);
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            string imageUrl;
            try
            {
                var imageNode = node.SelectSingleNode(".//div[@class='img']")
                .ChildNodes.First(el => el.GetAttributeValue("class", "No class") == "img-table-cell")
                .ChildNodes.Single(el => el.Name == "a")
                .ChildNodes.Single(el => el.Name == "img");
                imageUrl = imageNode.GetAttributeValue("src", "No image");
                if (imageUrl.StartsWith("/"))
                {
                    imageUrl = string.Format("http://www.barnebys.se{0}", imageUrl);
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            
            var productNode = node.SelectSingleNode(".//div[@class='text']").SelectSingleNode(".//div[@class='left']");

            if (productNode == null)
                throw new Exception("Invalid Product Data");
            var titleNode = productNode.SelectSingleNode(".//h3/a");
            string productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl== "No url")
                throw new Exception("Invalid Product Data");
            if (productUrl=="javascript:void(0);")
                throw new Exception("Invalid Product Data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = string.Format("http://www.barnebys.se{0}", productUrl);
            }
            string title = titleNode.InnerText.Trim();
            var priceNode = productNode.SelectSingleNode(".//div[@class='price']").SelectSingleNode(".//div[@class='price-wrapper']").ChildNodes.First(el=>el.Name=="p").LastChild;

            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", "").Replace("\t", "").Replace("\n", ""),
                Id = "No id",
                Location = "No location",
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "No class")=="hit" && node.SelectSingleNode(".//div[@class='sold-lot']")==null)
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
