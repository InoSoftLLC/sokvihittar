using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
   public class PriceRunnerCrawlerRequest :CrawlerRequest
    {
        private bool _isCategorizied;
       private bool _categorizedDesign;

       public PriceRunnerCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

       public string GetCategoryLink(HtmlNode node)
       {
           return string.Format("http://www.pricerunner.se{0}", node.SelectSingleNode(".//div[@class='productinfobody']")
               .SelectSingleNode(".//div[@class='productname']")
               .SelectSingleNode(".//h3/a").GetAttributeValue("href", "noLink"));
       }

       public override Encoding Encoding
       {
           get { return Encoding.UTF8; }
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
           var doc = new HtmlDocument();
           GetProducts(doc.DocumentNode, ref firstProductNodes);
           var products =  ProccedResultPage(firstProductNodes);
           if (products.Length == 0)
               return new ProductInfo[0];
           result.AddRange(products);
            int i = 2;
            while (result.Count < Limit)
            {
                var requestUrl = GetNonFirstRequestUrl(i);
                var response = WebRequestHelper.GetResponse(requestUrl);
                var newProducts = ProccedResultPage(WebRequestHelper.GetResponseHtml(response, Encoding)).ToArray();
                if (newProducts.Length == 0)
                {
                    break;
                }
                result.AddRange(newProducts);
                i++;
            }
            return result.Take(Limit).ToArray();
        }

       public override string SourceName
       {
           get { return "Pricerunner"; }
       }

       protected override string FirstRequestUrl
       {
           get
           {
               return String.Format(
                   "http://www.pricerunner.se/search?q={0}",
                   HttpUtility.UrlEncode(ProductText, Encoding));
           }
       }

       private ProductInfo[] ProccedResultPage(List<HtmlNode> productNodes)
       {
           var result = new List<ProductInfo>();
           foreach (var productNode in productNodes)
           {
               var categoryLink = GetCategoryLink(productNode);
               if (categoryLink == "noLink")
                   continue;
               ProductInfo[] products = GetProductInfos(categoryLink);
               if (products.Length == 0)
               {
                   break;
               }
               result.AddRange(products);
               if (result.Count > Limit )
                   break;
               
           }
           return result.ToArray();
       }

       private ProductInfo[] GetProductInfos(string categoryLink)
       {
           var htmlDoc = new HtmlDocument();
           htmlDoc.LoadHtml(WebRequestHelper.GetResponseHtml(categoryLink, Encoding));
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
                   if (productUrl.StartsWith("/"))
                   {
                       productUrl = String.Format("http://www.pricerunner.se{0}", productUrl);
                   }
                   var priceNode = productNode.SelectSingleNode(".//td[@class='price']").SelectNodes(".//div").FirstOrDefault();
                   if (priceNode==null)
                       continue;
                   var productId = productUrl.Split(';')[2].Replace("oi=", "").Replace("&amp", "");
                   string date;
                   try
                   {
                       date = productNode.SelectSingleNode(".//td[@class='about-retailer']").SelectSingleNode(".//div[@class='in-stock-date-more']").SelectSingleNode(".//span[@class='date']").InnerText;
                   }
                   catch (Exception)
                   {
                       date = "No date";
                   }
                   
                   result.Add(new ProductInfo
                   {
                       ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                       Date = date,
                       ProductUrl = HttpUtility.HtmlDecode(productUrl),
                       Name = HttpUtility.HtmlDecode(title),
                       Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t","").Replace("\n",""),
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

       private bool CheckIfCategorizied(ref List<HtmlNode> firstProductNodes)
        {

            GetProducts(FirstResponseHtmlDocument.DocumentNode, ref firstProductNodes);
            var node = firstProductNodes.FirstOrDefault();
            if (node == null)
            {
                return false;
            }
            if (node.GetAttributeValue("class", "no attribute") != "product clearfix") return false;
            if (node.SelectSingleNode(".//div[@class='productinfobody withretailerlogo']") != null)
            {
                _categorizedDesign = true;
                return false;
            }
            return true;
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
            if (_categorizedDesign)
            {
                var titleNode = node.SelectSingleNode(".//div[@class='productinfobody withretailerlogo']").SelectSingleNode((".//div[@class='productname']"));
                if (titleNode==null)
                    throw new Exception("Invalid node data");
                var title = titleNode.InnerText;
                var productUrlNode = titleNode.SelectSingleNode(".//h3/a");
                if (productUrlNode == null)
                    throw new Exception("Invalid node data");
                var productUrl = productUrlNode.GetAttributeValue("href", "No url");
                if (productUrl == "No url")
                    throw new Exception("Invalid node data");
                if (productUrl.StartsWith("/"))
                {
                    productUrl = String.Format("http://www.pricerunner.se{0}", productUrl);
                }
                var productId = node.Id.Replace("prod-", "");
                var imageNode = node.SelectSingleNode(".//div[@class = 'productimg clearfix jshover_list']").SelectSingleNode(".//a/img");
                string imageUrl = imageNode == null ? "No image" : imageNode.GetAttributeValue("src", "No image");
                var priceNode = node.SelectSingleNode(".//div[@class='price']").SelectSingleNode(".//p[@class='price-rang']");
                return new ProductInfo
                {
                    ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                    Date = "No date",
                    ProductUrl = HttpUtility.HtmlDecode(productUrl),
                    Name = HttpUtility.HtmlDecode(title).Trim().Replace("\t", "").Replace("\n", ""),
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                    Id = productId,
                    Location = "No location",
                    Domain = "www.pricerunner.se"

                };
            }
            else
            {
                var titleNode = node.SelectSingleNode(".//div[@class='product-wrapper']")
                    .ChildNodes.First(el => el.GetAttributeValue("class", "no class") == "no class" && el.Name == "p").SelectSingleNode(".//a[@target='_blank']");
                var title = titleNode.GetAttributeValue("title", "No title");
                if (title == "No title")
                    throw new Exception("Invalid node data");
                var productUrl = titleNode.GetAttributeValue("href", "No url");
                if (productUrl == "No url")
                    throw new Exception("Invalid node data");
                if (productUrl.StartsWith("/"))
                {
                    productUrl = String.Format("http://www.pricerunner.se{0}", productUrl);
                }
                string imageUrl = titleNode.SelectSingleNode(".//img").GetAttributeValue("src", "No image");

                var priceNode = node.SelectSingleNode("//div[@class='product-wrapper']").SelectSingleNode(".//p[@class='price']");
                string productId =node.Id.Replace("prod-", "");
                return new ProductInfo()
                {
                    ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                    Date = "No date",
                    ProductUrl = HttpUtility.HtmlDecode(productUrl),
                    Name = HttpUtility.HtmlDecode(title),
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                    Id = productId,
                    Location = "No location",
                    Domain = Domain
                };    
            }
            
        }

       public override int Id
       {
           get { return 2; }
       }

       public override string Domain
       {
           get { return "www.pricerunner.se"; }
       }

       protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("{0}&page={1}", FirstResponseUrl, pageNum);
        }
    }
}
