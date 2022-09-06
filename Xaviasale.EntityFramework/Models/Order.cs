using System;
using System.Collections.Generic;

namespace Xaviasale.EntityFramework.Models
{
    public class Order
    {
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
        public DateTime CreateDate { get; set; }
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }
    }
}
