using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace Sokvihittar.Crawlers.Common
{
    public abstract class CrawlerRequest 
    {
        protected CrawlerRequest(string productText, int limit)
        {
            ProductText = productText;
            Limit = limit;
        }


        public HtmlDocument FirstResponseHtml { get; protected set; }

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public virtual ProductInfo[] ProceedSearchRequest()
        {
            var products = new List<ProductInfo>();
            products.AddRange(ProccedResultPage(FirstResponseHtml));
            var i = 2;
            while (products.Count < Limit)
            {
                var requestUrl = GetNonFirstRequestUrl(i);
                var response = WebRequestHelper.GetResponse(requestUrl);
                if (response.ResponseUri.OriginalString == requestUrl)
                {
                    var newProducts = ProccedResultPage(WebRequestHelper.GetResponseHtml(response)).ToArray();
                    if (newProducts.Length==0)
                    {
                        break;
                    }
                    products.AddRange(newProducts);
                }
                else
                {
                    break;
                }
                i++;             
            }
            return products.Take(Limit).ToArray();
        }

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


        protected abstract void GetProducts(HtmlNode node, ref List<HtmlNode> result);

        protected abstract ProductInfo GetProductInfoFromNode(HtmlNode node);

        protected abstract string GetNonFirstRequestUrl(int pageNum);
    }
}