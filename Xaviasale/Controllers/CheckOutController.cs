using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Xaviasale.ClassHelper;
using Xaviasale.EntityFramework.Context;
using Xaviasale.Models;
using Xaviasale.EntityFramework.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Xaviasale.Controllers
{
    public class CheckOutController : SurfaceController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult HandleCheckOut(CheckOutModel model)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(model.CultureLcid);
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Error",
                    view = ConvertViewToString("~/Views/Partials/CheckOut/_Form.cshtml", model)
                }, JsonRequestBehavior.AllowGet);
            }
            var home = Umbraco.Content(model.Carts.FirstOrDefault().ProductId).Root();
            var successUrl = home.DescendantOfType("checkOutSuccessPage")?.Url(mode: UrlMode.Absolute);
            var cancelUrl = home.DescendantOfType("checkOutFailPage")?.Url(mode: UrlMode.Absolute);
            //var notifyUrl = home.DescendantOfType("checkOutNotifyPage")?.Url(mode: UrlMode.Absolute);
            var notifyUrl = home.Url(mode: UrlMode.Absolute).Substring(0, home.Url(mode: UrlMode.Absolute).IndexOf('/', "https://".Length)) + "/umbraco/surface/checkout/PaymentNotify";
            var returnData = Checkout(model.Carts, successUrl, cancelUrl, notifyUrl, model);
            if (returnData == null)
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            Session["CheckOutOrder"] = returnData;
            return Json(new { success = true, message = "Success" }, JsonRequestBehavior.AllowGet);
        }
        private VnxRequest Checkout(List<Cart> model, string successUrl, string cancelUrl, string notifyUrl, CheckOutModel checkoutModel)
        {
            var orderModel = SaveOrdersToDatabase(checkoutModel);
            var merchanId = WebConfigurationManager.AppSettings["VNX.MerchantId"];
            var merchanUrl = WebConfigurationManager.AppSettings["VNX.merchantUrl"];
            successUrl = successUrl + "?data=" + orderModel.ResponGuid;
            cancelUrl = cancelUrl + "?data=" + orderModel.ResponGuid;
            notifyUrl = notifyUrl + "?data=" + orderModel.ResponGuid;
            var secret = WebConfigurationManager.AppSettings["VNX.Secret"];
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            decimal money = 0;
            var hasCoupon = false;
            foreach (var item in model)
            {
                decimal discount = 0;
                if (item.CouponId > 0 && hasCoupon == false)
                {
                    hasCoupon = true;
                    var coupon = Umbraco.Content(item.CouponId);
                    discount = coupon.Value<decimal>("discount");
                }
                var product = Umbraco.Content(item.ProductId);
                if (product != null)
                {
                    var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                    if (itemColorNested != null)
                    {
                        var productPrice = itemColorNested.Value<decimal>("price");
                        money += discount > 0 ? (productPrice - productPrice * (discount / 100)) * item.Quantity : productPrice * item.Quantity; ;
                    }
                }
            }
            var data = createOrderOnServer(merchanId, merchanUrl, money, successUrl, cancelUrl, notifyUrl, orderModel.OrderId.ToString(), timestamp, secret);
            var req = new VnxRequest {
                operation = "PAY",
                order = data
            };

            try
            {
                using (var db = new XaviasaleContext())
                {
                    var order = db.Orders.FirstOrDefault(x => x.ResponGuid.ToString().ToLower().Equals(orderModel.ResponGuid.ToString().ToLower()));
                    order.RequestApi = JsonConvert.SerializeObject(req);

                    db.Orders.Attach(order);
                    db.Entry(order).Property(x => x.RequestApi).IsModified = true;
                    db.SaveChanges();
                }
                return req;
            }
            catch
            {
                return null;
            }
        }
        private VnxOrder createOrderOnServer(string merchantId, string merchantUrl, decimal amountOrder, string successUrl, string cancelUrl, string notifyUrl, string orderId, string timestamp, string secret)
        {
            var stringToHash =
                "amountOrder=" + amountOrder + "&cancelUrl=" + cancelUrl + "&merchantId=" + merchantId + "&merchantUrl=" + merchantUrl + "&notifyUrl=" + notifyUrl + "&orderId=" + orderId + "&successUrl=" + successUrl + "&timestamp=" + timestamp;
            var secureHash = Utils.ToMd5(stringToHash + Utils.ToMd5(secret)).ToString();
            var order = new VnxOrder {
                merchantId = merchantId,
                merchantUrl = merchantUrl,
                amountOrder = amountOrder,
                successUrl = successUrl,
                cancelUrl = cancelUrl,
                notifyUrl = notifyUrl,
                OrderId = orderId,
                timestamp = timestamp,
                secureHash = secureHash.ToLower()
            };
            return order;
        }
        private OrderResponseModel SaveOrdersToDatabase(CheckOutModel model)
        {
            using (var db = new XaviasaleContext())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var order = new Order
                    {
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Address = model.Address,
                        Apartment = model.Apartment,
                        ZipCode = model.ZipCode,
                        City = model.City,
                        State = model.State,
                        Country = model.Country,
                        Phone = model.Phone
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    if (model.Carts != null)
                    {
                        foreach (var product in model.Carts)
                        {
                            var item = new ShoppingCart
                            {
                                OrderId = order.OrderId,
                                ProductId = product.ProductId,
                                Quantity = product.Quantity,
                                Color = product.Color,
                                CouponId = product.CouponId
                            };
                            db.ShoppingCarts.Add(item);
                            db.SaveChanges();
                        }
                    }

                    transaction.Commit();
                    return new OrderResponseModel { OrderId = order.OrderId, ResponGuid = order.ResponGuid };
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new OrderResponseModel();
                }
            }
        }
        private string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }
        [HttpGet]
        public void PaymentNotify(string data, bool status)
        {
            if (!string.IsNullOrEmpty(data))
            {
                using (var db = new XaviasaleContext())
                {
                    var param = data.ToLower();
                    var order = db.Orders.FirstOrDefault(x => x.ResponGuid.ToString().ToLower().Equals(param));
                    if (order != null)
                    {
                        if (status)
                        {
                            order.Status = true;
                            order.IsSuccess = true;
                        }
                        order.UpdateDate = DateTime.Now;
                        db.Orders.Attach(order);
                        db.Entry(order).Property(x => x.IsSuccess).IsModified = true;
                        db.Entry(order).Property(x => x.Status).IsModified = true;
                        db.Entry(order).Property(x => x.UpdateDate).IsModified = true;
                        db.SaveChanges();
                        var obj = new CheckOutModel
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
                        obj.Carts = db.ShoppingCarts.Where(x => x.OrderId == order.OrderId)
                            .Select(x => new Cart
                            {
                                Color = x.Color,
                                CouponId = x.CouponId,
                                ProductId = x.ProductId,
                                Quantity = x.Quantity
                            }).ToList();
                        var email = RenderMailMessage(obj);
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
            }
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