using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Xaviasale.ClassHelper;
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class ProductController : SurfaceController
    {
        // GET: Product
        public ActionResult LoadProducts(string lang = "", string sort = ":", int page = 1)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);
            var root = Umbraco.ContentAtRoot().FirstOrDefault(x => x.Cultures.ContainsKey(lang));
            var sortby = sort.Split(':');
            var lstProducts = new List<IPublishedContent>();
            switch (sortby.First())
            {
                case var value when value == Utils.SortByCreatedDate:
                    lstProducts = sortby.Last().Equals("desc")
                        ? root.DescendantsOfType("product").OrderByDescending(x => x.CreateDate).ToList()
                        : root.DescendantsOfType("product").OrderBy(x => x.CreateDate).ToList();
                    break;
                case var value when value == Utils.SortByName:
                    lstProducts = sortby.Last().Equals("desc")
                        ? root.DescendantsOfType("product").OrderByDescending(x => x.Name).ToList()
                        : root.DescendantsOfType("product").OrderBy(x => x.Name).ToList();
                    break;
                case var value when value == Utils.SortByPrice:
                    lstProducts = sortby.Last().Equals("desc")
                        ? root.DescendantsOfType("product").OrderByDescending(x => x.Value<int>("price")).ToList()
                        : root.DescendantsOfType("product").OrderBy(x => x.Value<int>("price")).ToList();
                    break;
                default:
                    lstProducts = Umbraco.AssignedContentItem.Root().DescendantsOfType("product")
                        .OrderByDescending(x => x.SortOrder).ToList();
                    break;
            }
            return PartialView("~/Views/Partials/Product/_Products.cshtml", lstProducts);
        }

        public ActionResult Loadrecentlyproducts(string ids)
        {
            var model = new List<IPublishedContent>();
            var lstId = ids.Split('|');
            foreach (var id in lstId)
            {
                var product = Umbraco.Content(id);
                model.Add(product);
            }
            return PartialView("~/Views/Partials/Layout/ProductsCarousel/_Recently.cshtml", model);
        }
        public ActionResult LoadProductContentAjax(int pageId, string keyId = "")
        {
            var currentPage = Umbraco.Content(pageId);
            var lstProducts = currentPage.Value<IEnumerable<IPublishedElement>>("productColorNested");
            var data = string.IsNullOrEmpty(keyId)
                ? lstProducts.FirstOrDefault()
                : lstProducts.FirstOrDefault(x => x.Key.ToString().Equals(keyId));

            var model = new ProductDetailModel
            {
                Id = currentPage.Id,
                ProductColors = currentPage.Value<IEnumerable<IPublishedElement>>("productColorNested"),
                Name = currentPage.Name,
                Description = currentPage.Value<string>("productDescription"),
                Warranty = currentPage.Value<string>("returnAndWarranty"),
                Images = data.Value<IEnumerable<IPublishedContent>>("images"),
                Price = data.Value<decimal>("price"),
                OldPrice = data.Value<decimal>("oldPrice"),
                Save = data.Value<int>("save"),
                ColorName = data.Value<string>("title"),
                MetaTitle = !string.IsNullOrEmpty(currentPage.Value<string>("metaTitle")) ? currentPage.Value<string>("metaTitle") : currentPage.Name,
                MetaDescription = !string.IsNullOrEmpty(currentPage.Value<string>("metaDescription")) ? currentPage.Value<string>("metaDescription") : currentPage.Name,
                MetaThumbnails = currentPage.Value<IPublishedContent>("metaThumbnails") != null ? currentPage.Value<IPublishedContent>("metaThumbnails").Url(mode: UrlMode.Absolute) : data.Value<IEnumerable<IPublishedContent>>("images") != null && data.Value<IEnumerable<IPublishedContent>>("images").Any() ? data.Value<IEnumerable<IPublishedContent>>("images").First()?.Url(mode: UrlMode.Absolute) : "",
                Coupons = currentPage.Value<IEnumerable<IPublishedContent>>("coupons"),
                IsOutOfStock = currentPage.Value<bool>("isOutOfStock")
            };
            return PartialView("~/Views/Partials/Product/_ProductContentAjax.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult HandleReview(ReviewModel model)
        {
            var mediaService = Services.MediaService;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(model.CultureLcid);

            if (!ModelState.IsValid)
            {
                model.ErrorMsg = Umbraco.GetDictionaryValue("Message.Error");
                return PartialView("~/Views/Partials/Product/Review/_Form.cshtml", model);
            }

            var currentPage = Umbraco.Content(model.PageId);
            var lstReviews = currentPage.Value<IEnumerable<IPublishedElement>>("reviewNested");
            var cs = Services.ContentService;
            var parent = cs.GetById(Convert.ToInt32(model.PageId));
            var review = new List<Dictionary<string, object>>();
            var guid = Guid.NewGuid();

            var listOfFiles = new List<IMedia>();
            if (model.ImagesGallery != null && model.ImagesGallery.Count() > 0 )
            {
                var listFile = model.ImagesGallery;
                var mediaRoot = mediaService.GetRootMedia().FirstOrDefault(x => x.Name.Equals("Review"));
                if (mediaRoot == null)
                {
                    mediaRoot = mediaService.CreateMedia("Review", -1, "Folder");
                    mediaService.Save(mediaRoot);
                }
                foreach (var file in listFile)
                {
                    var mediaFile = mediaService.CreateMedia(file.FileName, mediaRoot.Id, Constants.Conventions.MediaTypes.Image);
                    mediaFile.SetValue(Services.ContentTypeBaseServices, Constants.Conventions.Media.File, file.FileName, file);
                    mediaService.Save(mediaFile);
                    var mediaItem = mediaService.GetById(mediaFile.Id);
                    listOfFiles.Add(mediaItem);
                }
            }
            if (lstReviews != null && lstReviews.Any())
            {
                foreach (var item in lstReviews)
                {
                    var lstMediaImages = new List<IMedia>();
                    var lstImages = item.Value<IEnumerable<IPublishedContent>>("images");
                    if (lstImages != null && lstImages.Any())
                    {
                        foreach (var img in lstImages)
                        {
                            var mediaItem = mediaService.GetById(img.Id);
                            lstMediaImages.Add(mediaItem);
                        }
                    }
                    review.Add(new Dictionary<string, object>()
                    {
                        {"key", item.Key},
                        {"name", $"{item.Value("title")} - {item.Value("reviewName")} - {item.Value("email")}"},
                        {"ncContentTypeAlias","reviewNested"},
                        {"title", item.Value("title")},
                        {"reviewName", item.Value("reviewName")},
                        {"email", item.Value("email")},
                        {"approved", item.Value("approved")},
                        {"star", item.Value("star")},
                        {"review", item.Value("review")},
                        {"createdDate", item.Value("createdDate")},
                        {"images", lstMediaImages != null && lstMediaImages.Count > 0 ? string.Join(",", lstMediaImages.Select(x => x.GetUdi()).Where(x => !string.IsNullOrEmpty(x.ToString()))) : "" }
                    });
                }
            }
            review.Add(new Dictionary<string, object>()
            {
                {"key", guid},
                {"name", $"{model.Title} - {model.Name} - {model.Email}"},
                {"ncContentTypeAlias","reviewNested"},
                {"title", model.Title},
                {"reviewName", model.Name},
                {"email", model.Email},
                {"approved", "false"},
                {"star", model.Star.ToString()},
                {"review", model.Review},
                {"createdDate", model.CreatedDate},
                {"images", string.Join(",", listOfFiles.Select(x => x.GetUdi()).Where(x => !string.IsNullOrEmpty(x.ToString()))) }
            });
            parent.SetValue("reviewNested", Newtonsoft.Json.JsonConvert.SerializeObject(review), Thread.CurrentThread.CurrentUICulture.Name);
            cs.SaveAndPublish(parent);

            //All done - lets redirect to the current page & show our thanks/success message
            model.Name = "";
            model.Email = "";
            model.Review = "";
            model.Title = "";
            model.Star = 5;
            model.ErrorMsg = Umbraco.GetDictionaryValue("Message.Success");
            ModelState.Clear();
            return PartialView("~/Views/Partials/Product/Review/_Form.cshtml", model);
        }
        private List<IMedia> ListMedia()
        {
            return null;
        }
    }
}