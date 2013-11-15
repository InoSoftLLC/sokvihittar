using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sokvihittar.Crawlers
{
    [DataContract]
    public class PrisjaktResponse
    {
        [DataMember(Name = "error")]
        public string error { get; set; }

        [DataMember(Name = "message")]
        public PrisjaktMessage message { get; set; }
    }

    [DataContract]
    public class PrisjaktMessage
    {
        [DataMember(Name = "product")]
        public PrisjaktProduct product { get; set; }

        [DataMember(Name = "book")]
        public PrisjaktProduct book { get; set; }

        [DataMember(Name = "row")]
        public PrisjaktProduct row { get; set; }

    }

    public class PrisjaktProduct
    {
        [DataMember(Name = "items")]
        public List<PrisjaktProductInfo> items { get; set; }

        [DataMember(Name = "more_hits_available")]
        public bool? more_hits_available { get; set; }
    }

    [DataContract]
    public class PrisjaktProductInfo
    {
        [DataMember(Name = "id")]
        public long id { get; set; }

        [DataMember(Name = "popularity")]
        public int popularity { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "price")]
        public PrisjaktPrice price { get; set; }
    }

    [DataContract]
    public class PrisjaktPrice
    {
        [DataMember(Name = "display")]
        public string display { get; set; }
    }
}
