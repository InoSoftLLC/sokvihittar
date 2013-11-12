using System.Runtime.Serialization;

namespace Sokvihittar.Crawlers.Common
{
    [DataContract]
    public class CrawlerResult
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "state")]
        public CrawlerRequestState State { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "products")]
        public ProductInfo[] Products { get; set; }
    }

    public enum CrawlerRequestState
    {
        None = 0,
        Success = 1,
        Failure = 2
    }
}
