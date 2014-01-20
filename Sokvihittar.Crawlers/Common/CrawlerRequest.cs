using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace Sokvihittar.Crawlers.Common
{
    /// <summary>
    /// Class containing logic of crawler search request.
    /// </summary>
    public abstract class CrawlerRequest : ICrawlerRequest 
    {
        /// <summary>
        /// Html document containing html text of first result page response.
        /// </summary>
        private HtmlDocument _firstResponseHtml;

        /// <summary>
        /// Url of first result response page.
        /// </summary>
        private string _firstResponseUrl;


        /// <summary>
        /// Creates instance of crawler result class.
        /// </summary>
        /// <param name="productText">Search text.</param>
        /// <param name="limit">Needed product count.</param>
        /// <param name="isStrictResults"></param>
        protected CrawlerRequest(string productText, int limit, bool isStrictResults)
        {
            ProductText = productText;
            Limit = limit;
            IsStrictResults = isStrictResults;
        }
        
        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public abstract string Domain { get; }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public abstract Encoding Encoding { get; }

        public bool IsStrictResults { get; private set; }

        /// <summary>
        /// Html document containing html text of first result page response.
        /// </summary>
        public HtmlDocument FirstResponseHtmlDocument
        {
            get
            {
                if (_firstResponseHtml == null)
                {
                    GetFirstResponse();
                }
                return _firstResponseHtml;
            }
            
        }

        /// <summary>
        /// Url of first result response page.
        /// </summary>
        public string FirstResponseUrl
        {
            get
            {
                if (_firstResponseUrl == String.Empty)
                {
                    GetFirstResponse();
                }
                return _firstResponseUrl;
            }
        }

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
        public abstract string SourceName { get; }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected abstract string FirstRequestUrl { get; }

        /// <summary>
        /// Executes search request, forms and returns product information models.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        public virtual ProductInfo[] ExecuteSearchRequest()
        {
            var products = new List<ProductInfo>();
            var prevResult = ProccedResultPage(FirstResponseHtmlDocument).ToArray();
            products.AddRange(prevResult);
            var i = 2;
            while (products.Count < Limit)
            {
                var requestUrl = GetNonFirstRequestUrl(i);
                if (requestUrl == null)
                    break;
                ProductInfo[] newProducts =
                    ProccedResultPage(WebRequestHelper.GetResponseHtml(GetSearchResultPage(requestUrl), Encoding))
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
                i++;
            }
            return products.Take(Limit).ToArray();
        }

        public virtual HttpWebResponse GetSearchResultPage(string requestUrl)
        {
            return WebRequestHelper.GetResponse(requestUrl);
        }
        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected abstract string GetNonFirstRequestUrl(int pageNum);

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected abstract ProductInfo GetProductInfoFromNode(HtmlNode node);

        /// <summary>
        /// Get html nodes conatinig information about products.
        /// </summary>
        /// <param name="node">Html node of search result page.</param>
        /// <param name="result">List of html nodes conatinig information about products.</param>
        protected abstract void GetProducts(HtmlNode node, ref List<HtmlNode> result);


        /// <summary>
        /// Get product information from search result page.
        /// </summary>
        /// <param name="html">String containg search result page response.</param>
        /// <returns>Returns collection of models containing information about product.</returns>
        protected IEnumerable<ProductInfo> ProccedResultPage(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return ProccedResultPage(htmlDoc);
        }

        /// <summary>
        /// Get product information from search result page.
        /// </summary>
        /// <param name="htmlDoc">Html document</param>
        /// <returns>Returns collection of models containing information about product.</returns>
        protected virtual IEnumerable<ProductInfo> ProccedResultPage(HtmlDocument htmlDoc)
        {
            var productNodes = new List<HtmlNode>();
            GetProducts(htmlDoc.DocumentNode, ref productNodes);
            var result = new List<ProductInfo>();
            foreach (var productNode in productNodes)
            {
                try
                {
                    var info = GetProductInfoFromNode(productNode);
                    if (IsStrictResults && !info.IsStrict(ProductText))
                    {
                        continue;
                    } 
                    result.Add(info);
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        /// <summary>
        /// Gets first search result page response. Sets firs response url and html.
        /// </summary>
        private void GetFirstResponse()
        {
            _firstResponseHtml = new HtmlDocument();
            var firstResponse = GetSearchResultPage(FirstRequestUrl);
            _firstResponseUrl = firstResponse.ResponseUri.OriginalString;
            _firstResponseHtml.LoadHtml(WebRequestHelper.GetResponseHtml(firstResponse, Encoding));
        }
    }
}