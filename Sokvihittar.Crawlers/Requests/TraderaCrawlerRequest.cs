﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class TraderaCrawlerRequest : CrawlerRequest
    {
        public TraderaCrawlerRequest(string productText, int limit, bool isStrictResults) : base(productText, limit, isStrictResults)
        {
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 4; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.tradera.com"; }
        }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName
        {
            get { return "Tradera"; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get
            {
                return String.Format("http://www.tradera.com/finding.mvc/itemlisting?header=true&search={0}",
                    HttpUtility.UrlEncode(ProductText, Encoding));
            }
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            return String.Format("http://www.tradera.com/finding.mvc/itemlisting?search={0}&page={1}",
                    HttpUtility.UrlEncode(ProductText, Encoding),pageNum);
        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            var productNode = node.SelectSingleNode(".//div[@class='boxbody']");
            string imageUrl;
            try
            {
                var imageNode =
                    productNode.SelectSingleNode(".//div[@class='picture']")
                        .SelectSingleNode(".//div[@class='imageHolder']").ChildNodes.First().ChildNodes.First();
                imageUrl = imageNode.GetAttributeValue("src", "No image");
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            if (imageUrl.StartsWith("/"))
            {
                imageUrl = String.Format("http://www.tradera.com{0}", imageUrl);
            }
            var infoNode =
                productNode.SelectSingleNode(".//div[@class='picture']")
                        .SelectSingleNode(".//div[@class='infoHolder']");
            if (infoNode==null)
                throw new Exception("Invalid node data");
            var titleNode = infoNode.SelectSingleNode(".//div[@class='ObjectHeadline']").ChildNodes.Single(el=>el.Name=="a");
            if (titleNode == null)
                throw new Exception("Invalid node data");
            string title = titleNode.InnerText;
            string productUrl=titleNode.GetAttributeValue("href", "No url");
            if(productUrl == "No url")
                throw new Exception("Invalid node data");
            if (productUrl.StartsWith("/"))
            {
                productUrl = String.Format("http://www.tradera.com{0}", productUrl);
            }
            var  priceNode = productNode.SelectSingleNode(".//div[@class='price']");
            if(productUrl == null)
                throw new Exception("Invalid node data");
            string price;
            if (priceNode.FirstChild.Name == "#text")
            {
                var tempNode = priceNode.SelectSingleNode(".//div[@class='auctionWithBIN']");
                price = tempNode != null ? tempNode.ChildNodes.First(el => el.Name == "#text").InnerText : priceNode.InnerText;
            }
            else
            {
                price = priceNode.FirstChild.ChildNodes.First(el => el.Name == "#text").InnerText;
            }
            var productIdNode = infoNode.SelectSingleNode(".//div[@class='memoryListAdder']");
            if (productIdNode == null)
                throw new Exception("Invalid node data");
            var productId = productIdNode.Id.Replace("mlaLink", "");
            string date;
            try
            {
                var dateNode =
                    productNode.SelectSingleNode(".//div[@class='endDate']")
                        .ChildNodes.First(el => el.Name == "div")
                        .ChildNodes.First(el => el.Name == "span");
                date = HttpUtility.HtmlDecode(dateNode.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            }
            catch (Exception)
            {
                date = "No date";
            }
            
            return new ProductInfo()
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = price.Trim().Replace("\t", "").Replace("\n", ""),
                Id = productId,
                Location = "No location",
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
            if (node.GetAttributeValue("class", "no class") == "Box-F listStyle")
            {
                result.Add(node);
            }
            foreach (var childNode in node.ChildNodes)
            {
                GetProducts(childNode, ref  result);
            }
        }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
