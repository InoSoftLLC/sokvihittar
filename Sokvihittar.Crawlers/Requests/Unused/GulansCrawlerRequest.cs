using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests.Unused
{
    public class GulansCrawlerRequest : CrawlerRequest
    {
        private readonly string _updateProductText;

        public GulansCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
            bool flag = true;
            var chars = new List<char>();
            foreach (char c in productText)
            {
                if (Char.IsDigit(c) || Char.IsLetter(c))
                {
                    chars.Add(c);
                    flag = true;
                }
                else
                {
                    if (flag)
                    {
                        chars.Add('-');
                        flag = false;
                    }
                }
            }
            _updateProductText = new string(chars.ToArray()).ToLower();
        }



        public override string SourceName
        {
            get { return "Gulans"; }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("http://www.gulans.se/{0}.html", _updateProductText);
                }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("http://www.gulans.se/{0}.html?page={1}", _updateProductText, pageNum);
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var imageUrlNode = node.SelectSingleNode(".//div[@class='search_result_item_image']").SelectSingleNode(".//img");
            string imageUrl = imageUrlNode == null ? "No image" : imageUrlNode.GetAttributeValue("src", "No image");
            if (imageUrl.StartsWith("/"))
            {
                imageUrl = String.Format("http://www.gulans.se{0}", imageUrl);
            }
            var idNode =
                node.SelectSingleNode(".//div[@class='search_result_item_image']").ChildNodes.First(el=>el.Name=="div");
            if (idNode==null)
                throw new Exception("Invalid Product Data");
            var productId = idNode.GetAttributeValue("data-value", "No id");
            if (productId == "No id")
                throw new Exception("Invalid Product Data");
            var titleNode = node.SelectSingleNode(".//a");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid Product Data");
            productUrl = String.Format("http://www.gulans.se{0}", productUrl);
            var priceNode = node.SelectSingleNode(".//span[@class='price']");
            var title = titleNode.InnerText;
            var location = node.ChildNodes.Last(el=>el.Name=="span").InnerText.Split('|').First().Trim();
            var date = node.SelectSingleNode(".//div[@class='search_result_item_image']").SelectSingleNode(".//div[@class='date']").ChildNodes.First().InnerText;


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
            get { return "www.gulans.se"; }
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "No class") == "search_result_item_inner")
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
