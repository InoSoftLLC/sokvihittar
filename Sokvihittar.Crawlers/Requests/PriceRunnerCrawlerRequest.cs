using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    public class PriceRunnerCrawlerRequest : CrawlerRequest
    {
        /// <summary>
        /// Flag shows if search results are cateforized.
        /// </summary>
        private bool _isCategorizied;

        /// <summary>
        /// Flag shows if cateforized design is used.
        /// </summary>
        private bool _categorizedDesign;

        public PriceRunnerCrawlerRequest(string productText, int limit) : base(productText, limit)
        {
        }

        /// <summary>
        /// Gets url to category page.
        /// </summary>
        /// <param name="node">Node containing information about product category.</param>
        /// <returns>String containing url to category.</returns>
        public string GetCategoryLink(HtmlNode node)
        {
            return string.Format("http://www.pricerunner.se{0}",
                node.SelectSingleNode(".//div[@class='productinfobody']")
                    .SelectSingleNode(".//div[@class='productname']")
                    .SelectSingleNode(".//h3/a").GetAttributeValue("href", "noLink"));
        }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        /// <summary>
        /// Executes search request, forms and returns product information models.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        public override ProductInfo[] ExecuteSearchRequest()
        {
            var firstResultNodes = new List<HtmlNode>();
            var result = new List<ProductInfo>();
            _isCategorizied = CheckIfCategorizied(ref firstResultNodes);
            if (!_isCategorizied)
            {
                return base.ExecuteSearchRequest();
            }
            var doc = new HtmlDocument();
            GetProducts(doc.DocumentNode, ref firstResultNodes);
            var products = ProccedResultPage(firstResultNodes);
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

        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName
        {
            get { return "Pricerunner"; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format(
                    "http://www.pricerunner.se/search?q={0}",
                    HttpUtility.UrlEncode(ProductText, Encoding));
            }
        }

        
        /// <summary>
        /// Get product information for categoty nodes.
        /// </summary>
        /// <param name="categotyNodes">List of node containig information for category.</param>
        ///  <returns>Returns collection of models containing information about product.</returns>
        private ProductInfo[] ProccedResultPage(List<HtmlNode> categotyNodes)
        {
            var result = new List<ProductInfo>();
            foreach (var productNode in categotyNodes)
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
                if (result.Count > Limit)
                    break;

            }
            return result.ToArray();
        }

        /// <summary>
        /// Gets products' information from category page.
        /// </summary>
        /// <param name="categoryLink">String containing url to catygoy page.</param>
        /// <returns>Array of models containing information about product.</returns>
        private ProductInfo[] GetProductInfos(string categoryLink)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(WebRequestHelper.GetResponseHtml(categoryLink, Encoding));
            var imageNode =
                htmlDoc.DocumentNode.SelectSingleNode(".//head").SelectSingleNode(".//meta[@property='og:image']");
            var imageUrl = imageNode == null ? "No image" : imageNode.GetAttributeValue("content", "No image");
            var productNodes =
                htmlDoc.GetElementbyId("price-list")
                    .SelectSingleNode(".//table[@class='list price-list']")
                    .SelectNodes(".//tbody/tr");
            var result = new List<ProductInfo>();
            foreach (var productNode  in productNodes)
            {
                try
                {
                    if (productNode.GetAttributeValue("class", "Mo class") != "Mo class")
                        continue;
                    var retailerNode =
                        productNode.SelectSingleNode(".//td[@class='about-retailer']").SelectSingleNode(".//h4/a") ??
                        productNode.SelectSingleNode(".//td[@class='about-retailer']").SelectSingleNode(".//h4");
                    var title = retailerNode.InnerText;
                    var productUrl = retailerNode.GetAttributeValue("href", "No link");
                    if (productUrl == "No link")
                        continue;
                    if (productUrl.StartsWith("/"))
                    {
                        productUrl = String.Format("http://www.pricerunner.se{0}", productUrl);
                    }
                    var priceNode =
                        productNode.SelectSingleNode(".//td[@class='price']").SelectNodes(".//div").FirstOrDefault();
                    if (priceNode == null)
                        continue;
                    var productId = productUrl.Split(';')[2].Replace("oi=", "").Replace("&amp", "");
                    string date;
                    try
                    {
                        date =
                            productNode.SelectSingleNode(".//td[@class='about-retailer']")
                                .SelectSingleNode(".//div[@class='in-stock-date-more']")
                                .SelectSingleNode(".//span[@class='date']")
                                .InnerText;
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
                        Title = HttpUtility.HtmlDecode(title),
                        Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                        Id = productId,
                        Location = "No location",
                        Domain = "www.pricerunner.se"
                    });
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Checks if results are categorized and categorized design is used.
        /// </summary>
        /// <param name="firstResultNodes">List of html nodes containing first results.</param>
        /// <returns>bool flag that shois if results are categorized.</returns>
        private bool CheckIfCategorizied(ref List<HtmlNode> firstResultNodes)
        {

            GetProducts(FirstResponseHtmlDocument.DocumentNode, ref firstResultNodes);
            var node = firstResultNodes.FirstOrDefault();
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


        /// <summary>
        /// Get html nodes conatinig information about products.
        /// </summary>
        /// <param name="node">Html node of search result page.</param>
        /// <param name="result">List of html nodes conatinig information about products.</param>
        protected override void GetProducts(HtmlNode node, ref List<HtmlNode> result)
        {
            var className = node.GetAttributeValue("class", "no attribute");
            if (className.Contains("product clearfix"))
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref result);
            }
        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            if (_categorizedDesign)
            {
                var titleNode =
                    node.SelectSingleNode(".//div[@class='productinfobody withretailerlogo']")
                        .SelectSingleNode((".//div[@class='productname']"));
                if (titleNode == null)
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
                var imageNode =
                    node.SelectSingleNode(".//div[@class = 'productimg clearfix jshover_list']")
                        .SelectSingleNode(".//a/img");
                string imageUrl = imageNode == null ? "No image" : imageNode.GetAttributeValue("src", "No image");
                var priceNode =
                    node.SelectSingleNode(".//div[@class='price']").SelectSingleNode(".//p[@class='price-rang']");
                return new ProductInfo
                {
                    ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                    Date = "No date",
                    ProductUrl = HttpUtility.HtmlDecode(productUrl),
                    Title = HttpUtility.HtmlDecode(title).Trim().Replace("\t", "").Replace("\n", ""),
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                    Id = productId,
                    Location = "No location",
                    Domain = "www.pricerunner.se"

                };
            }
            else
            {
                var titleNode = node.SelectSingleNode(".//div[@class='product-wrapper']")
                    .ChildNodes.First(el => el.GetAttributeValue("class", "no class") == "no class" && el.Name == "p")
                    .SelectSingleNode(".//a[@target='_blank']");
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

                var priceNode =
                    node.SelectSingleNode("//div[@class='product-wrapper']").SelectSingleNode(".//p[@class='price']");
                string productId = node.Id.Replace("prod-", "");
                return new ProductInfo()
                {
                    ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                    Date = "No date",
                    ProductUrl = HttpUtility.HtmlDecode(productUrl),
                    Title = HttpUtility.HtmlDecode(title),
                    Price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                    Id = productId,
                    Location = "No location",
                    Domain = Domain
                };
            }

        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 2; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.pricerunner.se"; }
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("{0}&page={1}", FirstResponseUrl, pageNum);
        }
    }
}
