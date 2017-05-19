namespace OpenMic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nextShow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "nextShow", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "nextShow");
        }
    }
}
