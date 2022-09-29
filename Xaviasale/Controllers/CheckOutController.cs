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
            var notifyUrl = home.Url(mode: UrlMode.Absolute).Substring(0, home.Url(mode: UrlMode.Absolute).IndexOf('/', "https://".Length)) + "/umbraco/surface/checkout/PaymentNotify";
            var returnData = Checkout(model.Carts, model.PaymentMethod, successUrl, notifyUrl, model);
            var result = JsonConvert.DeserializeObject<CoinHomePayOrderReturnModel>(returnData);
            Session[AppConstant.SESSION_CART_ITEMS] = null;
            return Json(new
            {
                success = Convert.ToBoolean(result.success),
                message = result.message,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
        private string Checkout(List<Cart> model, string paymentMethod, string redirectUrl, string notifyUrl, CheckOutModel checkoutModel)
        {
            var resGuid = SaveOrdersToDatabase(checkoutModel);
            var merchanId = WebConfigurationManager.AppSettings["CoinHomePay.MerchanId"];
            var token = WebConfigurationManager.AppSettings["CoinHomePay.Token"];
            var url = WebConfigurationManager.AppSettings["CoinHomePay.CreateOrderUrl"];
            var client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            var outTradeNo = Utils.GetTimestamp(DateTime.Now);
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var goodName = $"Payment of #" + outTradeNo;
            var outBody = "";
            var sweepFeeUser = 0;
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
            redirectUrl = redirectUrl + "?data=" + resGuid;
            notifyUrl = notifyUrl + "?data=" + resGuid;
            var signString = $"channel={paymentMethod}&goodsName={goodName}&merchantId={merchanId}&money={money}&notifyUrl={notifyUrl}&outBody={outBody}&outTradeNo={outTradeNo}&returnUrl={redirectUrl}";
            if (sweepFeeUser > 0)
            {
                signString += "&sweepFeeUser=" + sweepFeeUser;
            }
            signString += "&timestamp=" + timestamp;
            var sign = Utils.ToMd5(signString + token).ToUpper();

            List<KeyValuePair<string, string>> requestParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("merchantId", merchanId),
                new KeyValuePair<string, string>("sign", sign),
                new KeyValuePair<string, string>("timestamp", timestamp),
                new KeyValuePair<string, string>("money", money.ToString()),
                new KeyValuePair<string, string>("channel", paymentMethod),
                new KeyValuePair<string, string>("outTradeNo", outTradeNo),
                new KeyValuePair<string, string>("notifyUrl", notifyUrl),
                new KeyValuePair<string, string>("returnUrl", redirectUrl),
                new KeyValuePair<string, string>("goodsName", goodName),
                new KeyValuePair<string, string>("outBody", outBody)
            };
            if (sweepFeeUser > 0)
            {
                requestParams.Add(new KeyValuePair<string, string>("sweepFeeUser", sweepFeeUser.ToString()));
            }

            var content = new FormUrlEncodedContent(requestParams);
            httpRequestMessage.Content = content;

            var res = string.Empty;
            using (var db = new XaviasaleContext())
            {
                var order = db.Orders.FirstOrDefault(x => x.ResponGuid.ToString().Equals(resGuid));
                order.RequestApi = JsonConvert.SerializeObject(requestParams);
                try
                {
                    var response = client.SendAsync(httpRequestMessage).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        client.Dispose();
                        res = dataObjects.Result;
                        order.ResponseApi = JsonConvert.SerializeObject(dataObjects.Result);
                    }
                    else
                    {
                        client.Dispose();
                    }
                }
                catch (Exception) { }
                db.Orders.Attach(order);
                db.Entry(order).Property(x => x.RequestApi).IsModified = true;
                db.Entry(order).Property(x => x.ResponseApi).IsModified = true;
                db.SaveChanges();
            }
            return res;
        }

        private string SaveOrdersToDatabase(CheckOutModel model)
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
                    return order.ResponGuid.ToString();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return string.Empty;
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
                        db.Orders.Attach(order);
                        db.Entry(order).Property(x => x.IsSuccess).IsModified = true;
                        db.Entry(order).Property(x => x.Status).IsModified = true;
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