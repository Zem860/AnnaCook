namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rebuildAnnaCook : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdImgs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ImgUrl = c.String(nullable: false),
                        AdId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Advertisements", t => t.AdId, cascadeDelete: true)
                .Index(t => t.AdId);
            
            CreateTable(
                "dbo.Advertisements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdName = c.String(nullable: false, maxLength: 100),
                        AdDisplayPage = c.Int(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        LinkUrl = c.String(nullable: false),
                        AdPrice = c.Decimal(precision: 18, scale: 2),
                        Currency = c.String(nullable: false, maxLength: 20),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AdTags",
                c => new
                    {
                        AdId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.AdId, t.TagId })
                .ForeignKey("dbo.Advertisements", t => t.AdId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.AdId)
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
                "dbo.Recipes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        DisplayId = c.String(nullable: false, maxLength: 10),
                        ViewCount = c.Int(nullable: false),
                        RecipeName = c.String(nullable: false, maxLength: 24),
                        IsPublished = c.Boolean(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        RecipeIntro = c.String(maxLength: 500),
                        CookingTime = c.Decimal(nullable: false, precision: 10, scale: 2),
                        Portion = c.Decimal(nullable: false, precision: 10, scale: 2),
                        SharedCount = c.Int(nullable: false),
                        Rating = c.Decimal(nullable: false, precision: 10, scale: 2),
                        RecipeVideoLink = c.String(maxLength: 4000),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.DisplayId, unique: true);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RecipeId = c.Int(nullable: false),
                        CommentContent = c.String(nullable: false, maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recipes", t => t.RecipeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RecipeId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayId = c.String(nullable: false, maxLength: 10),
                        AccountEmail = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.String(nullable: false, maxLength: 100),
                        Salt = c.String(nullable: false, maxLength: 100),
                        AccountName = c.String(nullable: false, maxLength: 50),
                        AccountProfilePhoto = c.String(maxLength: 500),
                        UserIntro = c.String(maxLength: 1000),
                        IsVerified = c.Boolean(nullable: false),
                        IsBanned = c.Boolean(nullable: false),
                        IsUploadable = c.Boolean(nullable: false),
                        IsCommentable = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        LoginProvider = c.Int(nullable: false),
                        UserRole = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.DisplayId, unique: true)
                .Index(t => t.AccountEmail, unique: true)
                .Index(t => t.AccountName, unique: true);
            
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
            
            CreateTable(
                "dbo.Ingredients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeId = c.Int(nullable: false),
                        IngredientName = c.String(nullable: false, maxLength: 20),
                        IsFlavoring = c.Boolean(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                        StepDescription = c.String(maxLength: 100),
                        VideoStart = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VideoEnd = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                        StepContent = c.String(nullable: false, maxLength: 45),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Steps", t => t.StepId, cascadeDelete: true)
                .Index(t => t.StepId);
            
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
            
            CreateTable(
                "dbo.LoginRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        IpAddress = c.String(maxLength: 45),
                        LoginAction = c.Int(nullable: false),
                        ActionTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.RecipeTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.RecipeTags", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Recipes", "UserId", "dbo.Users");
            DropForeignKey("dbo.StepPhotos", "Recipes_Id", "dbo.Recipes");
            DropForeignKey("dbo.SubSteps", "StepId", "dbo.Steps");
            DropForeignKey("dbo.StepPhotos", "StepId", "dbo.Steps");
            DropForeignKey("dbo.Steps", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.RecipePhotos", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Ingredients", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Ratings", "UserId", "dbo.Users");
            DropForeignKey("dbo.Ratings", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Follows", "UserId", "dbo.Users");
            DropForeignKey("dbo.Follows", "FollowedUserId", "dbo.Users");
            DropForeignKey("dbo.Favorites", "UserId", "dbo.Users");
            DropForeignKey("dbo.Favorites", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "RecipeId", "dbo.Recipes");
            DropForeignKey("dbo.AdTags", "AdId", "dbo.Advertisements");
            DropForeignKey("dbo.AdImgs", "AdId", "dbo.Advertisements");
            DropIndex("dbo.SubSteps", new[] { "StepId" });
            DropIndex("dbo.Steps", new[] { "RecipeId" });
            DropIndex("dbo.StepPhotos", new[] { "Recipes_Id" });
            DropIndex("dbo.StepPhotos", new[] { "StepId" });
            DropIndex("dbo.RecipePhotos", new[] { "RecipeId" });
            DropIndex("dbo.Ingredients", new[] { "RecipeId" });
            DropIndex("dbo.Ratings", new[] { "RecipeId" });
            DropIndex("dbo.Ratings", new[] { "UserId" });
            DropIndex("dbo.Follows", new[] { "FollowedUserId" });
            DropIndex("dbo.Follows", new[] { "UserId" });
            DropIndex("dbo.Favorites", new[] { "RecipeId" });
            DropIndex("dbo.Favorites", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "AccountName" });
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            DropIndex("dbo.Users", new[] { "DisplayId" });
            DropIndex("dbo.Comments", new[] { "RecipeId" });
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Recipes", new[] { "DisplayId" });
            DropIndex("dbo.Recipes", new[] { "UserId" });
            DropIndex("dbo.RecipeTags", new[] { "TagId" });
            DropIndex("dbo.RecipeTags", new[] { "RecipeId" });
            DropIndex("dbo.AdTags", new[] { "TagId" });
            DropIndex("dbo.AdTags", new[] { "AdId" });
            DropIndex("dbo.AdImgs", new[] { "AdId" });
            DropTable("dbo.LoginRecords");
            DropTable("dbo.AdMonthlyPerformances");
            DropTable("dbo.SubSteps");
            DropTable("dbo.Steps");
            DropTable("dbo.StepPhotos");
            DropTable("dbo.RecipePhotos");
            DropTable("dbo.Ingredients");
            DropTable("dbo.Ratings");
            DropTable("dbo.Follows");
            DropTable("dbo.Favorites");
            DropTable("dbo.Users");
            DropTable("dbo.Comments");
            DropTable("dbo.Recipes");
            DropTable("dbo.RecipeTags");
            DropTable("dbo.Tags");
            DropTable("dbo.AdTags");
            DropTable("dbo.Advertisements");
            DropTable("dbo.AdImgs");
        }
    }
}
