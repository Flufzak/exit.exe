using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Exit.exe.Repository.Migrations.App
{
    /// <inheritdoc />
    public partial class AddPuzzlesAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Puzzles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puzzles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PuzzleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GuessedLetters = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AttemptsLeft = table.Column<int>(type: "int", nullable: false),
                    HintsUsed = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Puzzles_PuzzleId",
                        column: x => x.PuzzleId,
                        principalTable: "Puzzles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Puzzles",
                columns: new[] { "Id", "CreatedAtUtc", "GameType", "Payload" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hangman", "{\"word\":\"KAZIMIR\",\"description\":\"The name of the ancient sect that imprisoned you\",\"category\":\"Names\",\"maxAttempts\":6}" },
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hangman", "{\"word\":\"SACRIFICE\",\"description\":\"The ritual demands this from the chosen one\",\"category\":\"Concepts\",\"maxAttempts\":6}" },
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hangman", "{\"word\":\"ASCENSION\",\"description\":\"What the priests believe awaits beyond death\",\"category\":\"Concepts\",\"maxAttempts\":6}" },
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hangman", "{\"word\":\"OFFERING\",\"description\":\"You are this to the ancient order\",\"category\":\"Concepts\",\"maxAttempts\":6}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_PuzzleId",
                table: "GameSessions",
                column: "PuzzleId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_UserId",
                table: "GameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_GameType",
                table: "Puzzles",
                column: "GameType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Puzzles");
        }
    }
}
