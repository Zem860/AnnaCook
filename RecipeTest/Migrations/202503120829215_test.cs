namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ingredients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeId = c.Int(nullable: false),
                        IngredientName = c.String(nullable: false, maxLength: 20),
                        IsFlavoring = c.Boolean(nullable: false),
                        Amount = c.Int(nullable: false),
                        Unit = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .Index(t => t.RecipeId);
            
            CreateTable(
                "dbo.Recipes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeName = c.String(nullable: false, maxLength: 20),
                        IsPublished = c.Boolean(nullable: false),
                        RecipeIntro = c.String(nullable: false, maxLength: 500),
                        CookingTime = c.Decimal(nullable: false, precision: 10, scale: 2),
                        Portion = c.Decimal(nullable: false, precision: 10, scale: 2),
                        LikedNumber = c.Int(nullable: false),
                        Rating = c.Decimal(nullable: false, precision: 10, scale: 2),
                        RecipeVideoLink = c.String(nullable: false, maxLength: 4000),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RecipePhotos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeId = c.Int(nullable: false),
                        ImgUrl = c.String(nullable: false),
                        IsCover = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .Index(t => t.RecipeId);
            
            CreateTable(
                "dbo.Steps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeId = c.Int(nullable: false),
                        StepOrder = c.Int(nullable: false),
                        StepDescription = c.String(nullable: false, maxLength: 100),
                        VideoStart = c.Int(nullable: false),
                        VideoEnd = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .Index(t => t.RecipeId);
            
            CreateTable(
                "dbo.StepPhotos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StepId = c.Int(nullable: false),
                        IsCover = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Steps", t => t.StepId, cascadeDelete: true)
                .Index(t => t.StepId);
            
            CreateTable(
                "dbo.SubSteps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StepId = c.Int(nullable: false),
                        StepContent = c.String(nullable: false, maxLength: 200),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Steps", t => t.StepId, cascadeDelete: true)
                .Index(t => t.StepId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubSteps", "StepId", "dbo.Steps");
            DropForeignKey("dbo.StepPhotos", "StepId", "dbo.Steps");
            DropForeignKey("dbo.Steps", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.RecipePhotos", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Ingredients", "RecipeId", "dbo.Recipes");
            DropIndex("dbo.SubSteps", new[] { "StepId" });
            DropIndex("dbo.StepPhotos", new[] { "StepId" });
            DropIndex("dbo.Steps", new[] { "RecipeId" });
            DropIndex("dbo.RecipePhotos", new[] { "RecipeId" });
            DropIndex("dbo.Ingredients", new[] { "RecipeId" });
            DropTable("dbo.SubSteps");
            DropTable("dbo.StepPhotos");
            DropTable("dbo.Steps");
            DropTable("dbo.RecipePhotos");
            DropTable("dbo.Recipes");
            DropTable("dbo.Ingredients");
        }
    }
}
