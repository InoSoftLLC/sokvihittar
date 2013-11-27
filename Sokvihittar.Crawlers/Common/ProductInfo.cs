using System;
using System.Runtime.Serialization;

namespace Sokvihittar.Crawlers.Common
{
    /// <summary>
    /// Model containing product information.
    /// </summary>
    [DataContract]
    public class ProductInfo
    {
        /// <summary>
        /// Date the announcement was created on.
        /// </summary>
        [IgnoreDataMember]
        private string _date;

        /// <summary>
        /// Url to the image.
        /// </summary>
        [IgnoreDataMember]
        private string _imageUrl;
        
        /// <summary>
        /// Location of product.
        /// </summary>
        [IgnoreDataMember]        
        private string _location;

        /// <summary>
        /// Full title of product.
        /// </summary>
        [IgnoreDataMember]
        private string _title;

        /// <summary>
        /// Price.
        /// </summary>
        [IgnoreDataMember]
        private string _price;

        /// <summary>
        /// Url to product sale page.
        /// </summary>
        [IgnoreDataMember]
        private string _productUrl;

        /// <summary>
        /// Date the announcement was created on.
        /// </summary>
        [DataMember(Name = "date")]
        public string Date { get { return _date; } set { _date = value == String.Empty ? "No date" : value; } }

        /// <summary>
        /// Source website domain.
        /// </summary>
        [DataMember(Name = "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Product id on source site.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Url to the image.
        /// </summary>
        [DataMember(Name = "img")]
        public string ImageUrl { get { return _imageUrl; } set { _imageUrl = value == String.Empty ? "No image" : value; } }

        /// <summary>
        /// Location of product.
        /// </summary>
        [DataMember(Name = "location")]
        public string Location { get { return _location; } set { _location = value == String.Empty ? "No location" : value; } }

        /// <summary>
        /// Full title of product.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title  { get { return _title; } set { _title = value == String.Empty ? "No title" : value; } }

        /// <summary>
        /// Price.
        /// </summary>
        [DataMember(Name = "price")]
        public string Price { get { return _price; } set { _price = value == String.Empty ? "No price" : value; } }

        /// <summary>
        /// Url to product sale page.
        /// </summary>
        [DataMember(Name = "href")]
        public string ProductUrl  { get { return _productUrl; } set { _productUrl = value == String.Empty ? "No url" : value; } }
    }
}
