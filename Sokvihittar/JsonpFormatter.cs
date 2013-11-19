using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Converters;

namespace Sokvihittar
{
    public class JsonpFormatter : JsonMediaTypeFormatter
    {

        public JsonpFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/javascript"));

            JsonpParameterName = "callback";
        }

        /// <summary>
        ///  Name of the query string parameter to look for
        ///  the jsonp function name
        /// </summary>
        public string JsonpParameterName { get; set; }

        /// <summary>
        /// Captured name of the Jsonp function that the JSON call
        /// is wrapped in. Set in GetPerRequestFormatter Instance
        /// </summary>
        private string JsonpCallbackFunction;


        public override bool CanWriteType(Type type)
        {
            return true;
        }

        /// <summary>
        /// Override this method to capture the Request object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="request"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            var formatter = new JsonpFormatter()
            {
                JsonpCallbackFunction = GetJsonCallbackFunction(request)
            };

            // this doesn't work unfortunately
            //formatter.SerializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;

            // You have to reapply any JSON.NET default serializer Customizations here    
            formatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            formatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            return formatter;
        }


        public override Task WriteToStreamAsync(Type type, object value,
            Stream stream,
            HttpContent content,
            TransportContext transportContext)
        {
           
            if (string.IsNullOrEmpty(JsonpCallbackFunction))
            {
                
                return base.WriteToStreamAsync(type, value, stream, content, transportContext);
            }
            StreamWriter writer = null;
            content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript");
            // write the pre-amble
            try
            {

                writer = new StreamWriter(stream);
                writer.Write(JsonpCallbackFunction + "(");
                writer.Flush();
            }
            catch (Exception ex)
            {
                try
                {
                    if (writer != null)
                        writer.Dispose();
                }
                catch { }

                var tcs = new TaskCompletionSource<object>();
                tcs.SetException(ex);
                return tcs.Task;
            }
            
            return base.WriteToStreamAsync(type, value, stream, content, transportContext)
                .ContinueWith(innerTask =>
                {
                    if (innerTask.Status == TaskStatus.RanToCompletion)
                    {
                        writer.Write(");");
                        writer.Flush();
                    }
                    writer.Dispose();
                    return innerTask;
                }, TaskContinuationOptions.ExecuteSynchronously)
                .Unwrap();
        }

        /// <summary>
        /// Retrieves the Jsonp Callback function
        /// from the query string
        /// </summary>
        /// <returns></returns>
        private string GetJsonCallbackFunction(HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Get)
                return null;
            
            try
            {
                var url = request.RequestUri.Query;
                var lastParam = url.Split('&').Single(el => el.ToLower().Contains("callback="));
                    lastParam = lastParam.Replace("callback=", "").Replace("?", "");
                    return String.Format("{0} && {0}",lastParam);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}