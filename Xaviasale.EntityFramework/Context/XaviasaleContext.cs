using System.Data.Entity;
using Xaviasale.EntityFramework.Models;

namespace Xaviasale.EntityFramework.Context
{
    public class XaviasaleContext : DbContext
    {
        public XaviasaleContext() : base("umbracoDbDSN")
        {
        }
        public virtual DbSet<Newsletter> Newsletters { get; set; }
        public virtual DbSet<GroupNewsletter> GroupNewsletters { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
    }
}
