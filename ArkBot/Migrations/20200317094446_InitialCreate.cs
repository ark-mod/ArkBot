using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ArkBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    LastServerKey = table.Column<string>(nullable: true),
                    LastLogin = table.Column<DateTime>(nullable: false),
                    LastActive = table.Column<DateTime>(nullable: false),
                    IsOnline = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    At = table.Column<DateTime>(nullable: false),
                    ServerKey = table.Column<string>(nullable: true),
                    SteamId = table.Column<decimal>(nullable: false),
                    PlayerName = table.Column<string>(nullable: true),
                    CharacterName = table.Column<string>(nullable: true),
                    TribeName = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Mode = table.Column<int>(nullable: false),
                    Icon = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Players_SteamId",
                        column: x => x.SteamId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoggedLocations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SteamId = table.Column<decimal>(nullable: false),
                    At = table.Column<DateTime>(nullable: false),
                    ServerKey = table.Column<string>(nullable: true),
                    X = table.Column<float>(nullable: false),
                    Y = table.Column<float>(nullable: false),
                    Z = table.Column<float>(nullable: false),
                    Latitude = table.Column<float>(nullable: false),
                    Longitude = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoggedLocations_Players_SteamId",
                        column: x => x.SteamId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SteamId",
                table: "ChatMessages",
                column: "SteamId");

            migrationBuilder.CreateIndex(
                name: "IX_LoggedLocations_SteamId",
                table: "LoggedLocations",
                column: "SteamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "LoggedLocations");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
