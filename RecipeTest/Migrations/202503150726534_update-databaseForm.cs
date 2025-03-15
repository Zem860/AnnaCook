namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedatabaseForm : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Recipes", "RecipeIntro", c => c.String(maxLength: 500));
            AlterColumn("dbo.Recipes", "RecipeVideoLink", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Recipes", "RecipeVideoLink", c => c.String(nullable: false, maxLength: 4000));
            AlterColumn("dbo.Recipes", "RecipeIntro", c => c.String(nullable: false, maxLength: 500));
        }
    }
}
