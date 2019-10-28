using Microsoft.EntityFrameworkCore.Migrations;

namespace vscodecore.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    TournamentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    WinnerContesterId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.TournamentId);
                });

            migrationBuilder.CreateTable(
                name: "Contesters",
                columns: table => new
                {
                    ContesterId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    Score = table.Column<int>(nullable: false),
                    GamesPlayed = table.Column<int>(nullable: false),
                    TournamentWon = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    TournamentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contesters", x => x.ContesterId);
                    table.ForeignKey(
                        name: "FK_Contesters_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
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
                name: "FK_Tournaments_Contesters_WinnerContesterId",
                table: "Tournaments",
                column: "WinnerContesterId",
                principalTable: "Contesters",
                principalColumn: "ContesterId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contesters_Tournaments_TournamentId",
                table: "Contesters");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropTable(
                name: "Contesters");
        }
    }
}
