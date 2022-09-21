namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCoumnCouponCart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShoppingCarts", "CouponId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShoppingCarts", "CouponId");
        }
    }
}
