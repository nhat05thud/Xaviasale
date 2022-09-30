using System;
using System.Collections.Generic;

namespace Xaviasale.EntityFramework.Models
{
    public class Order
    {
        public Order()
        {
            IsReaded = false;
            IsDelete = false;
            IsSuccess = false;
            Status = false;
            ResponGuid = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }
        public int OrderId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public bool IsReaded { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsDelete { get; set; }
        public bool Status { get; set; }
        public string OrderStatus { get; set; }
        public string RequestApi { get; set; }
        public string ResponseApi { get; set; }
        public Guid ResponGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public decimal AmountOrder { get; set; }
        public decimal ShipFee { get; set; }
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }
    }
}
