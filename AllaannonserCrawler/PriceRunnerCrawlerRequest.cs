﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers
{
   public class PriceRunnerCrawlerRequest :CrawlerRequest
    {
        private readonly string _firstResponseUrl ;
        private bool _isCategorizied;

        public PriceRunnerCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
            var firstResponse =WebRequestHelper.GetResponse(String.Format(
                    "http://www.pricerunner.se/search?q={0}&numberOfProducts=60",
                    HttpUtility.UrlEncode(ProductText)));
            FirstResponseHtml = new HtmlDocument();
            _firstResponseUrl = firstResponse.ResponseUri.OriginalString;
            FirstResponseHtml.LoadHtml(WebRequestHelper.GetResponseHtml(firstResponse));
        }

       public string GetCategoryLink(HtmlNode node)
       {
           return string.Format("http://www.pricerunner.se{0}", node.SelectSingleNode(".//div[@class='productinfobody']")
               .SelectSingleNode(".//div[@class='productname']")
               .SelectSingleNode(".//h3/a").GetAttributeValue("href", "noLink"));
       }

       public override ProductInfo[] ProceedSearchRequest()
        {
            var firstProductNodes = new List<HtmlNode>();
            var result = new List<ProductInfo>();
            _isCategorizied = CheckIfCategorizied(ref firstProductNodes);
            if (!_isCategorizied)
            {
               return base.ProceedSearchRequest();
            }
            ProccedResultPage(firstProductNodes, result);
            int i = 2;
            while (result.Count < Limit)
            {
                var requestUrl = GetNonFirstRequestUrl(i);
                var response = WebRequestHelper.GetResponse(requestUrl);
                var newProducts = ProccedResultPage(WebRequestHelper.GetResponseHtml(response)).ToArray();
                if (newProducts.Length == 0)
                {
                    break;
                }
                result.AddRange(newProducts);
                i++;
            }
            return result.Take(Limit).ToArray();
        }

       private void ProccedResultPage(List<HtmlNode> productNodes, List<ProductInfo> result)
       {
           foreach (var productNode in productNodes)
           {
               var categoryLink = GetCategoryLink(productNode);
               if (categoryLink == "noLink")
                   continue;
               result.AddRange(GetProductInfos(categoryLink));
               if (result.Count > Limit)
                   break;
           }        
       }

       private ProductInfo[] GetProductInfos(string categoryLink)
       {
           var htmlDoc = new HtmlDocument();
           htmlDoc.LoadHtml(WebRequestHelper.GetResponseHtml(categoryLink));
           var imageNode =
               htmlDoc.DocumentNode.SelectSingleNode(".//head").SelectSingleNode(".//meta[@property='og:image']");
           var imageUrl = imageNode == null ? "No image" : imageNode.GetAttributeValue("content", "No image");
           var productNodes = htmlDoc.GetElementbyId("price-list").SelectSingleNode(".//table[@class='list price-list']").SelectNodes(".//tbody/tr");
           var result = new List<ProductInfo>();
           foreach (var productNode  in productNodes)
           {
               try
               {
                   if (productNode.GetAttributeValue("class", "Mo class") != "Mo class")
                       continue;
                   var retailerNode = productNode.SelectSingleNode(".//td[@class='about-retailer']").SelectSingleNode(".//h4/a") ??
                                      productNode.SelectSingleNode(".//td[@class='about-retailer']").SelectSingleNode(".//h4");
                   var title = retailerNode.InnerText;
                   var productUrl = retailerNode.GetAttributeValue("href", "No link");
                   if (productUrl == "No link")
                       continue;
                   var priceNode = productNode.SelectSingleNode(".//td[@class='price']").SelectNodes(".//div").FirstOrDefault();
                   if (priceNode==null)
                       continue;;
                   var productId = productUrl.Split(';')[2].Replace("oi=", "").Replace("&amp", "");
                   var date = productNode.ChildNodes.Last(el => el.Name == "td").SelectSingleNode(".//p[@class='date']").InnerText;
                   result.Add(new ProductInfo
                   {
                       ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                       Date = date,
                       ProductUrl = HttpUtility.HtmlDecode(productUrl),
                       Name = HttpUtility.HtmlDecode(title),
                       Price = HttpUtility.HtmlDecode(priceNode.InnerText).Replace("\t","").Replace("\n",""),
                       Id = productId,
                       Location = "No location",
                       Domain = "www.pricerunner.se"
                       });
               }
               catch(Exception ex)
               {
                   continue;
               }
           }
           return result.ToArray();
       }

       private bool CheckIfCategorizied(ref List<HtmlNode> productNodes)
        {
            
            GetProducts(FirstResponseHtml.DocumentNode, ref productNodes);
            var node = productNodes.FirstOrDefault();
            if (node == null)
            {
                return false;
            }
            return  node.GetAttributeValue("class", "no attribute")== "product clearfix";
        }


        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            var className = node.GetAttributeValue("class", "no attribute");
            if (className.Contains("product clearfix"))
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }

        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode(".//div[@class='product-wrapper']")
                .ChildNodes.First(el => el.GetAttributeValue("class", "no class") == "no class" && el.Name=="p").SelectSingleNode(".//a[@target='_blank']");
            var title = titleNode.GetAttributeValue("title", "No title");
            if(title == "No title")
                throw new Exception("Invalid node data");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid node data");
            productUrl = String.Format("http://www.pricerunner.se/{0}", productUrl);
            string imageUrl = titleNode.SelectSingleNode(".//img").GetAttributeValue("src", "No image");

            var priceNode = node.SelectSingleNode("//div[@class='product-wrapper']").SelectSingleNode(".//p[@class='price']");
            string productId = String.Format("item_{0}" ,node.Id.Replace("prod-",""));
            return new ProductInfo()
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = "No date",
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Name = HttpUtility.HtmlDecode(title),
                Price =HttpUtility.HtmlDecode(priceNode.InnerText).Trim(),
                Id = productId,
                Location = "No location",
                Domain = "www.pricerunner.se"

            };
        }

        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("{0}@page={1}", _firstResponseUrl, pageNum);
        }
    }
}