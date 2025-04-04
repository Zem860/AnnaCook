namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changecolumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recipes", "SharedCount", c => c.Int(nullable: false));
            DropColumn("dbo.Recipes", "LikedNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Recipes", "LikedNumber", c => c.Int(nullable: false));
            DropColumn("dbo.Recipes", "SharedCount");
        }
    }
}
