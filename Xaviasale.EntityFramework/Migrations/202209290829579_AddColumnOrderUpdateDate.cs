namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnOrderUpdateDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "UpdateDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "UpdateDate");
        }
    }
}
