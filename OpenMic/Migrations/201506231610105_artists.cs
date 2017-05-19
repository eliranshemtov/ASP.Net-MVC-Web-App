namespace OpenMic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class artists : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Tracks", name: "Artists_Id", newName: "ArtistsID");
            RenameIndex(table: "dbo.Tracks", name: "IX_Artists_Id", newName: "IX_ArtistsID");
            DropColumn("dbo.Tracks", "ApplicationUserID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tracks", "ApplicationUserID", c => c.String());
            RenameIndex(table: "dbo.Tracks", name: "IX_ArtistsID", newName: "IX_Artists_Id");
            RenameColumn(table: "dbo.Tracks", name: "ArtistsID", newName: "Artists_Id");
        }
    }
}
