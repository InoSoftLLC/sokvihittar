using System.IO;
using System.Net;

namespace Sokvihittar.Common
{
    public static class WebHelper
    {
        public static string GetResponseHtml(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "GeT";
            var resp = (HttpWebResponse)request.GetResponse();
            return new StreamReader(resp.GetResponseStream()).ReadToEnd();
        }
    }
}
