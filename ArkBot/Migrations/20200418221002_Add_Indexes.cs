using Microsoft.EntityFrameworkCore.Migrations;

namespace ArkBot.Migrations
{
    public partial class Add_Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastServerKey",
                table: "Players",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServerKey",
                table: "ChatMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_IsOnline_LastServerKey",
                table: "Players",
                columns: new[] { "IsOnline", "LastServerKey" });

            migrationBuilder.CreateIndex(
                name: "IX_LoggedLocations_At",
                table: "LoggedLocations",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ServerKey_At",
                table: "ChatMessages",
                columns: new[] { "ServerKey", "At" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_IsOnline_LastServerKey",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_LoggedLocations_At",
                table: "LoggedLocations");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ServerKey_At",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "LastServerKey",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServerKey",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
