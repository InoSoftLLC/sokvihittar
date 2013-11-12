using System.IO;
using System.Net;
using System.Text;

namespace Sokvihittar.Crawlers.Common
{
    public static class WebRequestHelper
    {
        public static string GetResponseHtml(string url)
        {
            HttpWebResponse response = GetResponse(url);
            return GetResponseHtml(response);
        }

        public static string GetResponseHtml(HttpWebResponse response)
        {
            var sb = new StringBuilder();
            var buf = new byte[8192];
            Stream resStream = response.GetResponseStream();
            int count;
            do
            {
                count = resStream.Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buf, 0, count));
                }
            } while (count > 0);
            return sb.ToString();
        }

        public static HttpWebResponse GetResponse(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            return (HttpWebResponse)request.GetResponse();
        }

        public static string GetResponceUrl(string requestUrl)
        {
            return GetResponse(requestUrl).ResponseUri.ToString();
        }
    }
}
