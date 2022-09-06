using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xaviasale.EntityFramework.Models
{
    public class GroupNewsletter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Newsletter> Newsletters { get; set; }
    }
}
