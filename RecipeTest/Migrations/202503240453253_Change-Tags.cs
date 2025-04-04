namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeTags : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Ingredients", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Ingredients", "Amount", c => c.Int(nullable: false));
        }
    }
}
