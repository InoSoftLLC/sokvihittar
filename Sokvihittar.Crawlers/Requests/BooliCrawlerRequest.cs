using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class BooliCrawlerRequest : CrawlerRequest
    {
        public BooliCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        public override string Domain
        {
            get { return "www.booli.se"; }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override int Id
        {
            get { return 12; }
        }
        public override string SourceName
        {
            get { return "Booli"; }
        }

        protected override string FirstRequestUrl
        {
            get { return String.Format("http://www.booli.se/{0}", ProductText); }
        }

        public override ProductInfo[] ExecuteSearchRequest()
        {
            var products = new List<ProductInfo>();
            var prevResult = ProccedResultPage(FirstResponseHtmlDocument).ToArray();
            var prevResponse = FirstResponseHtmlDocument;
            products.AddRange(prevResult);
            while (products.Count < Limit)
            {

                var requestUrl = GetNextPageUrl(prevResponse);
                if (requestUrl == null)
                    break;
                string currentResponse = WebRequestHelper.GetResponseHtml(GetSearchResultPage(requestUrl), Encoding);
                ProductInfo[] newProducts =
                    ProccedResultPage(currentResponse)
                        .ToArray();
                if (newProducts.Length == 0)
                {
                    break;
                }
                if (newProducts[0].Id == prevResult[0].Id && newProducts[0].Id != "No id")
                {
                    break;
                }
                products.AddRange(newProducts);
                prevResult = newProducts;
                prevResponse = new HtmlDocument();
                prevResponse.LoadHtml(currentResponse);

            }
            return products.Take(Limit).ToArray();
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            throw new Exception("Unexpected usage of method \"GetNonFirstRequestUrl\". Use \"GetNextPageUrl\" instead.");
        }
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productId = node.Id.Replace("id_", "");
            var nodes = node.SelectNodes(".//td");
            string imageUrl;
            try
            {
                var imageNode = nodes[0].SelectSingleNode(".//div[@class='thumbBorder']").SelectSingleNode(".//a/img");
                imageUrl = imageNode.GetAttributeValue("src", "No image");
                if (imageUrl.StartsWith("/"))
                {
                    imageUrl = String.Format("http://www.booli.se{0}", imageUrl);
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var adressNode = nodes[1].SelectSingleNode(".//div[@class='addressCell']");
            var title = adressNode.SelectSingleNode(".//meta[@itemprop='streetAddress']").GetAttributeValue("content", "No title");
            if (title == "No title")
                throw new Exception("Invalid node data");
            var area = adressNode.SelectSingleNode(".//meta[@itemprop='addressLocality']").GetAttributeValue("content", "No title");
            area = HttpUtility.HtmlDecode(area).Trim().Replace("\t", "").Replace("\n", "");
            var productUrl = adressNode.SelectSingleNode(".//a").GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid node data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.booli.se{0}", productUrl);
            }
            var priceNode = nodes[2].SelectSingleNode(".//div");
            var location = area == "" ? title : String.Format("{0}, {1}", title, area);
            return new ProductInfo
            {
                ImageUrl = "No image",
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = location,
                Domain = Domain
            };
        }

        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.Id.Contains("id_"))
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

        private string GetNextPageUrl(HtmlDocument htmlNode)
        {
            try
            {
                var link =  htmlNode.GetElementbyId("pagination").ChildNodes.Last(el => el.Name == "a").GetAttributeValue("href", "null");
                if (link.StartsWith("/"))
                {
                    link = String.Format("http://www.booli.se{0}", link);
                }
                return link;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}