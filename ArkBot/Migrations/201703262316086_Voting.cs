namespace ArkBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Voting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserVotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        VoteId = c.Int(nullable: false),
                        VoteType = c.Int(nullable: false),
                        InitiatedVote = c.Boolean(nullable: false),
                        VotedFor = c.Boolean(nullable: false),
                        Vetoed = c.Boolean(nullable: false),
                        Reason = c.String(maxLength: 4000),
                        When = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Votes", t => t.VoteId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.VoteId);
            
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Started = c.DateTime(nullable: false),
                        Finished = c.DateTime(nullable: false),
                        Result = c.Int(nullable: false),
                        Identifier = c.String(maxLength: 4000),
                        Reason = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BanVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PlayerName = c.String(maxLength: 4000),
                        CharacterName = c.String(maxLength: 4000),
                        TribeName = c.String(maxLength: 4000),
                        SteamId = c.Long(nullable: false),
                        BannedUntil = c.DateTime(),
                        DurationInHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.UnbanVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PlayerName = c.String(maxLength: 4000),
                        CharacterName = c.String(maxLength: 4000),
                        TribeName = c.String(maxLength: 4000),
                        SteamId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SetTimeOfDayVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TimeOfDay = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.DestroyWildDinosVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.UpdateServerVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.RestartServerVotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Votes", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.Users", "DisallowVoting", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "Unlinked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RestartServerVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.UpdateServerVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.DestroyWildDinosVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.SetTimeOfDayVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.UnbanVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.BanVotes", "Id", "dbo.Votes");
            DropForeignKey("dbo.UserVotes", "VoteId", "dbo.Votes");
            DropForeignKey("dbo.UserVotes", "UserId", "dbo.Users");
            DropIndex("dbo.RestartServerVotes", new[] { "Id" });
            DropIndex("dbo.UpdateServerVotes", new[] { "Id" });
            DropIndex("dbo.DestroyWildDinosVotes", new[] { "Id" });
            DropIndex("dbo.SetTimeOfDayVotes", new[] { "Id" });
            DropIndex("dbo.UnbanVotes", new[] { "Id" });
            DropIndex("dbo.BanVotes", new[] { "Id" });
            DropIndex("dbo.UserVotes", new[] { "VoteId" });
            DropIndex("dbo.UserVotes", new[] { "UserId" });
            DropColumn("dbo.Users", "Unlinked");
            DropColumn("dbo.Users", "DisallowVoting");
            DropTable("dbo.RestartServerVotes");
            DropTable("dbo.UpdateServerVotes");
            DropTable("dbo.DestroyWildDinosVotes");
            DropTable("dbo.SetTimeOfDayVotes");
            DropTable("dbo.UnbanVotes");
            DropTable("dbo.BanVotes");
            DropTable("dbo.Votes");
            DropTable("dbo.UserVotes");
        }
    }
}
