using System;
using System.Runtime.Serialization;

namespace Sokvihittar.Crawlers.Common
{
    [DataContract]
    public class CrawlerResult
    {
        [IgnoreDataMember]
        public long ExecutionTime { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "state")]
        public CrawlerRequestState State { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "products")]
        public ProductInfo[] Products { get; set; }

        [IgnoreDataMember]
        public Exception Exception { get; set; }
    }
}
