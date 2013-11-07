using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers
{
    public class AllaannonserCrawlerRequest : CrawlerRequest
    {
        private readonly string _firstRequestUrl;

        public AllaannonserCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
            _firstRequestUrl = String.Format("http://www.allaannonser.se/annonser/?q={0}&sort=last",
                HttpUtility.UrlEncode(ProductText));
            FirstResponseHtml =   new HtmlDocument();
            FirstResponseHtml.LoadHtml(WebRequestHelper.GetResponseHtml(_firstRequestUrl));
        }



        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            var url = String.Format("{0}&o={1}", _firstRequestUrl, pageNum);
            return url;
        }
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            try
            {
                var id = Int32.Parse(node.Id.Replace("hitlist_row_", ""));
                var imageNode =
                    node.SelectNodes(".//td").SingleOrDefault(l =>
                        l.Id == String.Format("hitlist_column_{0}_0", id) ||
                        l.Id == String.Format("hitlist_column_{0}_0", id + 1))
                        .SelectSingleNode(".//a/img");
                string imageUrl = imageNode != null ? imageNode.GetAttributeValue("src", "No image") : "No image";
                var titleNode = node.SelectSingleNode(String.Format(".//td[@id='hitlist_column_{0}_1']", id))
                    .SelectSingleNode((".//span[@class='link_row']"))
                    .SelectNodes(".//a")
                    .SingleOrDefault(el => el.Id.Contains("hitlist_title"));
                if (titleNode == null) throw new Exception("Invalid Product Data");
                var productUrl = titleNode.GetAttributeValue("href", "No url");
                if (productUrl == "No url")
                    throw new Exception("Invalid Product Data");
                var productId = String.Format("item_{0}", productUrl.Replace("/click.php?id=", ""));
                productUrl = String.Format("http://www.allaannonser.se{0}", productUrl);
                var title = titleNode.InnerText;
                if (title.Length > 54)
                {
                    title = title.Substring(0, 54) + "...";
                }
                var priceNode =
                    node.SelectSingleNode(String.Format(".//td[@id='hitlist_column_{0}_1']", id))
                        .SelectSingleNode(".//span[@class='prc']");
                if (priceNode == null)
                    throw new Exception("Invalid Product Data");
                var dateNode =
                    node.SelectSingleNode(String.Format(".//td[@id='hitlist_column_{0}_1']", id))
                        .SelectSingleNode(".//p[@class='date']/time");
                string date = dateNode == null ? "No date" : dateNode.GetAttributeValue("datetime", "No date");

                var locationNode = node.SelectSingleNode(".//td[@class='cr']");

                var locationNodeChilds = locationNode.ChildNodes.Where(el => el.Name == "#text").ToArray();
                string location = locationNodeChilds.Length < 2 ? "No location" : locationNodeChilds[1].InnerText;
                return new ProductInfo()
                {
                    ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                    Date = date,
                    ProductUrl = HttpUtility.HtmlDecode(productUrl),
                    Name = HttpUtility.HtmlDecode(title),
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText),
                    Id = productId,
                    Location = HttpUtility.HtmlDecode(location),
                    Domain = "www.allaannonser.se"

                };

            }
            catch (Exception)
            {

                throw;
            }
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.Id.Contains("hitlist_row"))
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