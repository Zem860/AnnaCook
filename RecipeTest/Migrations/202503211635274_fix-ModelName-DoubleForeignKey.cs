namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixModelNameDoubleForeignKey : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Favorites",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RecipeId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RecipeId })
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RecipeId);
            
            CreateTable(
                "dbo.Recipes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RecipeName = c.String(maxLength: 20),
                        IsPublished = c.Boolean(nullable: false),
                        RecipeIntro = c.String(maxLength: 500),
                        CookingTime = c.Decimal(nullable: false, precision: 10, scale: 2),
                        Portion = c.Decimal(nullable: false, precision: 10, scale: 2),
                        LikedNumber = c.Int(nullable: false),
                        Rating = c.Decimal(nullable: false, precision: 10, scale: 2),
                        RecipeVideoLink = c.String(maxLength: 4000),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
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
                "dbo.StepPhotos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StepId = c.Int(nullable: false),
                        IsCover = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        Recipes_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Steps", t => t.StepId, cascadeDelete: true)
                .ForeignKey("dbo.Recipes", t => t.Recipes_Id)
                .Index(t => t.StepId)
                .Index(t => t.Recipes_Id);
            
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
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountEmail = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.Binary(nullable: false, maxLength: 100),
                        Salt = c.Binary(nullable: false, maxLength: 100),
                        AccountName = c.String(nullable: false, maxLength: 50),
                        AccountProfilePhoto = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.AccountEmail, unique: true)
                .Index(t => t.AccountName, unique: true);
            
            CreateTable(
                "dbo.Follows",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FollowedUserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.FollowedUserId })
                .ForeignKey("dbo.Users", t => t.FollowedUserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.FollowedUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorites", "UserId", "dbo.Users");
            DropForeignKey("dbo.Favorites", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Recipes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Follows", "UserId", "dbo.Users");
            DropForeignKey("dbo.Follows", "FollowedUserId", "dbo.Users");
            DropForeignKey("dbo.StepPhotos", "Recipes_Id", "dbo.Recipes");
            DropForeignKey("dbo.SubSteps", "StepId", "dbo.Steps");
            DropForeignKey("dbo.StepPhotos", "StepId", "dbo.Steps");
            DropForeignKey("dbo.Steps", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.RecipePhotos", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Ingredients", "RecipeId", "dbo.Recipes");
            DropIndex("dbo.Follows", new[] { "FollowedUserId" });
            DropIndex("dbo.Follows", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "AccountName" });
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            DropIndex("dbo.SubSteps", new[] { "StepId" });
            DropIndex("dbo.Steps", new[] { "RecipeId" });
            DropIndex("dbo.StepPhotos", new[] { "Recipes_Id" });
            DropIndex("dbo.StepPhotos", new[] { "StepId" });
            DropIndex("dbo.RecipePhotos", new[] { "RecipeId" });
            DropIndex("dbo.Ingredients", new[] { "RecipeId" });
            DropIndex("dbo.Recipes", new[] { "UserId" });
            DropIndex("dbo.Favorites", new[] { "RecipeId" });
            DropIndex("dbo.Favorites", new[] { "UserId" });
            DropTable("dbo.Follows");
            DropTable("dbo.Users");
            DropTable("dbo.SubSteps");
            DropTable("dbo.Steps");
            DropTable("dbo.StepPhotos");
            DropTable("dbo.RecipePhotos");
            DropTable("dbo.Ingredients");
            DropTable("dbo.Recipes");
            DropTable("dbo.Favorites");
        }
    }
}
