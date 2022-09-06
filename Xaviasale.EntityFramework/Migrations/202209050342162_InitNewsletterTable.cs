namespace Xaviasale.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitNewsletterTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Newsletters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        GroupNewsletterId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupNewsletters", t => t.GroupNewsletterId, cascadeDelete: true)
                .Index(t => t.GroupNewsletterId);
            
            CreateTable(
                "dbo.GroupNewsletters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Newsletters", "GroupNewsletterId", "dbo.GroupNewsletters");
            DropIndex("dbo.Newsletters", new[] { "GroupNewsletterId" });
            DropTable("dbo.GroupNewsletters");
            DropTable("dbo.Newsletters");
        }
    }
}
