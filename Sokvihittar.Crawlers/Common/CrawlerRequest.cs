using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Sokvihittar.Crawlers.Common
{
    public interface ICrawlerRequest
    {
        string Domain { get; }

        int Limit { get; }

        string ProductText { get; }

        string SourceName { get; }

        Encoding Encoding { get; }

        ProductInfo[] ProceedSearchRequest();
    }

    public abstract class CrawlerRequest : ICrawlerRequest 
    {
        private HtmlDocument _firstResponseHtml;

        private string _firstResponseUrl;

        protected CrawlerRequest(string productText, int limit)
        {
            ProductText = productText;
            Limit = limit;
        }


        public abstract string Domain { get; }

        public abstract Encoding Encoding { get; }

        public HtmlDocument FirstResponseHtml
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

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public abstract string SourceName { get; }

        protected abstract string FirstRequestUrl { get; }

        public virtual ProductInfo[] ProceedSearchRequest()
        {
            var products = new List<ProductInfo>();
            var prevResult = ProccedResultPage(FirstResponseHtml).ToArray();
            products.AddRange(prevResult);
            var i = 2;
            while (products.Count < Limit)
            {
                var requestUrl = GetNonFirstRequestUrl(i);
                if (requestUrl == null)
                    break;
                var response = WebRequestHelper.GetResponse(requestUrl);
                ProductInfo[] newProducts = ProccedResultPage(WebRequestHelper.GetResponseHtml(response, Encoding)).ToArray();
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
                i++;
            }
            return products.Take(Limit).ToArray();
        }

        protected abstract string GetNonFirstRequestUrl(int pageNum);

        protected abstract ProductInfo GetProductInfoFromNode(HtmlNode node);

        protected abstract void GetProducts(HtmlNode node, ref List<HtmlNode> result);

        protected IEnumerable<ProductInfo> ProccedResultPage(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return ProccedResultPage(htmlDoc);
        }

        protected IEnumerable<ProductInfo> ProccedResultPage(HtmlDocument htmlDoc)
        {
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
            return result;
        }

        private void GetFirstResponse()
        {
                _firstResponseHtml  = new HtmlDocument();
            var a = new Stopwatch();
            a.Start();
                var firstResponse = WebRequestHelper.GetResponse(FirstRequestUrl);
                _firstResponseUrl = firstResponse.ResponseUri.OriginalString;
                _firstResponseHtml.LoadHtml(WebRequestHelper.GetResponseHtml(firstResponse, Encoding));
            a.Stop();


        }
    }
}