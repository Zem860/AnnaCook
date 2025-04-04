namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRecipeDisplayIdViewCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recipes", "DisplayId", c => c.String(nullable: false, maxLength: 10));
            AddColumn("dbo.Recipes", "ViewCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Recipes", "ViewCount");
            DropColumn("dbo.Recipes", "DisplayId");
        }
    }
}
