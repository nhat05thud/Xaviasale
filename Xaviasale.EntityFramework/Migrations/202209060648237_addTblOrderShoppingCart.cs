namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTblOrderShoppingCart : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Address = c.String(),
                        Apartment = c.String(),
                        ZipCode = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Country = c.String(),
                        Phone = c.String(),
                        IsReaded = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId);
            
            CreateTable(
                "dbo.ShoppingCarts",
                c => new
                    {
                        ShoppingCartId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        Color = c.String(),
                        Quantity = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ShoppingCartId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ShoppingCarts", "OrderId", "dbo.Orders");
            DropIndex("dbo.ShoppingCarts", new[] { "OrderId" });
            DropTable("dbo.ShoppingCarts");
            DropTable("dbo.Orders");
        }
    }
}
