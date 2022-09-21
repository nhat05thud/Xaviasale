using Xaviasale.EntityFramework.Context;
using Xaviasale.Models.BackOffice;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Xaviasale.ClassHelper;

namespace Xaviasale.Controllers
{
    public class BackOfficeShoppingCartController : SurfaceController
    {
        public ActionResult RenderViewShoppingCart()
        {
            return PartialView("~/Views/BackOfficeShoppingCart/Index.cshtml");
        }
        public ActionResult RenderViewStatistic()
        {
            return PartialView("~/Views/BackOfficeShoppingCart/_Statistic.cshtml");
        }
        public ActionResult GetData()
        {
            using (var db = new XaviasaleContext())
            {
                var itemCount = 0;
                var orders = db.Orders
                    .Select(x => new OrderViewModel
                    {
                        Id = x.OrderId,
                        CreateDate = x.CreateDate,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        OrderProducts = (from b in db.ShoppingCarts where b.OrderId == x.OrderId select new OrderProduct
                        {
                            ProductId = b.ProductId,
                            Quantity = b.Quantity,
                            Color = b.Color,
                            CouponId = b.CouponId
                        }).ToList()
                    }).ToList();

                foreach (var order in orders)
                {
                    var hasCoupon = false;
                    itemCount += 1;
                    order.ItemNo = itemCount;
                    foreach (var item in order.OrderProducts)
                    {
                        #region coupon
                        decimal discount = 0;
                        if (item.CouponId > 0 && hasCoupon == false)
                        {
                            var coupon = Umbraco.Content(item.CouponId);
                            discount = coupon.Value<decimal>("discount");
                        }
                        #endregion
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            var productPrice = itemColorNested.Value<decimal>("price");
                            order.TotalPrice += (discount > 0 ? (productPrice - productPrice * (discount / 100)) * item.Quantity : productPrice * item.Quantity);
                            if (item.CouponId > 0 && hasCoupon == false)
                            {
                                order.Discount = discount > 0 ? productPrice * (discount / 100) * item.Quantity : 0;
                                hasCoupon = true;
                            }
                        }
                    }
                }

                return Json(orders, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetProductByOrderId(int id = 0)
        {
            if (id > 0)
            {
                var itemCount = 0;
                using (var db = new XaviasaleContext())
                {
                    var products = db.ShoppingCarts
                        .Where(x => x.OrderId.Equals(id))
                        .Select(x => new OrderProduct
                        {
                            ProductId = x.ProductId,
                            Quantity = x.Quantity,
                            Color = x.Color,
                            CouponId = x.CouponId
                        })
                        .ToList();
                    var hasCoupon = false;
                    foreach (var item in products)
                    {
                        itemCount += 1;
                        item.ItemNo = itemCount;
                        #region coupon
                        decimal discount = 0;
                        var couponName = string.Empty;
                        if (item.CouponId > 0 && hasCoupon == false)
                        {
                            var coupon = Umbraco.Content(item.CouponId);
                            discount = coupon.Value<decimal>("discount");
                            couponName = coupon.Name;
                        }
                        #endregion
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            var productPrice = itemColorNested.Value<decimal>("price");
                            item.ProductName = product.Name;
                            item.ProductUrl = product.Url(mode: UrlMode.Absolute);
                            item.ProductPrice = (discount > 0 ? (productPrice - productPrice * (discount / 100)) : productPrice);

                            if (item.CouponId > 0 && hasCoupon == false)
                            {
                                item.CouponName = couponName;
                                item.Discount = discount > 0 ? productPrice * (discount / 100) : 0;
                                hasCoupon = true;
                            }
                        }
                    }
                    return Json(products, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }
        public ActionResult ViewOrder(int id = 0)
        {
            if (id > 0)
            {
                using (var db = new XaviasaleContext())
                {
                    var item = db.Orders.FirstOrDefault(x => x.OrderId == id);
                    if (item != null)
                    {
                        var model = new OrderViewModel
                        {
                            Id = item.OrderId,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Address = item.Address,
                            Apartment = item.Apartment,
                            ZipCode = item.ZipCode,
                            City = item.City,
                            State = item.State,
                            Country = item.Country,
                            Phone = item.Phone
                        };
                        return PartialView("~/Views/BackOfficeShoppingCart/_ViewOrder.cshtml", model);
                    }
                }
            }
            return PartialView("~/Views/BackOfficeShoppingCart/_ViewOrder.cshtml", new OrderViewModel());
        }
        [HttpPost]
        public ActionResult DeleteOrder(int id = 0)
        {
            using (var db = new XaviasaleContext())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {

                    if (id > 0)
                    {
                        var order = db.Orders.FirstOrDefault(x => x.OrderId == id);
                        if (order == null)
                        {
                            return Json(new { success = false, message = Umbraco.GetDictionaryValue("Order.Id.NotFound") }, JsonRequestBehavior.AllowGet);
                        }
                        var lstShoppingCarts = db.ShoppingCarts.Where(x => x.OrderId == order.OrderId).ToList();
                        foreach (var cart in lstShoppingCarts)
                        {
                            db.ShoppingCarts.Remove(cart);
                        }

                        db.Orders.Remove(order);
                        db.SaveChanges();
                        transaction.Commit();

                        return Json(new { success = true, message = Umbraco.GetDictionaryValue("Order.Delete.Success") }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = Umbraco.GetDictionaryValue("Order.Delete.Id.Zero") }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = Umbraco.GetDictionaryValue("Order.Error") }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpGet]
        public ActionResult GetStatisticByDateRange(string startDate, string endDate)
        {
            using (var db = new XaviasaleContext())
            {
                var itemCount = 0;
                var start = Utils.UnixTimeStampToDateTime(Convert.ToDouble(startDate)).EndOfDay();
                var end = Utils.UnixTimeStampToDateTime(Convert.ToDouble(endDate)).EndOfDay();
                var orders = db.Orders.Where(x => x.CreateDate >= start && x.CreateDate <= end).ToList();
                var lstProducts = new List<OrderProduct>();
                foreach (var o in orders)
                {
                    var products = db.ShoppingCarts
                        .Where(x => x.OrderId == o.OrderId)
                        .Select(x => new OrderProduct
                        {
                            ProductId = x.ProductId,
                            Quantity = x.Quantity,
                            Color = x.Color,
                            CouponId = x.CouponId
                        })
                        .ToList();

                    var hasCoupon = false;
                    foreach (var item in products)
                    {
                        #region coupon
                        decimal discount = 0;
                        if (item.CouponId > 0 && hasCoupon == false)
                        {
                            var coupon = Umbraco.Content(item.CouponId);
                            discount = coupon.Value<decimal>("discount");
                        }
                        #endregion
                        itemCount += 1;
                        item.ItemNo = itemCount;
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            var productPrice = itemColorNested.Value<decimal>("price");
                            item.ProductName = product.Name;
                            item.ProductUrl = product.Url(mode: UrlMode.Absolute);
                            item.ProductPrice = (discount > 0 ? (productPrice - productPrice * (discount / 100)) * item.Quantity : productPrice * item.Quantity);
                        }
                        lstProducts.Add(item);
                    }
                }

                return Json(lstProducts, JsonRequestBehavior.AllowGet);
            }
        }
    }
}