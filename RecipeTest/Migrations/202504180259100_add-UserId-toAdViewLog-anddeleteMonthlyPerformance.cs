namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserIdtoAdViewLoganddeleteMonthlyPerformance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdViewLogs", "UserId", c => c.Int());
            AddColumn("dbo.AdViewLogs", "AdDisplayPage", c => c.Int(nullable: false));
            DropTable("dbo.AdMonthlyPerformances");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AdMonthlyPerformances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdId = c.Int(nullable: false),
                        ClickCount = c.Int(nullable: false),
                        ExposureCount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AdViewLogs", "AdDisplayPage");
            DropColumn("dbo.AdViewLogs", "UserId");
        }
    }
}
