using Microsoft.EntityFrameworkCore.Migrations;

namespace MakingFuss.Migrations
{
    public partial class RemoveRatioAndMergeNamesAndAddDefaultValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Contesters",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE Contesters
                SET Name = FirstName + ' ' + LastName;");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Contesters");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Contesters");

            migrationBuilder.DropColumn(
                name: "Ratio",
                table: "Contesters");

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Contesters");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Contesters",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Contesters",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Ratio",
                table: "Contesters",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
