using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class AnnonsborsenCrawlerRequest : ICrawlerRequest
    {
        public AnnonsborsenCrawlerRequest(string productText,int limit )
        {
            ProductText = productText;
            Limit = limit;
        }

        public int Id { get { return 7; } }

        public string Domain { get { return "www.annonsborsen.se"; } }

        public Encoding Encoding 
        {
            get { return Encoding.GetEncoding(EncodingHelper.CodePages["iso-8859-1"]); }
        }

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public string SourceName { get { return "Annonsborsen"; } }

        public ProductInfo[] ProceedSearchRequest()
        {
            var jSessionId = Search();
            int pageNum = 1;
            var prevResult = ProccedResultPage(GetSearchResult(jSessionId,pageNum));

            var products = new List<ProductInfo>();
            products.AddRange(prevResult);

            while (products.Count < Limit)
            {
                pageNum++;    
               
                ProductInfo[] newProducts = ProccedResultPage(GetSearchResult(jSessionId, pageNum)).ToArray();
                if (newProducts.Length == 0)
                {
                    break;
                }
                if (newProducts[0].Id == prevResult[0].Id)
                {
                    break;
                }
                products.AddRange(newProducts);
                prevResult = newProducts;

            }
            return products.Take(Limit).ToArray();
        }

        private ProductInfo GetProductInfoFromNode(HtmlNode productNode)
        {
            HtmlNodeCollection productNodes = productNode.SelectNodes(".//td");

            var urlsNode = productNodes[0];
            var productUrlNode = urlsNode.SelectSingleNode(".//a");
            if (productUrlNode == null)
                throw new Exception("Invalid Product Data");
            var id = productUrlNode.GetAttributeValue("id", "No id");
            if (id == "No id")
                throw new Exception("Invalid Product Data");
            id = id.Replace("advert_", "");
            var productUrl = productUrlNode.GetAttributeValue("href", "No Url");
            if (productUrl == "No url")
                throw new Exception("Invalid Product Data");
            if (productUrl.StartsWith("/"))
                productUrl = String.Format("http://www.annonsborsen.se{0}", productUrl);
            var imageUrlNode = productUrlNode.SelectSingleNode(".//img");
            string imageUrl = imageUrlNode == null ? "No image" : imageUrlNode.GetAttributeValue("src", "No image");
            if (imageUrl.StartsWith("/"))
                imageUrl = String.Format("http://www.annonsborsen.se{0}", imageUrl);
            string title = productNodes[1].InnerText.Trim().Replace("\t", "").Replace("\n", "") + " " + productNodes[2].InnerText.Trim().Replace("\t", "").Replace("\n", "");
            if (title.EndsWith(","))
            {
                title = title.Substring(0, title.Length - 1);
            }
            string location = productNodes[5].InnerText.Trim().Replace("\t", "").Replace("\n", ""); ;
            var price = HttpUtility.HtmlDecode(productNodes[6].InnerText).Trim().Replace("\t", "").Replace("\n", "");
            if (price.Contains("Kampanjpris"))
            {
                price = price.Substring(price.IndexOf("Kampanjpris") + 13);
            }
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = id,
                Location = location,
                Domain = Domain
            };
        }

        private void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "No class").Contains("searchList"))
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }

        private string GetSearchResult(Cookie c, int pageNum)
        {
            var url = pageNum == 1
                ? "http://www.annonsborsen.se/search/searchFast.jspx?"
                : String.Format("http://www.annonsborsen.se/search/searchFast.jspx?offset={0}", 20 * (pageNum - 1));

          var resp =  WebRequestHelper.GetResponse(url, null, new Cookie[]{c});
            return WebRequestHelper.GetResponseHtml(resp, Encoding);
        }

        private ProductInfo[] ProccedResultPage(string searchResult)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(searchResult);
            var productNodes = new List<HtmlNode>();
            GetProducts(htmlDoc.DocumentNode, ref productNodes);
            var result = new List<ProductInfo>();
            foreach (var productNode in productNodes)
            {
                try
                {
                    result.Add(GetProductInfoFromNode(productNode));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result.ToArray();
        }
        private Cookie Search()
        {
            var postText =String.Format(
                    "fastSearchCountries=29&fastSearchCountries=26&fastSearchCountries=27&fastSearchCountries=28&query={0}",
                    HttpUtility.UrlEncode(ProductText,Encoding));
            var response = WebRequestHelper.GetPostResponse("http://www.annonsborsen.se/search/searchFast!save.jspx",
                postText);
            var cookie =  response.Cookies["JSESSIONID"];
            response.Dispose();
            return cookie;
        }
    }
}
