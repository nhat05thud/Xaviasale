using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Xaviasale.ClassHelper;
using Xaviasale.EntityFramework.Context;
using Xaviasale.EntityFramework.Models;
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class checkOutSuccessPageController : RenderMvcController
    {
        // GET: checkOutSuccessPage
        public override ActionResult Index(ContentModel model)
        {
            if (Request.Params["data"] != null)
            {
                Session[AppConstant.SESSION_CART_ITEMS] = null;
                using (var db = new XaviasaleContext())
                {
                    var param = Request.Params["data"].ToLower();
                    var order = db.Orders.FirstOrDefault(x => x.ResponGuid.ToString().ToLower().Equals(param));
                    if (order == null)
                    {
                        return CurrentTemplate(model);
                    }
                    order.IsSuccess = true;
                    db.Orders.Attach(order);
                    db.Entry(order).Property(x => x.IsSuccess).IsModified = true;
                    db.SaveChanges();
                    var data = new CheckOutModel
                    {
                        Email = order.Email,
                        FirstName = order.FirstName,
                        LastName = order.LastName,
                        Address = order.Address,
                        Apartment = order.Apartment,
                        ZipCode = order.ZipCode,
                        City = order.City,
                        State = order.State,
                        Country = order.Country,
                        Phone = order.Phone
                    };
                    data.Carts = db.ShoppingCarts.Where(x => x.OrderId == order.OrderId)
                        .Select(x => new Cart
                        {
                            Color = x.Color,
                            CouponId = x.CouponId,
                            ProductId = x.ProductId,
                            Quantity = x.Quantity
                        }).ToList();
                    var email = RenderMailMessage(data);
                    try
                    {
                        var smtp = new SmtpClient();
                        smtp.Send(email);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            return CurrentTemplate(model);
        }
        private MailMessage RenderMailMessage(CheckOutModel model)
        {
            decimal total = 0;

            var sendTo = WebConfigurationManager.AppSettings["EmailContactReceive"];
            var messageString = "<h3>" + WebConfigurationManager.AppSettings["EmailCheckOutTitle"] + "</h3>";
            messageString += "<b>Email: </b>" + model.Email + "<br />";
            messageString += "<b>First Name: </b>" + model.FirstName + "<br />";
            messageString += "<b>Last Name: </b>" + model.LastName + "<br />";
            messageString += "<b>Address: </b>" + model.Address + "<br />";
            messageString += "<b>Apartment, suite, etc.: </b>" + model.Apartment + "<br />";
            messageString += "<b>Zip Code: </b>" + model.ZipCode + "<br />";
            messageString += "<b>City: </b>" + model.City + "<br />";
            messageString += "<b>State: </b>" + model.State + "<br />";
            messageString += "<b>Country: </b>" + model.Country + "<br />";
            messageString += "<b>Phone number: </b>" + model.Phone + "<br />";
            messageString += "<p>==================================================</p>";
            messageString += "<b>Orders: </b><br />";
            messageString += "<ul>";
            if (model.Carts != null)
            {
                var hasCoupon = false;
                foreach (var item in model.Carts)
                {
                    decimal discount = 0;
                    var couponName = string.Empty;
                    if (item.CouponId > 0 && hasCoupon == false)
                    {
                        var coupon = Umbraco.Content(item.CouponId);
                        discount = coupon.Value<decimal>("discount");
                        couponName = coupon.Name;
                    }
                    var product = Umbraco.Content(item.ProductId);
                    if (product != null)
                    {
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            messageString += "<li>";
                            messageString += "<b>Sản phẩm: </b> <a href='" + product.Url(mode: UrlMode.Absolute) + "'>" + product.Name + "</a><br />";
                            messageString += "<b>Color: </b>" + item.Color + "<br />";
                            messageString += "<b>Số lượng: </b> " + item.Quantity + "<br />";
                            if (!string.IsNullOrEmpty(couponName))
                            {
                                messageString += "<b>Coupon: </b> " + couponName + "<br />";
                            }
                            messageString += "<b>Giá (đơn vị 1 cái): </b> $" + (discount > 0 ? itemColorNested.Value<decimal>("price") - (itemColorNested.Value<decimal>("price") * (discount / 100)) : itemColorNested.Value<decimal>("price")) + "<br />";
                            messageString += "<b>Tổng: </b> $" + (discount > 0 ? (itemColorNested.Value<decimal>("price") - (itemColorNested.Value<decimal>("price") * (discount / 100))) * item.Quantity : itemColorNested.Value<decimal>("price") * item.Quantity) + "<br />";
                            messageString += "</li>";
                            total += (discount > 0 ? (itemColorNested.Value<decimal>("price") - (itemColorNested.Value<decimal>("price") * (discount / 100))) * item.Quantity : itemColorNested.Value<decimal>("price") * item.Quantity);
                        }
                    }
                }
            }
            messageString += "</ul>";
            messageString += "<p>==================================================</p>";
            messageString += "<b>Tổng tiền: </b>$" + total + "<br />";

            var email = new MailMessage
            {
                Subject = "Order",
                Body = messageString,
                IsBodyHtml = true,
                To = { sendTo }
            };
            return email;
        }
    }
}