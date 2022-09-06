using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Xaviasale.Models.BackOffice
{
    public class NewsletterViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsSelected { get; set; }
    }
}