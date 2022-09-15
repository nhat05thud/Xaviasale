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
                            Color = b.Color
                        }).ToList()
                    }).ToList();

                foreach (var order in orders)
                {
                    foreach (var item in order.OrderProducts)
                    {
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            order.TotalPrice += itemColorNested.Value<decimal>("price") * item.Quantity;
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
                using (var db = new XaviasaleContext())
                {
                    var products = db.ShoppingCarts
                        .Where(x => x.OrderId.Equals(id))
                        .Select(x => new OrderProduct
                        {
                            ProductId = x.ProductId,
                            Quantity = x.Quantity,
                            Color = x.Color
                        })
                        .ToList();
                    foreach (var item in products)
                    {
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            item.ProductName = product.Name;
                            item.ProductUrl = product.Url(mode: UrlMode.Absolute);
                            item.ProductPrice = itemColorNested.Value<decimal>("price");
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
                            Color = x.Color
                        })
                        .ToList();

                    foreach (var item in products)
                    {
                        var product = Umbraco.Content(item.ProductId);
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            item.ProductName = product.Name;
                            item.ProductUrl = product.Url(mode: UrlMode.Absolute);
                            item.ProductPrice = itemColorNested.Value<decimal>("price");
                        }
                        lstProducts.Add(item);
                    }
                }

                return Json(lstProducts, JsonRequestBehavior.AllowGet);
            }
        }
    }
}