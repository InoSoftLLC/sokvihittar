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
    class LokusCrawlerRequest : CrawlerRequest
    {
        /// <summary>
        /// Headers of search requests.
        /// </summary>
        public Dictionary<HttpRequestHeader, string> RequestHeaders
        {
            get
            {
                return new Dictionary<HttpRequestHeader, string>()
                {
                    {
                        HttpRequestHeader.Accept,
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
                    },
                    {HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch"},
                    {HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4"}
                };
            }
        }

        public LokusCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 8; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.lokus.se" ; }
        }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName
        {
            get { return "Lokus" ; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get
            {
                var postText =
                    String.Format(
                        "__VIEWSTATE=%2FwEPDwULLTIwMzU1ODc0NTFkGAIFHl9fQ29udHJvbHNSZXF1aXJlUG9zdEJhY2tLZXlfXxYBBUhjdGwwMCRmdWxsUmVnaW9uJGNvbnRlbnRSZWdpb25GdWxsJFNlYXJ" +
                        "jaEZvcm0xJFNlYXJjaEZvcm0xJHNlYXJjaEJ1dHRvbjEFOmN0bDAwJGZ1bGxSZWdpb24kY29udGVudFJlZ2lvbkZ1bGwkU2VhcmNoRm9ybTEkU2VhcmNoRm9ybTEPFCsAA2ZlAv%2F%2F" +
                        "%2F%2F8PZG6DXr0Gbym66BLqIZAnjHaimCaX&__EVENTVALIDATION=%2FwEWBwLc9v6cDgLgienOBQK1u4KPDgKb%2BY%2FmCwKV6KXhBwK1%2B7WbBgKOmvfrCVCbvyvhlIVP9k3Mhe" +
                        "ZOq7LzcCFQ&ctl00%24fullRegion%24contentRegionFull%24SearchAdStatus1%24cgAdStatusId_Select=0&ctl00%24fullRegion%24contentRegionFull%24SearchFo" +
                        "rm1%24SearchForm1%24freetext={0}&ctl00%24fullRegion%24contentRegionFull%24SearchForm1%24SearchForm1%24ProductTypeId=0&ctl00%24fullRegion%24co" +
                        "ntentRegionFull%24SearchForm1%24SearchForm1%24cgCountyId=0&ctl00%24fullRegion%24contentRegionFull%24SearchForm1%24SearchForm1%24searchButton1" +
                        ".x=44&ctl00%24fullRegion%24contentRegionFull%24SearchForm1%24SearchForm1%24searchButton1.y=17&ModalTextbox=&ModalSelect=100",
                        HttpUtility.UrlEncode(ProductText, Encoding));
                var response = WebRequestHelper.GetPostResponse("http://www.lokus.se/startsida.aspx?mode=1", postText);
                var url = response.Headers.Get("Location");
                response.Dispose();
                return String.Format("http://www.lokus.se{0}", url);
            }
        }

        /// <summary>
        /// Executes search request, forms and returns product information models.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        public override ProductInfo[] ExecuteSearchRequest()
        {
            var products = new List<ProductInfo>();
            var url = FirstRequestUrl;
            var firstResponse = WebRequestHelper.GetResponse(url, RequestHeaders);
            var firstResponseHtmlDocument = new HtmlDocument();
            firstResponseHtmlDocument.LoadHtml(WebRequestHelper.GetResponseHtml(firstResponse,Encoding.UTF8));
            products.AddRange(ProccedResultPage(firstResponseHtmlDocument).ToArray());
            try
            {
                var i = 2;
                var responseHtmlDocument = firstResponseHtmlDocument;
                while (products.Count < Limit)
                {
                    var requestUrl = GetNextPageRequestUrl(responseHtmlDocument);
                    if (requestUrl == null)
                        break;
                    var response = WebRequestHelper.GetResponse(requestUrl,RequestHeaders);
                    responseHtmlDocument = new HtmlDocument();
                    responseHtmlDocument.LoadHtml(WebRequestHelper.GetResponseHtml(response, Encoding));
                    ProductInfo[] newProducts = ProccedResultPage(responseHtmlDocument).ToArray();
                    if (newProducts.Length == 0)
                    {
                        break;
                    }
                    products.AddRange(newProducts);
                    i++;
                }
            }
            catch (Exception)
            {

            }
            return products.Take(Limit).ToArray();
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            throw new Exception("Unexpected usage of method \"GetNonFirstRequestUrl\". \"GetNextPageRequestUrl\" method should be used instead.");
        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            String imageUrl;
            try
            {
                
                var imageNode =
                    node.SelectSingleNode(".//div[@class='image']").Descendants("img").First();;
                imageUrl = imageNode.GetAttributeValue("src", "No image");
                if (imageUrl.StartsWith("/"))
                {
                    imageUrl = String.Format("http://www.lokus.se{0}", imageUrl);
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var productNode = node.SelectSingleNode(".//div[@class='containerItem clearfix']").SelectSingleNode(".//div[@class='leftside']");
            if (productNode==null)
                throw new Exception("Invalid Product Data");
            var dateNode = productNode.SelectSingleNode(".//p[@class='date']");
            string date = dateNode == null ? "No date" : dateNode.InnerText;
            var titleNode = productNode.SelectSingleNode(".//a");
            if (titleNode==null)
                throw new Exception("Invalid Product Data");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl=="No url")
                throw new Exception("Invalid Product Data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.lokus.se{0}", productUrl);
            }


            string title = titleNode.InnerText;
            string price = productNode.ChildNodes.Last(el=>el.Name=="p").InnerText;
            string location;
            try
            {
                location= node.SelectSingleNode(".//div[@class='containerItem clearfix']")
                        .SelectSingleNode(".//div[@class='rightside']")
                        .SelectSingleNode(".//p[@class='fact']").InnerText;
            }
            catch (Exception)
            {
                location = "No location";
            }

            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title).Trim(),
                Price = HttpUtility.HtmlDecode(price).Trim().Replace("\t", "").Replace("\n", ""),
                Id = "No id",
                Location = HttpUtility.HtmlDecode(location).Trim().Replace("\t", "").Replace("\n", ""),
                Domain = Domain
            };
        }

        /// <summary>
        /// Get html nodes conatinig information about products.
        /// </summary>
        /// <param name="node">Html node of search result page.</param>
        /// <param name="result">List of html nodes conatinig information about products.</param>
        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            if (node.GetAttributeValue("class", "No class") == "hrefAddQueryStringToDiv searchListItem clearfix")
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref result);
            }
        }

        /// <summary>
        /// Gets requst url to next search result page.
        /// </summary>
        /// <param name="doc">Html document containing last search result page response.</param>
        /// <returns>String containig url to next search result page.</returns>
        private string GetNextPageRequestUrl(HtmlDocument doc)
        {
            try
            {
                var paggingNode =
                    doc.GetElementbyId("ctl00_fullRegion_contentRegionFull_searchRegionCenter_SearchPaging2_lblPagesHTML");
                var nextPageNode = paggingNode.ChildNodes.Last(el => el.Name == "a");
                if (nextPageNode.GetAttributeValue("title", "No title") == "Gå till nästa sida")
                {
                    var url = nextPageNode.GetAttributeValue("href", "No url");
                    if (url == "No url")
                    {
                        return null;
                    }
                    return String.Format("http://www.lokus.se{0}", url);
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
