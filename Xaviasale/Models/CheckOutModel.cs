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
        public string PaymentMethod { get; set; }
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

    public class CoinHomePayOrderReturnModel
    {
        public string success { get; set; }
        public string message { get; set; }
        public string code { get; set; }
        public string showType { get; set; }
        public DataObject data { get; set; }
        public string timestamp { get; set; }
    }
    public class DataObject
    {
        public string tradeNo { get; set; }
        public string merchantId { get; set; }
        public string outTradeNo { get; set; }
        public string outBody { get; set; }
        public string channelType { get; set; }
        public string channel { get; set; }
        public string money { get; set; }
        public string payMoney { get; set; }
        public string realMoney { get; set; }
        public string moneyScale { get; set; }
        public string status { get; set; }
        public string statusDesc { get; set; }
        public string payUrl { get; set; }
        public string qrcodeUrl { get; set; }
        public string qrcodeContent { get; set; }
        public string paySucc { get; set; }
        public string paySuccTime { get; set; }
        public string notifySucc { get; set; }
        public string notifySuccTime { get; set; }
        public string validTime { get; set; }
        public string validTimeMills { get; set; }
    }
}