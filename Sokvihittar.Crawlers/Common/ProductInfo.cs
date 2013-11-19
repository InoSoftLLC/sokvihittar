using System;
using System.Runtime.Serialization;

namespace Sokvihittar.Crawlers.Common
{
    [DataContract]
    public class ProductInfo
    {
        [IgnoreDataMember]
        private string _date;

        [IgnoreDataMember]
        private string _imageUrl;

        [IgnoreDataMember]        
        private string _location;

        [IgnoreDataMember]
        private string _name;

        [IgnoreDataMember]
        private string _price;

        [IgnoreDataMember]
        private string _productUrl;


        [DataMember(Name = "date")]
        public string Date { get { return _date; } set { _date = value == String.Empty ? "No date" : value; } }

        [DataMember(Name = "domain")]
        public string Domain { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "img")]
        public string ImageUrl { get { return _imageUrl; } set { _imageUrl = value == String.Empty ? "No image" : value; } }

        [DataMember(Name = "location")]
        public string Location { get { return _location; } set { _location = value == String.Empty ? "No location" : value; } }

        [DataMember(Name = "title")]
        public string Name  { get { return _name; } set { _name = value == String.Empty ? "No title" : value; } }

        [DataMember(Name = "price")]
        public string Price { get { return _price; } set { _price = value == String.Empty ? "No price" : value; } }

        [DataMember(Name = "href")]
        public string ProductUrl  { get { return _productUrl; } set { _productUrl = value == String.Empty ? "No url" : value; } }
    }
}
