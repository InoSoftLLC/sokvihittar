using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests.SubRequests
{
    internal class BooliSubRequest : CrawlerSubRequest
    {
        private readonly string _firstRequestUrl;

        public BooliSubRequest(string productText, int limit, List<string> propertyTypes, bool stictResults, string fullSerchText)
            : base(productText, limit, stictResults, fullSerchText)
        {
            var url = new StringBuilder();
            url.Append("http://www.booli.se/");
            foreach (var propertyType in propertyTypes)
            {
                url.Append(propertyType).Append("/");
            }
            url.Append(productText);
            _firstRequestUrl = url.ToString();
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
            get { return _firstRequestUrl; }
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

        public int RegionId
        {
            get
            {
                try
                {
                    var link =
                    FirstResponseHtmlDocument.DocumentNode.SelectSingleNode(".//Head/meta[@rel='canonical']")
                        .GetAttributeValue("href", "No value");
                    var elements = link.Split('/');
                    return Int32.Parse(elements[elements.Length - 2]);
                }
                catch (Exception)
                {
                    throw new Exception("Error while determinating region id");
                }
            }
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            throw new Exception("Unexpected usage of method \"GetNonFirstRequestUrl\". Use \"GetNextPageUrl\" instead.");
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productId = node.Id.Replace("id_", "");

            //var nodes = node.SelectNodes(".//td");
            string imageUrl;

            try
            {
                var metaString = node.GetAttributeValue("data-meta", "no metadata");
                if (metaString == "no metadata")
                {
                    throw new Exception("No metadata");
                }
                metaString = metaString.Substring(metaString.IndexOf("primaryImage") + 14);
                metaString = metaString.Substring(0, metaString.IndexOf("\"") - 1);
                imageUrl = metaString.ToLower() == "false"
                    ? String.Format("http://i.bcdn.se/cache/primary_{0}_650x450.jpg", productId)
                    : String.Format("http://i.bcdn.se/cache/{0}_650x450.jpg", metaString);
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var infoNode = node.SelectSingleNode(".//div[@class='desktop']");
            var adressNode = infoNode.SelectSingleNode(".//ul[@class='postAddressCol']");
            var titleNode = adressNode.SelectSingleNode(".//li[@class='addressCell']").SelectSingleNode(".//a");
            var title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid node data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.booli.se{0}", productUrl);
            }
            var propertyType =
                HttpUtility.HtmlDecode(
                    adressNode.SelectSingleNode(".//li[@class='areaCell light']").InnerText).Split('–')[0]
                    .Trim();
            var priceNode = infoNode.SelectSingleNode(".//ul[@class='priceCol']");
            var b = priceNode.ChildNodes.First();
            var price = HttpUtility.HtmlDecode(b.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            if (price == "")
                throw new Exception("Invalid node data");
            var area = HttpUtility.HtmlDecode(
                    adressNode.SelectSingleNode(".//li[@class='areaCell light']").InnerText).Split('–')[1];
            area = HttpUtility.HtmlDecode(area).Trim().Replace("\t", "").Replace("\n", "");
            var location = area == "" ? title : String.Format("{0}, {1}", title, area);
            return new ProductInfo
            {
                ImageUrl = imageUrl,
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = productId,
                Location = location,
                Domain = Domain,
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
                var link = htmlNode.GetElementbyId("pagination").ChildNodes.Last(el => el.Name == "a").GetAttributeValue("href", "null");
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