namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userTableChanged : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            AddColumn("dbo.Users", "DisplayId", c => c.String(maxLength: 10));
            AddColumn("dbo.Users", "LoginProvider", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "UserRole", c => c.Int(nullable: false));
            AlterColumn("dbo.Users", "AccountEmail", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.Users", "AccountEmail", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            AlterColumn("dbo.Users", "Salt", c => c.String(maxLength: 100));
            AlterColumn("dbo.Users", "AccountEmail", c => c.String(maxLength: 100));
            DropColumn("dbo.Users", "UserRole");
            DropColumn("dbo.Users", "LoginProvider");
            DropColumn("dbo.Users", "DisplayId");
            CreateIndex("dbo.Users", "AccountEmail", unique: true);
        }
    }
}
