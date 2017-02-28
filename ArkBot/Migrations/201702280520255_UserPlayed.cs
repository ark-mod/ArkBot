namespace ArkBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPlayed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlayedEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        TimeInSeconds = c.Long(nullable: false),
                        UserId = c.Int(),
                        SteamId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlayedEntries", "UserId", "dbo.Users");
            DropIndex("dbo.PlayedEntries", new[] { "UserId" });
            DropTable("dbo.PlayedEntries");
        }
    }
}
