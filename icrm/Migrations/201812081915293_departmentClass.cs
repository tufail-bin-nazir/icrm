namespace icrm.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class departmentClass : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.AspNetUsers", "DepartmentID", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "DepartmentID");
            AddForeignKey("dbo.AspNetUsers", "DepartmentID", "dbo.Departments", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "DepartmentID", "dbo.Departments");
            DropIndex("dbo.AspNetUsers", new[] { "DepartmentID" });
            DropColumn("dbo.AspNetUsers", "DepartmentID");
            DropTable("dbo.Departments");
        }
    }
}
