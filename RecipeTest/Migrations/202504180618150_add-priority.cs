namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addpriority : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advertisements", "Priority", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advertisements", "Priority");
        }
    }
}
