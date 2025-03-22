namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrecipeTag : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RecipeTags",
                c => new
                    {
                        RecipeId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.RecipeId, t.TagId })
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId)
                .Index(t => t.RecipeId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagName = c.String(nullable: false, maxLength: 10),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RecipeTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.RecipeTags", "RecipeId", "dbo.Recipes");
            DropIndex("dbo.RecipeTags", new[] { "TagId" });
            DropIndex("dbo.RecipeTags", new[] { "RecipeId" });
            DropTable("dbo.Tags");
            DropTable("dbo.RecipeTags");
        }
    }
}
