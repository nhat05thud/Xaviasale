using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Xaviasale.Models.BackOffice
{
    public class SendMailViewModel
    {
        public string Emails { get; set; }
        public string Groups { get; set; }
        public string Subject { get; set; }
        [AllowHtml]
        public string Content { get; set; }
    }
}