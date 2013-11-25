using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;
using Sokvihittar.Crawlers.Enums;

namespace Sokvihittar.Crawlers.Requests.Unused
{
    public class KvdCrawlerRequest : CrawlerRequest
    {
        public KvdCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override string SourceName
        {
            get { return "Kvd"; }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.GetEncoding(EncodingHelper.CodePages["iso-8859-1"]); }
        }

        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("https://www.kvd.se/sv/auktion/search.html?q={0}",
                    HttpUtility.UrlEncode(ProductText));
            }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return null;
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var subNodes = node.ChildNodes.Where(el=>el.Name =="td").ToArray();
            var titleNode = subNodes[0].SelectSingleNode(".//div[@class ='object_name']");
            if (titleNode == null)
                throw new Exception("Invalid Product Data");
            string title = titleNode.InnerText;
            var productUrlNode = titleNode.ChildNodes.SingleOrDefault(el => el.Name == "a");
            if (productUrlNode == null)
                throw new Exception("Invalid Product Data");
            string productUrl = productUrlNode.GetAttributeValue("href", "No url");
            if(productUrl == "No url")
                throw new Exception("Invalid Product Data");
            productUrl = String.Format("http://www.kvd.se{0}", productUrl);
            var imageUrlNode = subNodes[0].SelectSingleNode(".//img[@class ='obj_images']");
            string imageUrl = imageUrlNode == null ? "No image" : imageUrlNode.GetAttributeValue("title", "No image");
            string productId = node.Id.Replace("object_row_", "");
            var priceNode = subNodes.Where(el => el.GetAttributeValue("class", "no class") == "obj_pres_price").ToArray()[1].ChildNodes.First().ChildNodes.First(el=>el.Name=="#text");
            var dateNode = subNodes.Where(el => el.GetAttributeValue("class", "no class") == "obj_pres").ToArray()[2];
            var date = dateNode.InnerText;
            var location = node.SelectSingleNode(".//span[@class='location2']").InnerText;
            return new ProductInfo()
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = HttpUtility.HtmlDecode(location),
                Domain = Domain
            };
        }

        public override int Id
        {
            get { throw new NotImplementedException(); }
        }

        public override string Domain
        {
            get { return "www.kvd.se"; }
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.Id.Contains("object_row_"))
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
