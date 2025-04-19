namespace RecipeTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addadvertiserNametoads : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advertisements", "AdvertiserName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advertisements", "AdvertiserName");
        }
    }
}
