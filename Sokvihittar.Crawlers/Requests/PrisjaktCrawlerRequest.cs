using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Sokvihittar.Crawlers.Common;

namespace Sokvihittar.Crawlers.Requests
{
    class PrisjaktCrawlerRequest : ICrawlerRequest
    {
        public PrisjaktCrawlerRequest(string productText, int limit)
        {
            Limit = limit;
            ProductText = productText;
        }

        public Encoding Encoding { get { return Encoding.UTF8; } }

        public ProductInfo[] ProceedSearchRequest()
        {
            string response = WebRequestHelper.GetResponseHtml(
                String.Format(
                    "http://www.prisjakt.nu/ajax/server.php?class=Search_Supersearch&method=search&skip_login=1&modes=product,book,raw&limit=20&q={0}",
                     HttpUtility.UrlEncode(ProductText)),Encoding);

            var serializer = new DataContractJsonSerializer(typeof (PrisjaktResponse));
            object prisjaktResponse;
            try
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new StreamWriter(stream);
                    writer.Write(response);
                    writer.Flush();
                    stream.Position = 0;
                    prisjaktResponse = serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while deserializing the following json string: \"{0}\"",response),ex);
            }
            return GetProductInfos((PrisjaktResponse)prisjaktResponse).Take(Limit).ToArray();
        }

        private IEnumerable<ProductInfo> GetProductInfos(PrisjaktResponse prisjaktResponse)
        {
            Exception lastException = null;
            try
            {
                if (prisjaktResponse.message.product.items.Count == 0)
                {
                    return new ProductInfo[0];
                }

                var result = new List<ProductInfo>();
                
                foreach (var productInfo in prisjaktResponse.message.product.items)
                {
                    try
                    {
                        result.Add(new ProductInfo
                        {
                            Date = "No date",
                            Domain = Domain,
                            Id = productInfo.id.ToString(CultureInfo.InvariantCulture),
                            ImageUrl = "no image",
                            Location = "No location",
                            Name = productInfo.name,
                            Price = productInfo.price.display,
                            ProductUrl = productInfo.url
                        });
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                 }
                if (result.Count==0)
                    throw new Exception();
                return result.ToArray();
            }
            catch (Exception ex)
            {
                throw lastException != null
                    ? new Exception("Error while getting product infos.", lastException)
                    : new Exception("Error while getting product infos.", ex);
            }

        }


        public string SourceName { get { return "Prisjakt"; }}

        public int Limit { get; private set; }

        public string ProductText { get; private set; }

        public string Domain { get { return "www.fyndiq.se"; }}
    }
}
