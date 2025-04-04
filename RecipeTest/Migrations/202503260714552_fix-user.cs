namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixuser : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Salt", c => c.Binary(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "PasswordHash", c => c.Binary(nullable: false, maxLength: 100));
        }
    }
}
