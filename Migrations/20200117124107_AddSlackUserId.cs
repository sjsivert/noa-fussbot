using Microsoft.EntityFrameworkCore.Migrations;

namespace MakingFuss.Migrations
{
    public partial class AddSlackUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlackUserId",
                table: "Contesters",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlackUserId",
                table: "Contesters");
        }
    }
}
