using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Sokvihittar.Controllers
{
    public static class JsonHelper
    {
        public static string Serialize<T>(T item)
        {
            var serializer = new DataContractJsonSerializer(item.GetType());
            string result;
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, item);
                var buf = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(buf, 0, buf.Length);
                result = Encoding.UTF8.GetString(buf);
            }
            return result;
        }

        public static T Deserialize<T>(string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            T result;
            using (var stream = File.OpenRead(path))
            {
                result = (T)serializer.ReadObject(stream);
               
            }
            return result;
        }
    }
}