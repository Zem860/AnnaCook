namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class makeDesNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Steps", "StepDescription", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Steps", "StepDescription", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
