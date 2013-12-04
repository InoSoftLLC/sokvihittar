using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class UddevallatorgetCrawlerRequest : CrawlerRequest
    {
        public UddevallatorgetCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }


        public override int Id
        {
            get { return 11; }
        }

        public override string Domain
        {
            get { return "http://www.uddevallatorget.se/"; }
        }

        public override string SourceName
        {
            get { return "Uddevallatorget"; }
        }

        protected override string FirstRequestUrl
        {
            get { return GetNonFirstRequestUrl(1); }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return
                String.Format(
                    "http://www.uddevallatorget.se/?page=main&c=0&o=0&t=0&s={0}&i1=0&i2=10000&p2=1&p={1}#menu",
                    HttpUtility.UrlEncode(ProductText, Encoding), pageNum);
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            string imageUrl;
            try
            {
                var imageNode = node.SelectSingleNode(".//div[@class='annons-image']").SelectSingleNode(".//a/img");
                imageUrl = imageNode.GetAttributeValue("src", "No Image");
                if (imageUrl.StartsWith("/"))
                {
                    imageUrl = String.Format("http://www.uddevallatorget.se{0}", imageUrl);
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var infoNode = node.SelectSingleNode(".//div[@class='annons-info']");
            if (infoNode == null)
                throw new Exception("Invalid node data");

            var titleNode = infoNode.SelectSingleNode(".//h2/a");
            if (titleNode == null)
                throw new Exception("Invalid node data");
            var title = titleNode.InnerText;
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid node data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.uddevallatorget.se{0}", productUrl);
            }
            var productId = titleNode.GetAttributeValue("id", "No id");
            var priceNode = infoNode.SelectSingleNode(".//p[@class='annons-price']").SelectSingleNode(".//b");
            var lastNode = infoNode.ChildNodes.Last(el => el.Name == "p");
            var date = lastNode.SelectSingleNode(".//td[@class='date']").InnerText;
            var locationNode = lastNode.SelectSingleNode(".//span");
            string location = locationNode == null ? "No location" : locationNode.InnerText.Trim();

            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = HttpUtility.HtmlDecode(location),
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "No class").Contains("annons row"))
            {
                result.Add(node);
            }
            else
            {
                foreach (var child in node.ChildNodes)
                {
                    GetProducts(child, ref result);
                }
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override HttpWebResponse GetSearchResultPage(string requestUrl)
        {
            int scope = 2;
            if (ProductText.ToLower().Contains("uddevalla"))
                scope = 0;
            else
            {
                if (ProductText.ToLower().Contains("göteborgs"))
                {
                    scope = 1;
                }
            }
            var cookie = new Cookie("scope", scope.ToString(), "/", "www.uddevallatorget.se");
            return WebRequestHelper.GetResponse(requestUrl, null, new[] {cookie});
        }
    }
}
