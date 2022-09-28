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
            var returnData = Checkout(model.Carts, model.PaymentMethod, home.DescendantOfType("checkOutSuccessPage")?.Url(mode: UrlMode.Absolute), model);
            var result = JsonConvert.DeserializeObject<CoinHomePayOrderReturnModel>(returnData);
            Session[AppConstant.SESSION_CART_ITEMS] = null;
            return Json(new
            {
                success = Convert.ToBoolean(result.success),
                message = result.message,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
        private string Checkout(List<Cart> model, string paymentMethod, string redirectUrl, CheckOutModel checkoutModel)
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
            var signString = $"channel={paymentMethod}&goodsName={goodName}&merchantId={merchanId}&money={money}&notifyUrl={redirectUrl}&outBody={outBody}&outTradeNo={outTradeNo}&returnUrl={redirectUrl}";
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
                new KeyValuePair<string, string>("notifyUrl", redirectUrl),
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
    }
}