using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Sokvihittar.Crawlers.Common
{
    public abstract class CrawlerSubRequest : CrawlerRequest
    {
        protected CrawlerSubRequest(string productText, int limit, bool isStrictResults, string fullSerchText)
            : base(productText, limit, isStrictResults)
        {
            FullSerchText = fullSerchText;
        }

        public string FullSerchText { get; private set; }

        protected override IEnumerable<ProductInfo> ProccedResultPage(HtmlDocument htmlDoc)
        {
            var productNodes = new List<HtmlNode>();
            GetProducts(htmlDoc.DocumentNode, ref productNodes);
            var result = new List<ProductInfo>();
            foreach (var productNode in productNodes)
            {
                try
                {
                    var info = GetProductInfoFromNode(productNode);
                    if (IsStrictResults)
                    {
                        if (info.IsStrict(FullSerchText))
                            result.Add(info);
                    }
                    result.Add(info);
                }
                catch (Exception)
                {
                }
            }
            return result;
        }
    }
}