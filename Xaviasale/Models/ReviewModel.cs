using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Xaviasale.ClassHelper;

namespace Xaviasale.Models
{
    public class ReviewModel : BaseModel
    {
        public int Star { get; set; }
        public string Title { get; set; }
        public string Review { get; set; }
        [UmbracoRequired("Form.Review.Name.Required")]
        public string Name { get; set; }
        [UmbracoEmail("Form.Review.Email.Validation")]
        public string Email { get; set; }
        public int PageId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}