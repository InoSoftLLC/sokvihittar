using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;
using String = System.String;

namespace Sokvihittar.Crawlers.Requests.SubRequests
{
    public class HastnetCrawlerSubRequest : CrawlerSubRequest
    {
        private readonly string _categotyId;

        public HastnetCrawlerSubRequest(string productText, int limit, string categotyId , bool isStrictResults, string fullSerchText) : base(productText, limit, isStrictResults, fullSerchText)
        {
            _categotyId = categotyId;
        }

        public override int Id
        {
            get { return 12; }
        }

        public override string Domain
        {
            get  { return "hastnet.se"; }
        }

        public override string SourceName
        {
           get { return "Hastner"; }
        }

        protected override string FirstRequestUrl
        {
            get { return GetNonFirstRequestUrl(1); }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("http://hastmarknad.hastnet.se/sok/index.php?start={0}&inriktning={1}&typ=&lanid=0&order=inlagddatum&frmsok={2}", 18*(pageNum-1), _categotyId,HttpUtility.UrlEncode(ProductText, Encoding));
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            string imageUrl;
            try
            {
                var imageNode = node.SelectSingleNode(".//div[@class='post-left']").SelectSingleNode(".//center/a/img");
                imageUrl = imageNode.GetAttributeValue("src", "No image");
                if (imageUrl == "No image" || imageUrl == "http://www.hastnet.se/pageimages/nopicture.gif")
                { throw new Exception("Invalid Product Data"); }
                if (imageUrl[imageUrl.Length - 5] == 's')
                {
                    imageUrl = imageUrl.Substring(0, imageUrl.Length - 5)+imageUrl.Substring(imageUrl.Length-4);
                }
            }
            catch(Exception)
            {
                throw new Exception("Invalid Product Data");
            }
            var UrlNode = node.SelectSingleNode(".//div[@class='post-left']").SelectSingleNode(".//center/a");
            var productUrl = UrlNode.GetAttributeValue("HREF", "No url");
            if (productUrl == "No url")
            {
                throw new Exception("Invalid Product Data");
            }
            productUrl = String.Format("http://hastmarknad.hastnet.se/sok/{0}", productUrl);
            var infoNode = node.SelectSingleNode(".//div[@class='post-right full']");
            var priceNode =
                infoNode.SelectSingleNode(".//div[@class='price-wrap']").SelectSingleNode(".//p").ChildNodes.Last();
            var title = infoNode.SelectSingleNode(".//h3/b").InnerText;
            var locationNode =
                infoNode.SelectNodes(".//p[@class='post-desc']").First();
            var b = locationNode.SelectNodes(".//div[@class='owner']").First().SelectSingleNode(".//a");
            var date =
                infoNode.SelectNodes(".//p[@class='post-desc']")
                    .First()
                    .SelectSingleNode(".//div[@class='datum']")
                    .SelectSingleNode(".//span/font")
                    .InnerText;
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                Id = productUrl.Replace("http://hastmarknad.hastnet.se/sok/annons.php?aid=",""),
                Location = HttpUtility.HtmlDecode(b.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if ((node.GetAttributeValue("class", "No class") == "post-block-out"))
            {
                var childNode = node.SelectSingleNode(".//div[@class='post-block']");
                if (childNode != null && childNode.SelectSingleNode(".//div[@class='post-left']")!=null) 
                {
                    result.Add(childNode);
                }
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}