using System;
using System.Collections.Generic;

namespace Xaviasale.Models.BackOffice
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            TotalPrice = 0;
        }
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
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
        public decimal TotalPrice { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }
    }
    public class OrderProduct
    {
        public int ProductId { get; set; }
        public string ProductUrl { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
    }
}