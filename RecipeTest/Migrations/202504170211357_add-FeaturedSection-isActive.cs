namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFeaturedSectionisActive : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FeaturedSecions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FeaturedSectionName = c.Int(nullable: false),
                        FeaturedSectionTags = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FeaturedSecions");
        }
    }
}
