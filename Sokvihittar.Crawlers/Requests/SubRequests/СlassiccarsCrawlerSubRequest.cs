using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests.SubRequests
{
    class СlassiccarsCrawlerSubRequest : CrawlerSubRequest
    {
        /// <summary>
        /// Catego
        /// </summary>
        private readonly string _category;

        public СlassiccarsCrawlerSubRequest(string productText, int limit, string category, bool isStrictResults, string fullSerchText) : base(productText, limit,isStrictResults, fullSerchText)
        {
            _category = category;
        }

        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        public override int Id
        {
            get { return 11; }
        }

        /// <summary>
        /// Source website domain.
        /// </summary>
        public override string Domain
        {
            get { return "www.classiccars.se"; }
        }

        /// <summary>
        /// Name of source website.
        /// </summary>
        public override string SourceName
        {
            get {  return "Сlassiccars"; }
        }

        /// <summary>
        /// Url to get first result page.
        /// </summary>
        protected override string FirstRequestUrl
        {
            get { return GetNonFirstRequestUrl(1); }
        }

        /// <summary>
        /// Returns url to get selected rusult page.
        /// </summary>
        /// <param name="pageNum">Number of needed page.</param>
        /// <returns>String containing url.</returns>
        protected override string GetNonFirstRequestUrl(int pageNum)
        {
            switch (_category)
            {
                case "Bilar":
                case "MC":
                    return String.Format(
                        "http://www.classiccars.se/classic/b_resultat.asp?PageNo={0}&txtPrismax=9999999&txtPrismin=-1&txtLand=&txtLan=&txtInlagd=2013-07-27" +
                        "&txtKategori={1}&txtArfran=9&txtArtill=3000&txtSorteras=datum%20DESC,%20klockan%20DESC&txtVar=&txtTyp=S%E4ljes&txtFritext={2}" +
                        "&txtMarke=", pageNum, HttpUtility.UrlEncode(_category, Encoding),
                        HttpUtility.UrlEncode(ProductText, Encoding));
                case "Delar":
                    return String.Format(
                        "http://www.classiccars.se/classic/p_resultat.asp?PageNo={0}&txtInlagd=2013-07-27&txtSorteras=&txtVar=1&txtBild=&txtTyp=&txtFritext={1}&txtTyp2=&txtPageSize=",
                        pageNum, HttpUtility.UrlEncode(ProductText, Encoding));
                case "Båtar":
                    if (pageNum == 1)
                    {
                        return String.Format(
                            "http://www.classiccars.se/classic/b_resultat.asp?txtKategori=B%E5t&txtVar=1&txtFritext={0}&txtLan=&txtPrismin=&txtArfran=10&txtSorteras=datum+DESC%2C+klockan+DESC&txtMarke=&txtPrismax=&txtArtill=84&txtTyp=S%E4ljes&txtInlagd=2013-07-27&txtLand=",
                            HttpUtility.UrlEncode(ProductText, Encoding));
                    }
                    return null;
                case "Mopender":
                    if (pageNum == 1)
                    {
                        return
                            string.Format(
                                "http://www.classiccars.se/classic/b_resultat.asp?txtKategori=Moped&txtVar=1&txtFritext={0}&txtLan=&txtPrismin=&txtArfran=10&txtSorteras=datum+DESC%2C+klockan+DESC&txtMarke=&txtPrismax=&txtArtill=84&txtTyp=S%E4ljes&txtInlagd=2013-07-27&txtLand=",
                                HttpUtility.UrlEncode(ProductText, Encoding));
                    }
                    return null;
                case "Husvagnar":
                    return string.Format( 
                        "http://www.classiccars.se/classic/b_resultat.asp?PageNo={0}&txtPrismax=9999999&txtPrismin=-1&txtLand=&txtLan=&txtInlagd=2013-07-27&txtKategori=Husvagn&txtArfran=9&txtArtill=3000&txtSorteras=datum%20DESC,%20klockan%20DESC&txtVar=&txtTyp=S%E4ljes&txtFritext={1}&txtMarke=",
                        pageNum, HttpUtility.UrlEncode(ProductText, Encoding));
                default:
                    return null;
            }

        }

        /// <summary>
        /// Gets product info from html node.
        /// </summary>
        /// <param name="node">html node conatinig information about product.</param>
        /// <returns>Model containing information about product.</returns>
        protected override ProductInfo GetProductInfoFromNode(HtmlNode node)
        {
            if (_category == "Delar")
            {
                return GetProductInfoFromMachinePartNode(node);
            }
            var productNodes = node.SelectNodes(".//td");
            string imageUrl;
            try
            {
                imageUrl = productNodes[0].SelectSingleNode(".//a/img").GetAttributeValue("src", "No image");
                if (imageUrl != "No image")
                {
                    imageUrl = String.Format("http://www.classiccars.se{0}", imageUrl.Replace("..", ""));
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var titleNode = productNodes[2].SelectSingleNode(".//a");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid Product Data");
            if (productUrl.StartsWith(""))
                productUrl = String.Format("http://www.classiccars.se/classic/{0}", productUrl);
            var title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            string price = String.Format("{0} {1}",
                HttpUtility.HtmlDecode(productNodes[3].InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                HttpUtility.HtmlDecode(productNodes[4].InnerText).Trim().Replace("\t", "").Replace("\n", ""));
            string date = productNodes[2].ChildNodes.Last(el => el.Name == "#text").InnerText.Replace("Inlagd: ","");
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = "No id",
                Location = "No location",
                Domain = Domain
            };
        }
        protected ProductInfo GetProductInfoFromMachinePartNode(HtmlNode node)
        {
                    var productNodes = node.SelectNodes(".//td");
            string imageUrl;
            try
            {
                imageUrl = productNodes[0].SelectSingleNode(".//a/img").GetAttributeValue("src", "No image");
                if (imageUrl != "No image")
                {
                    imageUrl = String.Format("http://www.classiccars.se{0}", imageUrl.Replace("..", ""));
                }
            }
            catch (Exception)
            {
                imageUrl = "No image";
            }
            var titleNode = productNodes[1].SelectSingleNode(".//a");
            var productUrl = titleNode.GetAttributeValue("href", "No url");
            if (productUrl == "No url")
                throw new Exception("Invalid Product Data");
            productUrl = String.Format("http://www.classiccars.se/classic/{0}", productUrl);
            var title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim().Replace("\t", "").Replace("\n", "");
            string price = String.Format("{0} {1}",
                HttpUtility.HtmlDecode(productNodes[2].InnerText).Trim().Replace("\t", "").Replace("\n", ""),
                HttpUtility.HtmlDecode(productNodes[3].InnerText).Trim().Replace("\t", "").Replace("\n", ""));
            string date;
            try
            {
                date = productNodes[1].SelectSingleNode(".//small").InnerText.Replace("Inlagd: ", "");
            }
            catch (Exception)
            {
                date = "No date";
            }
            return new ProductInfo
            {
                ImageUrl = HttpUtility.HtmlDecode(imageUrl),
                Date = date,
                ProductUrl = HttpUtility.HtmlDecode(productUrl),
                Title = HttpUtility.HtmlDecode(title),
                Price = price,
                Id = "No id",
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
            try
            {
                if (node.Name == "tr" && node.ChildNodes.First(el => el.Name == "td").GetAttributeValue("height", "no heght") == "50")
                {
                    result.Add(node);
                }
            }
            catch (Exception)
            {}
            
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
            get { return Encoding.Default; }
        }

    }
}
