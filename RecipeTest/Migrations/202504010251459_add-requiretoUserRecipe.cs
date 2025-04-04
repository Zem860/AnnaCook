namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrequiretoUserRecipe : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "DisplayId", c => c.String(nullable: false, maxLength: 10));
            CreateIndex("dbo.Recipes", "DisplayId", unique: true);
            CreateIndex("dbo.Users", "DisplayId", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", new[] { "DisplayId" });
            DropIndex("dbo.Recipes", new[] { "DisplayId" });
            AlterColumn("dbo.Users", "DisplayId", c => c.String(maxLength: 10));
        }
    }
}
