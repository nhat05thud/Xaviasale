namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addColumnProductAmountAndDiscount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShoppingCarts", "ProductAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ShoppingCarts", "ProductDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShoppingCarts", "ProductDiscount");
            DropColumn("dbo.ShoppingCarts", "ProductAmount");
        }
    }
}
