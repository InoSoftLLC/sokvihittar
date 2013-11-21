using System;
using System.Collections.Generic;
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

        public static HttpWebResponse GetResponse(string url, Dictionary<HttpRequestHeader, string> headers=null, Cookie[] cookies =null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.UseNagleAlgorithm = false;
            request.Method = WebRequestMethods.Http.Get;;
            request.Proxy = WebRequest.DefaultWebProxy;
            request.KeepAlive = false;
            request.CookieContainer = new CookieContainer();
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    if (kvp.Key != HttpRequestHeader.Accept)
                    {
                        request.Headers.Set(kvp.Key,kvp.Value);
                    }
                    else
                    {
                        request.Accept = kvp.Value;
                    }
                }
            }
            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    request.CookieContainer.Add(cookie);
                }
            }
            request.ProtocolVersion = new Version(1,1);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            return (HttpWebResponse)request.GetResponse();

        }

        public static string GetResponceUrl(string requestUrl)
        {
            return GetResponse(requestUrl).ResponseUri.ToString();
        }

        public static HttpWebResponse GetPostResponse(string url, string postText)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.UseNagleAlgorithm = false;
            WebRequest.DefaultWebProxy = null;
            request.Proxy = WebRequest.DefaultWebProxy;
            request.KeepAlive=false;
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded";
            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(postText);
            }
            request.CookieContainer = new CookieContainer();
            request.KeepAlive = false;
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";
            request.ProtocolVersion = new Version(1, 1);
            request.AllowAutoRedirect = false;
            return (HttpWebResponse)request.GetResponse();
        }
    }
}
