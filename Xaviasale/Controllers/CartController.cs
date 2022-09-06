using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PropertyEditors;
using Xaviasale.ClassHelper;
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class CartController : SurfaceController
    {
        [HttpPost]
        public ActionResult AddToCart(int id, string color, int quantity = 1)
        {
            var product = Umbraco.Content(id);
            if (string.IsNullOrEmpty(color))
            {
                var productColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested");
                color = productColorNested.FirstOrDefault().Value<string>("title");
            }
            var model = Session[AppConstant.SESSION_CART_ITEMS] != null ? (CartSession)Session[AppConstant.SESSION_CART_ITEMS] : new CartSession();
            if (model.Carts == null)
            {
                model.Carts = new List<Cart>();
            }
            var existItem = model.Carts.FirstOrDefault(x => x.ProductId.Equals(id) && x.Color.Equals(color));
            if (model.Carts.Count > 0 && existItem != null)
            {
                existItem.Quantity += quantity;
            }
            else
            {
                model.Carts.Add(new Cart { ProductId = id, Quantity = quantity, Color = color });
            }
            Session[AppConstant.SESSION_CART_ITEMS] = model;
            var numberInCart = this.RenderCartNumber();
            var viewModel = RenderViewModel(id);

            return Json(new
            {
                success = true,
                cartNumber = numberInCart,
                cartAside = ConvertViewToString("~/Views/Partials/Layout/_MiniCart.cshtml", viewModel),
                responseMessage = "Add product to cart success",
                responseType = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult BuyNow(int id, string color, int quantity = 1)
        {
            var product = Umbraco.Content(id);
            if (string.IsNullOrEmpty(color))
            {
                var productColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested");
                color = productColorNested.FirstOrDefault().Value<string>("title");
            }
            var model = Session[AppConstant.SESSION_CART_ITEMS] != null ? (CartSession)Session[AppConstant.SESSION_CART_ITEMS] : new CartSession();
            if (model.Carts == null)
            {
                model.Carts = new List<Cart>();
            }
            var existItem = model.Carts.FirstOrDefault(x => x.ProductId.Equals(id) && x.Color.Equals(color));
            if (model.Carts.Count > 0 && existItem != null)
            {
                existItem.Quantity += quantity;
            }
            else
            {
                model.Carts.Add(new Cart { ProductId = id, Quantity = quantity, Color = color });
            }
            Session[AppConstant.SESSION_CART_ITEMS] = model;

            var home = Umbraco.Content(id).Root();
            var CheckOutUrl = home.DescendantOfType("checkOut")?.Url(mode: UrlMode.Absolute);

            return Json(new
            {
                checkoutUrl = CheckOutUrl
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RemoveItemInCart(int id, string color)
        {
            var model = Session[AppConstant.SESSION_CART_ITEMS] != null ? (CartSession)Session[AppConstant.SESSION_CART_ITEMS] : new CartSession();
            if (model.Carts == null)
            {
                model.Carts = new List<Cart>();
            }
            var existItem = model.Carts.FirstOrDefault(x => x.ProductId.Equals(id) && x.Color.Equals(color));
            if (model.Carts.Count > 0 && existItem != null)
            {
                model.Carts.Remove(existItem);
            }
            Session[AppConstant.SESSION_CART_ITEMS] = model;
            var numberInCart = this.RenderCartNumber();
            var viewModel = RenderViewModel(id);

            return Json(new
            {
                success = true,
                cartNumber = numberInCart,
                cartAside = ConvertViewToString("~/Views/Partials/Layout/_MiniCart.cshtml", viewModel),
                responseMessage = "Remove product success",
                responseType = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        [ChildActionOnly]
        public int RenderCartNumber()
        {
            var itemsInCart = 0;
            if (Session[AppConstant.SESSION_CART_ITEMS] != null)
            {
                var cartObject = (CartSession)Session[AppConstant.SESSION_CART_ITEMS];
                itemsInCart = cartObject.Carts != null ? cartObject.Carts.Sum(x => x.Quantity) : 0;
            }
            return itemsInCart;
        }
        [HttpPost]
        public ActionResult UpdateCart(CartSession model)
        {
            Session[AppConstant.SESSION_CART_ITEMS] = model;
            TempData["UpdateCartStatus"] = "Update cart success";
            return RedirectToCurrentUmbracoPage();
        }
        [HttpGet]
        public ActionResult RenderCartItems()
        {
            var model = RenderViewModel();
            return PartialView("~/Views/Partials/Cart/_CartItems.cshtml", model);
        }
        [HttpGet]
        public ActionResult RenderEmptyCart()
        {
            var model = new EmptyCart
            {
                DictionaryText = "Cart empty",
                DictionaryButton = "Go To Home",
                RootUrl = "/"
            };
            return PartialView("~/Views/Partials/Cart/_CartEmpty.cshtml", model);
        }

        private CartViewModel RenderViewModel(int id = 0)
        {
            // cart Aside
            var viewModel = new CartViewModel
            {
                CartModels = new List<CartModel>()
            };
            if (Session[AppConstant.SESSION_CART_ITEMS] != null)
            {
                var home = id == 0 ? Umbraco.ContentAtRoot().FirstOrDefault() : Umbraco.Content(id).Root();
                viewModel.HomeUrl = home?.Url(mode: UrlMode.Absolute);
                viewModel.CartUrl = home.DescendantOfType("cart")?.Url(mode: UrlMode.Absolute);
                viewModel.CheckOutUrl = home.DescendantOfType("checkOut")?.Url(mode: UrlMode.Absolute);
                var cartObject = (CartSession)Session[AppConstant.SESSION_CART_ITEMS];
                if (cartObject.Carts != null)
                {
                    foreach (var item in cartObject.Carts)
                    {
                        var itemContent = Umbraco.Content(item.ProductId);
                        if (itemContent != null)
                        {
                            var obj = new CartModel
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Color = item.Color,
                                ProductName = itemContent.Name,
                                ProductUrl = itemContent.Url(mode: UrlMode.Absolute)
                            };
                            var itemColorNested = itemContent.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                            if (itemColorNested != null)
                            {
                                obj.ProductOldPrice = itemColorNested.Value<decimal>("oldPrice");
                                obj.ProductPrice = itemColorNested.Value<decimal>("price");
                                obj.ThumbnailPath = itemColorNested.Value<IEnumerable<IPublishedContent>>("images") != null && itemColorNested.Value<IEnumerable<IPublishedContent>>("images").Any()
                                    ? itemColorNested.Value<IEnumerable<IPublishedContent>>("images").FirstOrDefault()
                                        .GetCropUrl(224, 224, imageCropMode: ImageCropMode.Crop,
                                            furtherOptions: "&bgcolor=fff&slimmage=true")
                                    : "https://via.placeholder.com/224x224";
                                obj.SubTotal = itemColorNested.Value<decimal>("price") * item.Quantity;
                            }

                            viewModel.CartModels.Add(obj);
                        }
                    }
                    var total = viewModel.CartModels.Sum(x => x.SubTotal);
                    viewModel.TotalPrice = total;
                }
            }

            return viewModel;
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