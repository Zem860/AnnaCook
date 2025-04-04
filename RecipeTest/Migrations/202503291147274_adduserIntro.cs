namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adduserIntro : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "UserIntro", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "UserIntro");
        }
    }
}
