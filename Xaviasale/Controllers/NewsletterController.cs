using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Xaviasale.EntityFramework.Context;
using Xaviasale.EntityFramework.Models;
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class NewsletterController : SurfaceController
    {
        // GET: Newsletter
        public ActionResult HandleSubscribe(NewsletterModel model)
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    var existItem = db.Newsletters.FirstOrDefault(x => x.Email.Equals(model.Email));
                    if (existItem != null)
                    {
                        return Json(new { 
                            success = false, 
                            message = Umbraco.GetDictionaryValue("Newsletter.Email.Already.Exists"),
                            view = ConvertViewToString("~/Views/Partials/Home/Form/_Form.cshtml", new NewsletterModel())
                        }, JsonRequestBehavior.AllowGet);
                    }
                    var data = new Newsletter { Email = model.Email };
                    db.Newsletters.Add(data);
                    db.SaveChanges();

                    return Json(new
                    {
                        success = true, 
                        message = Umbraco.GetDictionaryValue("Newsletter.Subscribe.Success"),
                        view = ConvertViewToString("~/Views/Partials/Home/Form/_Form.cshtml", new NewsletterModel())
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Umbraco.GetDictionaryValue("Newsletter.Error") }, JsonRequestBehavior.AllowGet);
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