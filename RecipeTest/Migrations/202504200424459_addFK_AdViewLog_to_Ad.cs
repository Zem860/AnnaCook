namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFK_AdViewLog_to_Ad : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdViewLogs", "Advertisement_Id", c => c.Int());
            CreateIndex("dbo.AdViewLogs", "AdId");
            CreateIndex("dbo.AdViewLogs", "Advertisement_Id");
            AddForeignKey("dbo.AdViewLogs", "AdId", "dbo.Advertisements", "Id");
            AddForeignKey("dbo.AdViewLogs", "Advertisement_Id", "dbo.Advertisements", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdViewLogs", "Advertisement_Id", "dbo.Advertisements");
            DropForeignKey("dbo.AdViewLogs", "AdId", "dbo.Advertisements");
            DropIndex("dbo.AdViewLogs", new[] { "Advertisement_Id" });
            DropIndex("dbo.AdViewLogs", new[] { "AdId" });
            DropColumn("dbo.AdViewLogs", "Advertisement_Id");
        }
    }
}
