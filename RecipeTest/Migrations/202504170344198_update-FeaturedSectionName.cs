namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateFeaturedSectionName : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FeaturedSecions", newName: "FeaturedSections");
            AddColumn("dbo.FeaturedSections", "SectionPos", c => c.Int(nullable: false));
            AlterColumn("dbo.FeaturedSections", "FeaturedSectionName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FeaturedSections", "FeaturedSectionName", c => c.Int(nullable: false));
            DropColumn("dbo.FeaturedSections", "SectionPos");
            RenameTable(name: "dbo.FeaturedSections", newName: "FeaturedSecions");
        }
    }
}
