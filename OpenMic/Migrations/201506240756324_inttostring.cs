namespace OpenMic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inttostring : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Thoughts", new[] { "users_Id" });
            DropColumn("dbo.Thoughts", "ApplicationUserID");
            RenameColumn(table: "dbo.Thoughts", name: "users_Id", newName: "ApplicationUserID");
            AlterColumn("dbo.Thoughts", "ApplicationUserID", c => c.String(maxLength: 128));
            CreateIndex("dbo.Thoughts", "ApplicationUserID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Thoughts", new[] { "ApplicationUserID" });
            AlterColumn("dbo.Thoughts", "ApplicationUserID", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.Thoughts", name: "ApplicationUserID", newName: "users_Id");
            AddColumn("dbo.Thoughts", "ApplicationUserID", c => c.Int(nullable: false));
            CreateIndex("dbo.Thoughts", "users_Id");
        }
    }
}
