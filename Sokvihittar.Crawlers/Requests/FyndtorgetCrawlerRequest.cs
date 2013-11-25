using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class FyndtorgetCrawlerRequest : CrawlerRequest
    {
        public FyndtorgetCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override int Id
        {
            get { return 9; }
        }

        public override string Domain
        {
            get { return "www.fyndtorget.se"; }
        }

        public override string SourceName
        {
            get { return "Fyndtorget"; }
        }

        protected override string FirstRequestUrl
        {
            get { return String.Format("http://www.fyndtorget.se/sokresultat.php?loc=0&kat=0&sokord={0}",HttpUtility.UrlEncode(ProductText, Encoding)); }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("{0}&offset={1}", FirstRequestUrl, 45*(pageNum-1));
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productNodes = node.SelectNodes(".//td");


            string date = productNodes[0].InnerText;
            var urlNode = productNodes[1].SelectSingleNode(".//a");
            if (urlNode == null)
                throw new Exception("Invalid node data");
            string productUrl = urlNode.GetAttributeValue("HREF", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid node data");
            var productId = productUrl.Replace("annons.php?id=", "");
            productUrl = String.Format("http://www.fyndtorget.se/{0}", productUrl);
            string imageUrl;
            try
            {
                imageUrl = urlNode.SelectSingleNode(".//img").GetAttributeValue("SRC", "No image");
                if (imageUrl != "No image")
                {
                    imageUrl = String.Format("http://www.fyndtorget.se/{0}", imageUrl);
                }
            }
            catch
            {
                imageUrl = "No image";
            }
            var title = productNodes[3].SelectSingleNode(".//b/a").InnerText;
            string price;
            try
            {
                price = productNodes[3].SelectSingleNode(".//div").InnerText;
            }
            catch (Exception)
            {
                price = "No price";
            }

            string location= productNodes[4].InnerText;
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title),
                Price = price.Trim().Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = location,
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("role", "no role") == "listitem")
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
            get { return Encoding.GetEncoding(EncodingHelper.CodePages["iso-8859-1"]); }
        }
    }
}
