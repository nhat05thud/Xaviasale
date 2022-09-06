using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Xaviasale.Models
{
    public class CartViewModel
    {
        public decimal TotalPrice { get; set; }
        public List<CartModel> CartModels { get; set; }
        public string CartUrl { get; set; }
        public string CheckOutUrl { get; set; }
        public string HomeUrl { get; set; }
    }
    public class CartModel : Cart
    {
        public string ThumbnailPath { get; set; }
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
        public decimal ProductOldPrice { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
    public class CartSession
    {
        public List<Cart> Carts { get; set; }
    }
    public class Cart
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
    }

    public class EmptyCart
    {
        public string DictionaryText { get; set; }
        public string DictionaryButton { get; set; }
        public string RootUrl { get; set; }
    }
}