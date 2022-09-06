using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Xaviasale.EntityFramework.Context;
using Xaviasale.EntityFramework.Models;
using Xaviasale.Models.BackOffice;

namespace Xaviasale.Controllers
{
    public class BackOfficeNewsletterController : SurfaceController
    {
        // GET: BackOfficeNewsletter
        public ActionResult Page()
        {
            return PartialView("~/Views/BackOfficeNewsletter/Index.cshtml");
        }
        [HttpGet]
        public ActionResult GetNewsletter()
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    var lstNewsletter = db.Newsletters
                        .Where(x => x.GroupNewsletterId == 0 || x.GroupNewsletterId == null)
                        .Select(x => new NewsletterViewModel
                        {
                            Id = x.Id,
                            Email = x.Email
                        }).ToList();
                    return Json(lstNewsletter, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [HttpPost]
        public ActionResult DeleteNewsletter(int id = 0)
        {
            if (id > 0)
            {
                using (var db = new XaviasaleContext())
                {
                    var item = db.Newsletters.FirstOrDefault(x => x.Id == id);
                    if (item == null)
                    {
                        return Json(new { success = false, message = Umbraco.GetDictionaryValue("Newsletter.Id.NotFound") }, JsonRequestBehavior.AllowGet);
                    }
                    db.Newsletters.Remove(item);
                    db.SaveChanges();
                    return Json(new { success = true, message = Umbraco.GetDictionaryValue("Newsletter.Delete.Success") }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = Umbraco.GetDictionaryValue("Newsletter.Delete.Id.Zero") }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListEmailOnSendmail()
        {
            using (var db = new XaviasaleContext())
            {
                var list = db.Newsletters
                    .Where(x => x.GroupNewsletterId == 0 || x.GroupNewsletterId == null)
                    .Select(x => new NewsletterViewModel
                    {
                        Id = x.Id,
                        Email = x.Email
                    }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetGroups()
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    var lstNewsletter = db.GroupNewsletters
                        .Select(x => new GroupNewsletterViewModel
                        {
                            Id = x.Id,
                            Name = x.Name
                        }).ToList();
                    return Json(lstNewsletter, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [HttpGet]
        public ActionResult ViewGroup(int id = 0)
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    if (id > 0)
                    {
                        var item = db.GroupNewsletters.FirstOrDefault(x => x.Id == id);
                        if (item != null)
                        {
                            var model = new GroupNewsletterViewModel
                            {
                                Id = item.Id,
                                Name = item.Name
                            };

                            var lstEmails = db.Newsletters
                                .Where(x => x.GroupNewsletterId == 0 || x.GroupNewsletterId == null || x.GroupNewsletterId == item.Id)
                                .Select(x => new NewsletterViewModel
                                {
                                    Id = x.Id,
                                    Email = x.Email,
                                    IsSelected = x.GroupNewsletterId == item.Id
                                }).ToList();

                            model.NewsletterViewModels = lstEmails;
                            return PartialView("~/Views/BackOfficeNewsletter/_ViewGroup.cshtml", model);
                        }
                    }
                    var emails = db.Newsletters
                        .Where(x => x.GroupNewsletterId == 0 || x.GroupNewsletterId == null)
                        .Select(x => new NewsletterViewModel
                        {
                            Id = x.Id,
                            Email = x.Email
                        }).ToList();

                    return PartialView("~/Views/BackOfficeNewsletter/_ViewGroup.cshtml", new GroupNewsletterViewModel { NewsletterViewModels = emails });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        [HttpPost]
        public ActionResult AddGroup(string name, List<int> ids)
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    var item = db.GroupNewsletters.FirstOrDefault(x => x.Name.ToLower().Equals(name.ToLower()));
                    if (item != null)
                    {
                        return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Name.Duplicate") }, JsonRequestBehavior.AllowGet);
                    }

                    // add group
                    var group = new GroupNewsletter
                    {
                        Name = name
                    };
                    db.GroupNewsletters.Add(group);
                    db.SaveChanges();

                    var groupId = group.Id;
                    var lstEmails = db.Newsletters.Where(x => ids.Contains(x.Id)).ToList();
                    foreach (var email in lstEmails)
                    {
                        email.GroupNewsletterId = groupId;
                        db.Newsletters.Attach(email);
                        db.Entry(email).Property(x => x.GroupNewsletterId).IsModified = true;
                        db.SaveChanges();
                    }

                    return Json(new { success = true, message = Umbraco.GetDictionaryValue("GroupNewsletter.Add.Success") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Error") }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult EditGroup(int id, string name, List<int> ids)
        {
            try
            {
                using (var db = new XaviasaleContext())
                {
                    var item = db.GroupNewsletters.FirstOrDefault(x => x.Name.ToLower().Equals(name.ToLower()) && x.Id != id);
                    if (item != null)
                    {
                        return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Name.Duplicate") }, JsonRequestBehavior.AllowGet);
                    }
                    
                    var existItem = db.GroupNewsletters.FirstOrDefault(x => x.Id == id);
                    if (existItem != null)
                    {
                        existItem.Name = name;
                        db.GroupNewsletters.Attach(existItem);
                        db.Entry(existItem).Property(x => x.Name).IsModified = true;
                        db.SaveChanges();

                        var lstExistsEmails = db.Newsletters.Where(x => x.GroupNewsletterId == existItem.Id).ToList();

                        foreach (var email in lstExistsEmails)
                        {
                            email.GroupNewsletterId = null;
                            db.Newsletters.Attach(email);
                            db.Entry(email).Property(x => x.GroupNewsletterId).IsModified = true;
                            db.SaveChanges();
                        }

                        if (ids != null)
                        {
                            var lstEmails = db.Newsletters.Where(x => ids.Contains(x.Id)).ToList();
                            foreach (var email in lstEmails)
                            {
                                email.GroupNewsletterId = existItem.Id;
                                db.Newsletters.Attach(email);
                                db.Entry(email).Property(x => x.GroupNewsletterId).IsModified = true;
                                db.SaveChanges();
                            }
                        }
                        return Json(new { success = true, message = Umbraco.GetDictionaryValue("GroupNewsletter.Edit.Success") }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Error") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Error") }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteGroup(int id = 0)
        {
            if (id > 0)
            {
                using (var db = new XaviasaleContext())
                {
                    var item = db.GroupNewsletters.FirstOrDefault(x => x.Id == id);
                    if (item == null)
                    {
                        return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Id.NotFound") }, JsonRequestBehavior.AllowGet);
                    }
                    var lstEmails = db.Newsletters.Where(x => x.GroupNewsletterId == item.Id).ToList();
                    foreach (var email in lstEmails)
                    {
                        email.GroupNewsletterId = null;
                        db.Newsletters.Attach(email);
                        db.Entry(email).Property(x => x.GroupNewsletterId).IsModified = true;
                        db.SaveChanges();
                    }

                    db.GroupNewsletters.Remove(item);
                    db.SaveChanges();

                    return Json(new { success = true, message = Umbraco.GetDictionaryValue("GroupNewsletter.Delete.Success") }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = Umbraco.GetDictionaryValue("GroupNewsletter.Delete.Id.Zero") }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListGroupOnSendmail()
        {
            using (var db = new XaviasaleContext())
            {
                var list = db.GroupNewsletters
                    .Select(x => new GroupNewsletterViewModel
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetListGroupOnAddGroup()
        {
            using (var db = new XaviasaleContext())
            {
                var list = db.Newsletters
                    .Where(x => x.GroupNewsletterId == 0 || x.GroupNewsletterId == null)
                    .Select(x => new NewsletterViewModel
                    {
                        Id = x.Id,
                        Email = x.Email
                    }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
    }
}