namespace icrm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedback1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false),
                        contactNo = c.String(nullable: false),
                        email = c.String(nullable: false),
                        typeOfFeedback = c.String(nullable: false),
                        subject = c.String(nullable: false),
                        details = c.String(nullable: false),
                        userId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AspNetUsers", t => t.userId)
                .Index(t => t.userId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Feedbacks", "userId", "dbo.AspNetUsers");
            DropIndex("dbo.Feedbacks", new[] { "userId" });
            DropTable("dbo.Feedbacks");
        }
    }
}
