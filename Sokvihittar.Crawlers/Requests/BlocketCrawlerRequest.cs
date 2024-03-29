﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class BlocketCrawlerRequest : CrawlerRequest
    {
        public BlocketCrawlerRequest(string productText, int limit, bool isStrictResults)
            : base(productText, limit, isStrictResults)
        {
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.blocket.se"; }
        }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.GetEncoding(EncodingHelper.CodePages["iso-8859-1"]); }
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 5; }
        }
        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName
        {
            get { return "Blocket"; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("http://www.blocket.se/hela_sverige?q={0}&cg=0&w=3&st=s&ca=11&is=1&l=0&md=th",
                    HttpUtility.UrlEncode(ProductText.Replace(' ', '+'),Encoding));
            }
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("http://www.blocket.se/hela_sverige?q={0}&cg=0&w=3&st=s&ca=11&l=0&md=th&o={1}",
                    HttpUtility.UrlEncode(ProductText.Replace(' ', '+'),Encoding), pageNum);
        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            string imageUrl;
            try
            {
                var imageNode = node.ChildNodes.First(el => el.GetAttributeValue("class", "No class").Contains("image_container"))
                    .ChildNodes.First(el => el.Name == "div" && el.GetAttributeValue("class", "No class").Contains("image_content"))
                    .ChildNodes.Single(el => el.Name == "div")
                    .ChildNodes.Single(el => el.Name == "a")
                    .ChildNodes.Single(el => el.Name == "img");
                imageUrl = imageNode.GetAttributeValue("src", "No image");
                if (imageUrl == "/img/transparent.gif")
                {
                    imageUrl = imageNode.GetAttributeValue("longdesc", "No image");
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            if (imageUrl.StartsWith("/"))
            {
                imageUrl = String.Format("http://www.blocket.se{0}", imageUrl);
            }
            var productNode = node.SelectSingleNode(".//div[@class='desc']");
            if (productNode == null)
            {
                throw new Exception("Invalid Product Data");
            }
            var dateNode = productNode.SelectSingleNode(".//div[@class='list_date']");
            if (dateNode == null)
            {
                throw new Exception("Invalid Product Data");
            }
            var titleNode = productNode.SelectSingleNode(".//a[@class='item_link']");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
            {
                throw new Exception("Invalid Product Data");
            }
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.blocket.se{0}", productUrl);
            }
            var title = titleNode.InnerText;
            var priceNode = productNode.SelectSingleNode(".//p[@class='list_price']");
            if (priceNode == null)
            {
                throw new Exception("Invalid Product Data");
            }
            var price = HttpUtility.HtmlDecode(priceNode.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            if (price == String.Empty)
            {
                price = "No price";
            }
            string location;
            try
            {
                var locationNode =
                    productNode.SelectSingleNode(".//div[@class='cat_geo clean_links float_left']")
                        .SelectSingleNode(".//span[@class='list_area']");
                location = HttpUtility.HtmlDecode(locationNode.InnerText);
                location = location.Substring(location.IndexOf(",") + 1).Trim().Replace("\t", "").Replace("\n", "");
                if (location == String.Empty)
                {
                    location = "No location";
                }
            }
            catch (Exception)
            {
                location = "No location";
            }
            var productId = node.Id.Replace("item_", "");
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = HttpUtility.HtmlDecode(dateNode.InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = productId,
                Location = location,
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
            if (node.Id.Contains("item_") && node.GetAttributeValue("class", "No class").Contains("item_row"))
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }
    }
}
