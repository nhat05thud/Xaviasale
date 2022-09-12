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
            if (Request.Params["data"] != null)
            {
                var emailModel = JsonConvert.DeserializeObject<CheckOutModel>(Utils.DecryptString(Request.Params["data"]));
                var email = RenderMailMessage(emailModel);
                try
                {
                    var smtp = new SmtpClient();
                    smtp.Send(email);
                }
                catch (Exception ex)
                {
                    throw;
                }
                SaveOrdersToDatabase(emailModel);
            }

            return CurrentTemplate(model);
        }

        private void SaveOrdersToDatabase(CheckOutModel model)
        {
            using (var db = new XaviasaleContext())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var order = new Order
                    {
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Address = model.Address,
                        Apartment = model.Apartment,
                        ZipCode = model.ZipCode,
                        City = model.City,
                        State = model.State,
                        Country = model.Country,
                        Phone = model.Phone,
                        IsReaded = false,
                        CreateDate = DateTime.Now
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    if (model.Carts != null)
                    {
                        foreach (var product in model.Carts)
                        {
                            var item = new ShoppingCart
                            {
                                OrderId = order.OrderId,
                                ProductId = product.ProductId,
                                Quantity = product.Quantity,
                                Color = product.Color
                            };
                            db.ShoppingCarts.Add(item);
                            db.SaveChanges();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    // TODO: Log exception....    
                    transaction.Rollback();
                }
            }
        }
        private MailMessage RenderMailMessage(CheckOutModel model)
        {
            decimal total = 0;

            var sendTo = WebConfigurationManager.AppSettings["EmailContactReceive"];
            var messageString = "<h3>" + WebConfigurationManager.AppSettings["EmailCheckOutTitle"] + "</h3>";
            messageString += "<b>Email: </b>" + model.Email + "<br />";
            messageString += "<b>First Name: </b>" + model.FirstName + "<br />";
            messageString += "<b>Last Name: </b>" + model.LastName + "<br />";
            messageString += "<b>Address: </b>" + model.Address + "<br />";
            messageString += "<b>Apartment, suite, etc.: </b>" + model.Apartment + "<br />";
            messageString += "<b>Zip Code: </b>" + model.ZipCode + "<br />";
            messageString += "<b>City: </b>" + model.City + "<br />";
            messageString += "<b>State: </b>" + model.State + "<br />";
            messageString += "<b>Country: </b>" + model.Country + "<br />";
            messageString += "<b>Phone number: </b>" + model.Phone + "<br />";
            messageString += "<p>==================================================</p>";
            messageString += "<b>Orders: </b><br />";
            messageString += "<ul>";
            if (model.Carts != null)
            {
                foreach (var item in model.Carts)
                {
                    var product = Umbraco.Content(item.ProductId);
                    if (product != null)
                    {
                        var itemColorNested = product.Value<IEnumerable<IPublishedElement>>("productColorNested").FirstOrDefault(x => x.Value<string>("title").Equals(item.Color));
                        if (itemColorNested != null)
                        {
                            messageString += "<li>";
                            messageString += "<b>Sản phẩm:</b> <a href='" + product.Url(mode: UrlMode.Absolute) + "'>" + product.Name + "</a><br />";
                            messageString += "<b>Color:</b>" + item.Color + "<br />";
                            messageString += "<b>Số lượng:</b> " + item.Quantity + "<br />";
                            messageString += "<b>Giá (đơn vị 1 cái):</b> $" + itemColorNested.Value<decimal>("price") + "<br />";
                            messageString += "<b>Tổng:</b> $" + itemColorNested.Value<decimal>("price") * item.Quantity + "<br />";
                            messageString += "</li>";
                            total += itemColorNested.Value<decimal>("price") * item.Quantity;
                        }
                    }
                }
            }
            messageString += "</ul>";
            messageString += "<p>==================================================</p>";
            messageString += "<b>Tổng tiền: </b>$" + total + "<br />";

            var email = new MailMessage
            {
                Subject = "Order",
                Body = messageString,
                IsBodyHtml = true,
                To = { sendTo }
            };
            return email;
        }
    }
}