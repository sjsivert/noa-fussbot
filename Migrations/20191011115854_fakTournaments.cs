using Microsoft.EntityFrameworkCore.Migrations;

namespace MakingFuss.Migrations
{
    public partial class fakTournaments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contesters_Tournaments_TournamentId",
                table: "Contesters");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Contesters_TournamentId",
                table: "Contesters");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Contesters");

            migrationBuilder.DropColumn(
                name: "TournamentWon",
                table: "Contesters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Contesters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentWon",
                table: "Contesters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    TournamentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WinnerContesterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.TournamentId);
                    table.ForeignKey(
                        name: "FK_Tournaments_Contesters_WinnerContesterId",
                        column: x => x.WinnerContesterId,
                        principalTable: "Contesters",
                        principalColumn: "ContesterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contesters_TournamentId",
                table: "Contesters",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_WinnerContesterId",
                table: "Tournaments",
                column: "WinnerContesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contesters_Tournaments_TournamentId",
                table: "Contesters",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
