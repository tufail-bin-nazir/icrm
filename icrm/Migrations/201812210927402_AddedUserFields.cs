namespace icrm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LocationId", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "SubLocationId", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "PositionId", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "NationalityId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "NationalityId");
            DropColumn("dbo.AspNetUsers", "PositionId");
            DropColumn("dbo.AspNetUsers", "SubLocationId");
            DropColumn("dbo.AspNetUsers", "LocationId");
        }
    }
}
