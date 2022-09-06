using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xaviasale.EntityFramework.Models
{
    public class Newsletter
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int? GroupNewsletterId { get; set; }
        public GroupNewsletter GroupNewsletter { get; set; }
    }
}
