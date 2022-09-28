namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTraceColumnOrderTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "IsSuccess", c => c.Boolean(nullable: false));
            AddColumn("dbo.Orders", "IsDelete", c => c.Boolean(nullable: false));
            AddColumn("dbo.Orders", "RequestApi", c => c.String());
            AddColumn("dbo.Orders", "ResponseApi", c => c.String());
            AddColumn("dbo.Orders", "ResponGuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "ResponGuid");
            DropColumn("dbo.Orders", "ResponseApi");
            DropColumn("dbo.Orders", "RequestApi");
            DropColumn("dbo.Orders", "IsDelete");
            DropColumn("dbo.Orders", "IsSuccess");
        }
    }
}
