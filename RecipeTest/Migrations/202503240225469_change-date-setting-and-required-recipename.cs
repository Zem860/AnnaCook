namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changedatesettingandrequiredrecipename : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Recipes", "RecipeName", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.SubSteps", "StepContent", c => c.String(nullable: false, maxLength: 45));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SubSteps", "StepContent", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Recipes", "RecipeName", c => c.String(maxLength: 20));
        }
    }
}
