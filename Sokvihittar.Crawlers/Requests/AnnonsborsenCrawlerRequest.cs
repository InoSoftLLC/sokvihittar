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
    class AnnonsborsenCrawlerRequest : ICrawlerRequest
    {
        public AnnonsborsenCrawlerRequest(string productText, int limit, bool isStrictResults)
        {
            ProductText = productText;
            Limit = limit;
            IsStrictResults = isStrictResults;
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public int Id { get { return 7; } }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public string Domain { get { return "www.annonsborsen.se"; } }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public Encoding Encoding 
        {
            get { return Encoding.GetEncoding(EncodingHelper.CodePages["iso-8859-1"]); }
        }

        public bool IsStrictResults { get; private set; }

        /// <summary>
        /// Needed product count.
        /// </summary>
        public int Limit { get; private set; }

        /// <summary>
        /// Search text.
        /// </summary>
        public string ProductText { get; private set; }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public string SourceName { get { return "Annonsborsen"; } }

        /// <summary>
        /// Executes search request, forms and returns product information models.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        public ProductInfo[] ExecuteSearchRequest()
        {
            var jSessionId = Search();
            int pageNum = 1;
            var prevResult = ProccedResultPage(GetSearchRequestResponse(jSessionId,pageNum));

            var products = new List<ProductInfo>();
            products.AddRange(prevResult);

            while (products.Count < Limit)
            {
                pageNum++;    
               
                ProductInfo[] newProducts = ProccedResultPage(GetSearchRequestResponse(jSessionId, pageNum)).ToArray();
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

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="productNode">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
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
            string location = productNodes[5].InnerText.Trim().Replace("\t", "").Replace("\n", "");
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
                Title = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = id,
                Location = location,
                Domain = Domain
            };
        }

        /// <summary>
        /// Get html nodes conatinig information about products.
        /// </summary>
        /// <param name="node">Html node of search result page.</param>
        /// <param name="result">List of html nodes conatinig information about products.</param>
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

        /// <summary>
        /// Gets search request response.
        /// </summary>
        /// <param name="jSessionId">Cookie containing jSessionid of search session on source website.</param>
        /// <param name="pageNum">Page number.</param>
        /// <returns>String containing html response.</returns>
        private string GetSearchRequestResponse(Cookie jSessionId, int pageNum)
        {
            var url = pageNum == 1
                ? "http://www.annonsborsen.se/search/searchFast.jspx?"
                : String.Format("http://www.annonsborsen.se/search/searchFast.jspx?offset={0}", 20 * (pageNum - 1));

            var resp = WebRequestHelper.GetResponse(url, null, new[] { jSessionId });
            return WebRequestHelper.GetResponseHtml(resp, Encoding);
        }

        /// <summary>
        /// Get product information from search result page.
        /// </summary>
        /// <param name="htmlDoc">Html document</param>
        /// <param name="searchResult">Search text.</param>
        /// <returns>Returns collection of models containing information about product.</returns>
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
                    var info = GetProductInfoFromNode(productNode);
                    if (IsStrictResults &&!info.IsStrict(ProductText))
                    {
                        continue;
                    }
                    result.Add(info);
                }
                catch (Exception)
                {
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Execute search on source website.  
        /// </summary>
        /// <returns>Returns cookie containing jSession id of search session on source website.</returns>
        private Cookie Search()
        {
            var postText =String.Format(
                    "fastSearchCountries=29&query={0}",
                    HttpUtility.UrlEncode(ProductText,Encoding));
            var response = WebRequestHelper.GetPostResponse("http://www.annonsborsen.se/search/searchFast!save.jspx",
                postText);
            var cookie =  response.Cookies["JSESSIONID"];
            response.Dispose();
            return cookie;
        }
    }
}
