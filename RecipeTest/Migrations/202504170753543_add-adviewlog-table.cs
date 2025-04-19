namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addadviewlogtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdViewLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdId = c.Int(nullable: false),
                        SessionId = c.String(maxLength: 100),
                        IsClick = c.Boolean(nullable: false),
                        ViewedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Advertisements", "AdIntro", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advertisements", "AdIntro");
            DropTable("dbo.AdViewLogs");
        }
    }
}
