using System;
using System.Collections.Generic;
using System.Globalization;
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
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class SiteController : SurfaceController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult HandleContactForm(ContactModel model)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(model.CultureLcid);

            if (!ModelState.IsValid)
            {
                model.ErrorMsg = Umbraco.GetDictionaryValue("Message.Error");
                return PartialView("~/Views/Partials/Contact/_Form.cshtml", model);
            }
            var sendTo = WebConfigurationManager.AppSettings["EmailContactReceive"];
            var messageString = "<h3>" + WebConfigurationManager.AppSettings["EmailContactTitle"] + "</h3>";
            messageString += "<b>Name: </b>" + model.Name + "<br />";
            messageString += "<b>Email: </b>" + model.Email + "<br />";
            messageString += "<b>Phone: </b>" + model.Phone + "<br />";
            messageString += "<b>Issue type: </b>" + model.IssueType + "<br />";
            messageString += "<b>Product/Collection link: </b>" + model.ProductLink + "<br />";
            messageString += "<b>Order number: </b>" + model.OrderNumber + "<br />";
            messageString += "<b>Message: </b>" + model.Message;
            var email = new MailMessage
            {
                Subject = "Contact",
                Body = messageString,
                IsBodyHtml = true,
                To = { sendTo }
            };
            try
            {
                var smtp = new SmtpClient();
                smtp.Send(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //All done - lets redirect to the current page & show our thanks/success message
            model.Name = "";
            model.Email = "";
            model.Phone = "";
            model.IssueType = "";
            model.ProductLink = "";
            model.OrderNumber = "";
            model.Message = "";
            model.ErrorMsg = Umbraco.GetDictionaryValue("Message.Success");
            ModelState.Clear();
            return PartialView("~/Views/Partials/Contact/_Form.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult RenderSiteCart()
        {
            var model = new CartViewModel
            {
                CartModels = new List<CartModel>()
            };
            if (Session[AppConstant.SESSION_CART_ITEMS] != null)
            {
                var home = Umbraco.ContentAtRoot().FirstOrDefault();
                model.HomeUrl = home?.Url(mode: UrlMode.Absolute);
                model.CartUrl = home.DescendantOfType("cart")?.Url(mode: UrlMode.Absolute);
                model.CheckOutUrl = home.DescendantOfType("checkOut")?.Url(mode: UrlMode.Absolute);
                var cartObject = (CartSession)Session[AppConstant.SESSION_CART_ITEMS];
                if (cartObject.Carts != null)
                {
                    var hasCoupon = false;
                    foreach (var item in cartObject.Carts)
                    {
                        decimal discount = 0;
                        if (item.CouponId > 0 && hasCoupon == false)
                        {
                            var coupon = Umbraco.Content(item.CouponId);
                            discount = coupon.Value<decimal>("discount");
                        }
                        var product = Umbraco.Content(item.ProductId);
                        if (product != null)
                        {
                            var obj = new CartModel
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Color = item.Color,
                                ProductName = product.Name,
                                ProductUrl = product.Url(mode: UrlMode.Absolute),
                                CouponId = item.CouponId
                            };
                            var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                            if (itemColorNested != null)
                            {
                                obj.ProductOldPrice = itemColorNested.Value<decimal>("oldPrice");
                                obj.ProductPrice = itemColorNested.Value<decimal>("price");
                                obj.ThumbnailPath = itemColorNested.Value<IEnumerable<IPublishedContent>>("images") != null && itemColorNested.Value<IEnumerable<IPublishedContent>>("images").Any()
                                    ? itemColorNested.Value<IEnumerable<IPublishedContent>>("images").FirstOrDefault()
                                        .GetCropUrl(224, 224, imageCropMode: ImageCropMode.Crop,
                                            furtherOptions: "&bgcolor=fff&slimmage=true")
                                    : "https://via.placeholder.com/224x224";
                                obj.SubTotal = discount > 0 ? (obj.ProductPrice - obj.ProductPrice * (discount / 100)) * obj.Quantity : obj.ProductPrice * obj.Quantity;
                            }
                            if (item.CouponId > 0 && hasCoupon == false)
                            {
                                model.Discount = discount > 0 ? itemColorNested.Value<decimal>("price") * (discount / 100) * item.Quantity : 0;
                                hasCoupon = true;
                            }
                            model.CartModels.Add(obj);
                        }
                    }
                    var total = model.CartModels.Sum(x => x.SubTotal);
                    model.TotalPrice = total;
                }
            }
            return PartialView("~/Views/Partials/Layout/_CartAsideBody.cshtml", model);
        }
    }
}