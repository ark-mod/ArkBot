namespace ArkBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TamedCreatureLogEntries",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        LastSeen = c.DateTime(nullable: false),
                        RelatedLogEntries = c.String(maxLength: 4000),
                        X = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Y = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Z = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Latitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Longitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Team = c.Int(),
                        PlayerId = c.Int(),
                        Female = c.Boolean(nullable: false),
                        TamedAtTime = c.Decimal(precision: 18, scale: 2),
                        TamedTime = c.Decimal(precision: 18, scale: 2),
                        Tribe = c.String(maxLength: 4000),
                        Tamer = c.String(maxLength: 4000),
                        OwnerName = c.String(maxLength: 4000),
                        Name = c.String(maxLength: 4000),
                        BaseLevel = c.Int(nullable: false),
                        FullLevel = c.Int(),
                        Experience = c.Decimal(precision: 18, scale: 2),
                        ApproxFoodPercentage = c.Double(),
                        ImprintingQuality = c.Decimal(precision: 18, scale: 2),
                        SpeciesClass = c.String(maxLength: 4000),
                        IsConfirmedDead = c.Boolean(nullable: false),
                        IsInCluster = c.Boolean(nullable: false),
                        IsUnavailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                    .Index(t => t.Name, unique: false)
                    .Index(t => t.Team, unique: false)
                    .Index(t => t.PlayerId, unique: false)
                    .Index(t => t.IsUnavailable, unique: false);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DiscordId = c.Long(nullable: false),
                        SteamId = c.Long(nullable: false),
                        RealName = c.String(maxLength: 4000),
                        SteamDisplayName = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.WildCreatureLogEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(maxLength: 4000),
                        Count = c.Int(nullable: false),
                        Ids = c.String(),
                        LogId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WildCreatureLogs", t => t.LogId, cascadeDelete: true)
                .Index(t => t.LogId);
            
            CreateTable(
                "dbo.WildCreatureLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        When = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WildCreatureLogEntries", "LogId", "dbo.WildCreatureLogs");
            DropIndex("dbo.WildCreatureLogEntries", new[] { "LogId" });
            DropTable("dbo.WildCreatureLogs");
            DropTable("dbo.WildCreatureLogEntries");
            DropTable("dbo.Users");
            DropTable("dbo.TamedCreatureLogEntries");
        }
    }
}
