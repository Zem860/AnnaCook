namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removesaltpwd : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            AlterColumn("dbo.Users", "AccountEmail", c => c.String(maxLength: 100));
            AlterColumn("dbo.Users", "Salt", c => c.String(maxLength: 100));
            CreateIndex("dbo.Users", "AccountEmail", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            AlterColumn("dbo.Users", "Salt", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "AccountEmail", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.Users", "AccountEmail", unique: true);
        }
    }
}
