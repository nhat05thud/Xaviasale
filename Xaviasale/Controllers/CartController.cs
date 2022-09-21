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
            if (product != null)
            {
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
                var newItem = new Cart();
                if (model.Carts.Count > 0 && existItem != null)
                {
                    existItem.Quantity += quantity;
                }
                else
                {
                    newItem = new Cart { ProductId = id, Quantity = quantity, Color = color };
                }
                #region coupon
                var coupons = product.Value<IEnumerable<IPublishedContent>>("coupons");
                if (coupons != null && coupons.Any())
                {
                    var couponId = 0;
                    foreach (var coupon in coupons.OrderByDescending(x => x.Value<int>("amount")).ToList())
                    {
                        if (model.Carts.Count > 0 && existItem != null)
                        {
                            if (coupon.Value<int>("amount") <= existItem.Quantity)
                            {
                                existItem.CouponId = coupon.Id;
                                break;
                            }
                        }
                        else
                        {
                            if (coupon.Value<int>("amount") <= quantity)
                            {
                                newItem.CouponId = coupon.Id;
                                break;
                            }
                        }
                    }
                }
                #endregion
                model.Carts.Add(newItem);
                Session[AppConstant.SESSION_CART_ITEMS] = model;
                var numberInCart = this.RenderCartNumber();
                var viewModel = RenderViewModel(product.Id);

                return Json(new
                {
                    success = true,
                    cartNumber = numberInCart,
                    couponSection = ConvertViewToString("~/Views/Partials/Product/_Coupon.cshtml", product.Id),
                    cartAside = ConvertViewToString("~/Views/Partials/Layout/_MiniCart.cshtml", viewModel),
                    responseMessage = "Add product to cart success",
                    responseType = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false,
                responseMessage = "Fail to add product to cart",
                responseType = "Fail"
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
                couponSection = ConvertViewToString("~/Views/Partials/Product/_Coupon.cshtml", id),
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
            #region coupon
            if (model != null && model.Carts != null && model.Carts.Any())
            {
                foreach (var item in model.Carts)
                {
                    var product = Umbraco.Content(item.ProductId);
                    var coupons = product.Value<IEnumerable<IPublishedContent>>("coupons");
                    if (coupons != null && coupons.Any())
                    {
                        foreach (var coupon in coupons.OrderByDescending(x => x.Value<int>("amount")).ToList())
                        {
                            if (model.Carts.Count > 0)
                            {
                                if (coupon.Value<int>("amount") <= item.Quantity)
                                {
                                    item.CouponId = coupon.Id;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
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
                    var hasCoupon = false;
                    foreach (var item in cartObject.Carts)
                    {
                        decimal discount = 0;
                        if (item.CouponId > 0 && hasCoupon == false)
                        {
                            var coupon = Umbraco.Content(item.CouponId);
                            discount = coupon.Value<decimal>("discount");
                        }
                        var itemContent = Umbraco.Content(item.ProductId);
                        if (itemContent != null)
                        {
                            var obj = new CartModel
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Color = item.Color,
                                ProductName = itemContent.Name,
                                ProductUrl = itemContent.Url(mode: UrlMode.Absolute),
                                CouponId = item.CouponId
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
                                obj.SubTotal = discount > 0 ? (obj.ProductPrice - obj.ProductPrice * (discount / 100)) * obj.Quantity : obj.ProductPrice * obj.Quantity;
                            }
                            if (item.CouponId > 0 && hasCoupon == false)
                            {
                                viewModel.Discount = discount > 0 ? itemColorNested.Value<decimal>("price") * (discount / 100) * item.Quantity : 0;
                                hasCoupon = true;
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
        [HttpPost]
        public ActionResult AddCoupon(int id, int couponId, string color)
        {
            var product = Umbraco.Content(id);
            if (product != null)
            {
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
                var coupon = Umbraco.Content(couponId);
                if (coupon == null)
                {
                    return Json(new
                    {
                        success = false,
                        responseMessage = "Fail to apply coupon",
                        responseType = "Fail"
                    }, JsonRequestBehavior.AllowGet);
                }
                var quantity = coupon.Value<int>("amount");
                var existItem = model.Carts.FirstOrDefault(x => x.ProductId.Equals(id) && x.Color.Equals(color));
                if (model.Carts.Count > 0 && existItem != null)
                {
                    existItem.Quantity += quantity;
                    existItem.CouponId = couponId;
                }
                else
                {
                    model.Carts.Add(new Cart { ProductId = id, Quantity = quantity, Color = color, CouponId = couponId });
                }
                Session[AppConstant.SESSION_CART_ITEMS] = model; 
                
                var home = product.Root();
                var CartUrl = home.DescendantOfType("cart")?.Url(mode: UrlMode.Absolute);

                return Json(new
                {
                    success = true,
                    redirectUrl = CartUrl,
                    responseMessage = "Add product to cart success",
                    responseType = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = false,
                responseMessage = "Fail to add product to cart",
                responseType = "Fail"
            }, JsonRequestBehavior.AllowGet);
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