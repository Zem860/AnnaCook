namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addratingtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RecipeId = c.Int(nullable: false),
                        Rating = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RecipeId })
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RecipeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ratings", "UserId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "RecipeId", "dbo.Recipes");
            DropIndex("dbo.Ratings", new[] { "RecipeId" });
            DropIndex("dbo.Ratings", new[] { "UserId" });
            DropTable("dbo.Ratings");
        }
    }
}
