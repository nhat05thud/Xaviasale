using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;

namespace Xaviasale.Models
{
    public class ProductDetailModel
    {
        public int Id { get; set; }
        public IEnumerable<IPublishedContent> Images { get; set; }
        public IEnumerable<IPublishedElement> ProductColors { get; set; }
        public string Name { get; set; }
        public string ColorName { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int Save { get; set; }
        public string Description { get; set; }
        public string Warranty { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaThumbnails { get; set; }
    }
}