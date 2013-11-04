using System.Runtime.Serialization;

namespace AllaannonserCrawler
{
     
    [DataContract]
    public class ProductInfo
    {
        [DataMember(Name = "date")]
        public string Date { get; set; }

        [DataMember(Name = "domain")]
        public string Domain { get { return "www.allaannonser.se"; } set { } }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "img")]
        public string ImageUrl { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "title")]
        public string Name { get; set; }

        [DataMember(Name = "price")]
        public string Price { get; set; }

        [DataMember(Name = "href")]
        public string ProductUrl { get; set; }
    }
}
