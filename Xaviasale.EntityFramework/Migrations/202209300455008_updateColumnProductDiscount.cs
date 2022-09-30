namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateColumnProductDiscount : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ShoppingCarts", "ProductDiscount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ShoppingCarts", "ProductDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
