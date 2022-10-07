using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using Xaviasale.EntityFramework.Context;
using Xaviasale.EntityFramework.Models;
using Xaviasale.Models;

namespace Xaviasale.Controllers
{
    public class checkOutSuccessPageController : RenderMvcController
    {
        // GET: checkOutSuccessPage
        public override ActionResult Index(ContentModel model)
        {
            Session[AppConstant.SESSION_CART_ITEMS] = null;
            return CurrentTemplate(model);
        }
    }
}