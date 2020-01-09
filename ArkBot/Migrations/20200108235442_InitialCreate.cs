using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArkBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TamedCreatureLogEntries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    LastSeen = table.Column<DateTime>(nullable: false),
                    RelatedLogEntries = table.Column<string>(nullable: true),
                    X = table.Column<decimal>(nullable: false),
                    Y = table.Column<decimal>(nullable: false),
                    Z = table.Column<decimal>(nullable: false),
                    Latitude = table.Column<decimal>(nullable: false),
                    Longitude = table.Column<decimal>(nullable: false),
                    Team = table.Column<int>(nullable: true),
                    PlayerId = table.Column<int>(nullable: true),
                    Female = table.Column<bool>(nullable: false),
                    TamedAtTime = table.Column<decimal>(nullable: true),
                    TamedTime = table.Column<decimal>(nullable: true),
                    Tribe = table.Column<string>(nullable: true),
                    Tamer = table.Column<string>(nullable: true),
                    OwnerName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    BaseLevel = table.Column<int>(nullable: false),
                    FullLevel = table.Column<int>(nullable: true),
                    Experience = table.Column<decimal>(nullable: true),
                    ApproxFoodPercentage = table.Column<double>(nullable: true),
                    ApproxHealthPercentage = table.Column<double>(nullable: true),
                    ImprintingQuality = table.Column<decimal>(nullable: true),
                    SpeciesClass = table.Column<string>(nullable: true),
                    IsConfirmedDead = table.Column<bool>(nullable: false),
                    IsInCluster = table.Column<bool>(nullable: false),
                    IsUnavailable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TamedCreatureLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<long>(nullable: false),
                    SteamId = table.Column<long>(nullable: false),
                    RealName = table.Column<string>(nullable: true),
                    SteamDisplayName = table.Column<string>(nullable: true),
                    DisallowVoting = table.Column<bool>(nullable: false),
                    Unlinked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WildCreatureLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WildCreatureLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Started = table.Column<DateTime>(nullable: false),
                    Finished = table.Column<DateTime>(nullable: false),
                    Result = table.Column<int>(nullable: false),
                    ServerKey = table.Column<string>(nullable: true),
                    Identifier = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    PlayerName = table.Column<string>(nullable: true),
                    CharacterName = table.Column<string>(nullable: true),
                    TribeName = table.Column<string>(nullable: true),
                    SteamId = table.Column<long>(nullable: true),
                    BannedUntil = table.Column<DateTime>(nullable: true),
                    DurationInHours = table.Column<int>(nullable: true),
                    TimeOfDay = table.Column<string>(nullable: true),
                    UnbanVote_PlayerName = table.Column<string>(nullable: true),
                    UnbanVote_CharacterName = table.Column<string>(nullable: true),
                    UnbanVote_TribeName = table.Column<string>(nullable: true),
                    UnbanVote_SteamId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Played",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(nullable: false),
                    TimeInSeconds = table.Column<long>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    SteamId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Played", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Played_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WildCreatureLogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(nullable: true),
                    Count = table.Column<int>(nullable: false),
                    LogId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WildCreatureLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WildCreatureLogEntries_WildCreatureLogs_LogId",
                        column: x => x.LogId,
                        principalTable: "WildCreatureLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVotes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    VoteId = table.Column<int>(nullable: false),
                    VoteType = table.Column<int>(nullable: false),
                    InitiatedVote = table.Column<bool>(nullable: false),
                    VotedFor = table.Column<bool>(nullable: false),
                    Vetoed = table.Column<bool>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVotes_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Played_UserId",
                table: "Played",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserId",
                table: "UserVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_VoteId",
                table: "UserVotes",
                column: "VoteId");

            migrationBuilder.CreateIndex(
                name: "IX_WildCreatureLogEntries_LogId",
                table: "WildCreatureLogEntries",
                column: "LogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Played");

            migrationBuilder.DropTable(
                name: "TamedCreatureLogEntries");

            migrationBuilder.DropTable(
                name: "UserVotes");

            migrationBuilder.DropTable(
                name: "WildCreatureLogEntries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "WildCreatureLogs");
        }
    }
}
