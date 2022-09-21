using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Xaviasale.Models.BackOffice
{
    public class GroupNewsletterViewModel
    {
        public int ItemNo { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<NewsletterViewModel> NewsletterViewModels { get; set; }
    }
}