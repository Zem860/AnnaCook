namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class featAddUserTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountEmail = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.Binary(nullable: false, maxLength: 100),
                        Salt = c.Binary(nullable: false, maxLength: 100),
                        AccountName = c.String(nullable: false, maxLength: 50),
                        AccountProfilePhoto = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.AccountEmail, unique: true)
                .Index(t => t.AccountName, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", new[] { "AccountName" });
            DropIndex("dbo.Users", new[] { "AccountEmail" });
            DropTable("dbo.Users");
        }
    }
}
