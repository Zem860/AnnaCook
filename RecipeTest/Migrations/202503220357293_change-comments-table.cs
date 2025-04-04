namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changecommentstable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "RecipeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Comments", "RecipeId");
            AddForeignKey("dbo.Comments", "RecipeId", "dbo.Recipes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "RecipeId", "dbo.Recipes");
            DropIndex("dbo.Comments", new[] { "RecipeId" });
            DropColumn("dbo.Comments", "RecipeId");
        }
    }
}
