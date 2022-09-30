namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddictionColumnsForOrderAndShoppingCartTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "OrderStatus", c => c.String());
            AddColumn("dbo.Orders", "AmountOrder", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "ShipFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ShoppingCarts", "ProductAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ShoppingCarts", "ProductDiscount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShoppingCarts", "ProductDiscount");
            DropColumn("dbo.ShoppingCarts", "ProductAmount");
            DropColumn("dbo.Orders", "ShipFee");
            DropColumn("dbo.Orders", "AmountOrder");
            DropColumn("dbo.Orders", "OrderStatus");
        }
    }
}
