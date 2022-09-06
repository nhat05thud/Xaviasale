using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Xaviasale.ClassHelper;

namespace Xaviasale.Models
{
    public class CheckOutModel : BaseModel
    {
        [UmbracoRequired("FormField.Email.Required")]
        [UmbracoEmail("FormField.Email.Validation")]
        public string Email { get; set; }
        [UmbracoRequired("FormField.FirstName.Required")]
        public string FirstName { get; set; }
        [UmbracoRequired("FormField.LastName.Required")]
        public string LastName { get; set; }
        [UmbracoRequired("FormField.Address.Required")]
        public string Address { get; set; }
        public string Apartment { get; set; }
        [UmbracoRequired("FormField.ZipCode.Required")]
        public string ZipCode { get; set; }
        [UmbracoRequired("FormField.City.Required")]
        public string City { get; set; }
        [UmbracoRequired("FormField.State.Required")]
        public string State { get; set; }
        [UmbracoRequired("FormField.Country.Required")]
        public string Country { get; set; }
        public string Phone { get; set; }
        public bool IsSameBillingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public List<Cart> Carts { get; set; }
    }

    public class DiffernceBillingAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
    }
}