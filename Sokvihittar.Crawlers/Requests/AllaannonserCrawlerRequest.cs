using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    public class AllaannonserCrawlerRequest : CrawlerRequest
    {

        public AllaannonserCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }



        public override string SourceName
        {
            get { return "Allaannonser"; }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("http://www.allaannonser.se/annonser/?q={0}&sort=last",
                    HttpUtility.UrlEncode(ProductText));
            }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            var url = String.Format("{0}&o={1}", FirstRequestUrl, pageNum);
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
                if (imageUrl != "No image")
                    imageUrl = imageUrl.Split('&')[0];
                var titleNode = node.SelectSingleNode(String.Format(".//td[@id='hitlist_column_{0}_1']", id))
                    .SelectSingleNode((".//span[@class='link_row']"))
                    .SelectNodes(".//a")
                    .SingleOrDefault(el => el.Id.Contains("hitlist_title"));
                if (titleNode == null) throw new Exception("Invalid Product Data");
                var productUrl = titleNode.GetAttributeValue("href", "No url");
                if (productUrl == "No url")
                    throw new Exception("Invalid Product Data");
                productUrl = String.Format("http://www.allaannonser.se{0}", productUrl);
                var productId = productUrl.Replace("http://www.allaannonser.se/click.php?id=", "");
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
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", "").Replace("\t", "").Replace("\n", ""),
                    Id = productId,
                    Location = HttpUtility.HtmlDecode(location),
                    Domain = Domain
                };

            }
            catch (Exception)
            {

                throw;
            }
        }

        public override int Id
        {
            get { return 1; }
        }

        public override string Domain
        {
            get { return "www.allaannonser.se"; }
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