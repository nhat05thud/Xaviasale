namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addColumnAmountOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "AmountOrder", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "AmountOrder");
        }
    }
}
