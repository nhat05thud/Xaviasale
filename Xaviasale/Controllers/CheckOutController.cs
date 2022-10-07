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
using Stripe;
using Stripe.Checkout;
using System.Web;

namespace Xaviasale.Controllers
{
    public class CheckOutController : SurfaceController
    {
        public CheckOutController()
        {
            StripeConfiguration.ApiKey = WebConfigurationManager.AppSettings["Stripe.SecretKey"];
        }
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
            var returnData = Checkout(model.Carts, successUrl, cancelUrl, model);
            if (returnData == null)
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, session = returnData.Id, message = "Success" }, JsonRequestBehavior.AllowGet);
        }
        private Stripe.Checkout.Session Checkout(List<Cart> model, string successUrl, string cancelUrl, CheckOutModel checkoutModel)
        {
            var orderModel = SaveOrdersToDatabase(checkoutModel);
            var lstProducts = new List<CheckOutIPublishContent>();

            #region shipFee
            decimal money = 0;
            decimal shipFee = 0;
            if (model.Count == 1 && model.FirstOrDefault().Quantity == 1)
            {
                var product = Umbraco.Content(model.FirstOrDefault().ProductId);
                if (product != null)
                {
                    var nested = product.Value<IEnumerable<IPublishedElement>>("productColorNested");
                    if (nested != null && nested.Any())
                    {
                        shipFee = nested.FirstOrDefault().Value<decimal>("ship");
                    }
                }
            }
            money += shipFee;
            #endregion

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
                        money += discount > 0 ? (productPrice - productPrice * (discount / 100)) * item.Quantity : productPrice * item.Quantity;

                        var obj = new CheckOutIPublishContent
                        {
                            ProductName = product.Name,
                            ProductImage = itemColorNested.Value<IEnumerable<IPublishedContent>>("images")?.FirstOrDefault()?.Url(mode: UrlMode.Absolute),
                            ProductPrice = shipFee > 0 && model.IndexOf(item) == 0 ? itemColorNested.Value<decimal>("price") + shipFee : itemColorNested.Value<decimal>("price"),
                            Product = itemColorNested,
                            Quantity = item.Quantity
                        };
                        lstProducts.Add(obj);
                    }
                }
            }

            #region Stripe
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lstProducts.Select(x => new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmountDecimal = x.ProductPrice * 100, // cents to Usd
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = x.ProductName,
                            Description = $"{x.ProductName} - ({x.Product.Value<string>("title")})",
                            Images = new List<string>
                            {
                                HttpUtility.UrlPathEncode(x.ProductImage != null ? x.ProductImage : "https://via.placeholder.com/202x202")
                            }
                        },
                    },
                    Quantity = x.Quantity
                }).ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderModel.OrderId.ToString() },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            Session session = service.Create(options);
            #endregion

            try
            {
                using (var db = new XaviasaleContext())
                {
                    var order = db.Orders.FirstOrDefault(x => x.ResponGuid.ToString().ToLower().Equals(orderModel.ResponGuid.ToString().ToLower()));
                    order.RequestApi = JsonConvert.SerializeObject(options);
                    order.AmountOrder = money;
                    order.ShipFee = shipFee;

                    db.Orders.Attach(order);
                    db.Entry(order).Property(x => x.RequestApi).IsModified = true;
                    db.Entry(order).Property(x => x.AmountOrder).IsModified = true;
                    db.Entry(order).Property(x => x.ShipFee).IsModified = true;
                    db.SaveChanges();
                }
                return session;
            }
            catch
            {
                return null;
            }
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
                            var obj = Umbraco.Content(product.ProductId);
                            if (obj != null)
                            {
                                var itemColorNested = obj.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(product.Color));
                                if (itemColorNested != null)
                                {
                                    var coupon = Umbraco.Content(product.CouponId);
                                    var productPrice = itemColorNested.Value<decimal>("price");
                                    var item = new ShoppingCart
                                    {
                                        OrderId = order.OrderId,
                                        ProductId = product.ProductId,
                                        Quantity = product.Quantity,
                                        Color = product.Color,
                                        CouponId = coupon != null ? product.CouponId : 0,
                                        ProductAmount = productPrice,
                                        ProductDiscount = coupon != null ? coupon.Value<int>("discount") : 0
                                    };
                                    db.ShoppingCarts.Add(item);
                                    db.SaveChanges();
                                }
                            }
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
        [HttpPost]
        public ActionResult UpdatePaymentStatus()
        {
            Session[AppConstant.SESSION_CART_ITEMS] = null;
            using (var db = new XaviasaleContext())
            {
                try
                {
                    Stream req = Request.InputStream;
                    req.Seek(0, System.IO.SeekOrigin.Begin);
                    string json = new StreamReader(req).ReadToEnd();

                    // Get all Stripe events.
                    var stripeEvent = EventUtility.ParseEvent(json);
                    string stripeJson = stripeEvent.Data.RawObject + string.Empty;
                    var childData = Charge.FromJson(stripeJson);
                    var metadata = childData.Metadata;

                    int orderID = -1;
                    string strOrderID = string.Empty;
                    if (metadata.TryGetValue("OrderId", out strOrderID))
                    {
                        int.TryParse(strOrderID, out orderID);
                    }
                    Stripe.Checkout.Session session = null;
                    switch (stripeEvent.Type)
                    {
                        case Events.CheckoutSessionCompleted:
                            session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                            // Save an order in your database, marked as 'awaiting payment'
                            //CreateOrder(session);

                            // Check if the order is paid (for example, from a card payment)
                            //
                            // A delayed notification payment will have an `unpaid` status, as
                            // you're still waiting for funds to be transferred from the customer's
                            // account.
                            var orderPaid = session.PaymentStatus == "paid";

                            if (orderPaid)
                            {
                                // Fulfill the purchase
                                FulfillOrder(session, json, orderID);
                            }
                            break;
                        case Events.CheckoutSessionAsyncPaymentSucceeded:
                            session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                            // Fulfill the purchase
                            FulfillOrder(session, json, orderID);

                            break;
                        case Events.CheckoutSessionAsyncPaymentFailed:
                            session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                            // Send an email to the customer asking them to retry their order
                            //EmailCustomerAboutFailedPayment(orderID);

                            break;
                    }
                    return Json(new { Code = 1, Message = "Update success." });
                }
                catch (StripeException e)
                {
                    return Json(new { Code = -100, Message = "Error." });
                }
            }
        }
        private void FulfillOrder(Stripe.Checkout.Session session,string json, int orderID)
        {
            using (var db = new XaviasaleContext())
            {
                var order = db.Orders.FirstOrDefault(x => x.OrderId == orderID);
                if (order != null)
                {
                    order.IsSuccess = true;
                    order.Status = true;
                    order.OrderStatus = session.PaymentStatus;
                    order.UpdateDate = DateTime.Now;
                    order.ResponseApi = json;
                    db.Orders.Attach(order);
                    db.Entry(order).Property(x => x.IsSuccess).IsModified = true;
                    db.Entry(order).Property(x => x.Status).IsModified = true;
                    db.Entry(order).Property(x => x.OrderStatus).IsModified = true;
                    db.Entry(order).Property(x => x.UpdateDate).IsModified = true;
                    db.Entry(order).Property(x => x.ResponseApi).IsModified = true;
                    db.SaveChanges();
                }
            }
            EmailAdminAboutPayment(orderID);
        }
        private void EmailAdminAboutPayment(int orderID)
        {
            // get orderid
            using (var db = new XaviasaleContext())
            {
                var order = db.Orders.FirstOrDefault(x => x.OrderId == orderID);
                if (order != null)
                {
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
                    }
                }
            }
        }
        private void EmailCustomerAboutFailedPayment(int orderID)
        {
            // get orderid
            using (var db = new XaviasaleContext())
            {
                var order = db.Orders.FirstOrDefault();
                if (order != null)
                {
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
                    }
                }
            }
        }
    }
}