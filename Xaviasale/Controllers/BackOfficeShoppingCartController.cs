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

namespace Xaviasale.Controllers
{
    public class BackOfficeShoppingCartController : SurfaceController
    {
        public ActionResult RenderViewShoppingCart()
        {
            return PartialView("~/Views/BackOfficeShoppingCart/Index.cshtml");
        }
        public ActionResult GetData()
        {
            using (var db = new XaviasaleContext())
            {
                var orders = db.Orders.Select(x => new OrderViewModel
                {
                    Id = x.OrderId,
                    CreateDate = x.CreateDate,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email
                }).ToList();
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
    }
}