namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AllowNullForeignkeyGroup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Newsletters", "GroupNewsletterId", "dbo.GroupNewsletters");
            DropIndex("dbo.Newsletters", new[] { "GroupNewsletterId" });
            AlterColumn("dbo.Newsletters", "GroupNewsletterId", c => c.Int());
            CreateIndex("dbo.Newsletters", "GroupNewsletterId");
            AddForeignKey("dbo.Newsletters", "GroupNewsletterId", "dbo.GroupNewsletters", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Newsletters", "GroupNewsletterId", "dbo.GroupNewsletters");
            DropIndex("dbo.Newsletters", new[] { "GroupNewsletterId" });
            AlterColumn("dbo.Newsletters", "GroupNewsletterId", c => c.Int(nullable: false));
            CreateIndex("dbo.Newsletters", "GroupNewsletterId");
            AddForeignKey("dbo.Newsletters", "GroupNewsletterId", "dbo.GroupNewsletters", "Id", cascadeDelete: true);
        }
    }
}
