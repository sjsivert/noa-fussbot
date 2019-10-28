using Microsoft.EntityFrameworkCore.Migrations;

namespace vscodecore.Migrations
{
    public partial class lastUpdatedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastUpdated",
                table: "Contesters",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Contesters");
        }
    }
}
