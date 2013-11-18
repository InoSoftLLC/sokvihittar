using System;
using System.IO;
using System.Net;
using System.Text;

namespace Sokvihittar.Crawlers.Common
{
    public static class WebRequestHelper
    {
        public static string GetResponseHtml(string url, Encoding encoding)
        {
            HttpWebResponse response = GetResponse(url);
            return GetResponseHtml(response, encoding);
        }

        public static string GetResponseHtml(HttpWebResponse response, Encoding encoding)
        {
            string result;
            Stream resStream = response.GetResponseStream();
            using (var buffer = new BufferedStream(resStream))
            {
                using (var reader = new StreamReader(buffer, encoding))
                { 
                     result = reader.ReadToEnd();
                }
            }
            response.Dispose();
            return result;
        }

        public static HttpWebResponse GetResponse(string url)
        {
            ServicePointManager.UseNagleAlgorithm = false;
            WebRequest.DefaultWebProxy = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";
            request.ProtocolVersion = new Version(1,1);
            return (HttpWebResponse)request.GetResponse();

        }

        public static string GetResponceUrl(string requestUrl)
        {
            return GetResponse(requestUrl).ResponseUri.ToString();
        }
    }
}
