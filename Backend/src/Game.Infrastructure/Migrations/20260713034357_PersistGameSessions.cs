using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistGameSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPlayerIndex = table.Column<int>(type: "integer", nullable: false),
                    CurrentQuestionIndex = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    QuestionOrderJson = table.Column<string>(type: "jsonb", nullable: false),
                    WinnerPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "game_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionIndex = table.Column<int>(type: "integer", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_answers_game_sessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "game_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerIndex = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Socks = table.Column<bool>(type: "boolean", nullable: false),
                    Shirt = table.Column<bool>(type: "boolean", nullable: false),
                    Pants = table.Column<bool>(type: "boolean", nullable: false),
                    Underwear = table.Column<bool>(type: "boolean", nullable: false),
                    ClothingLostAtScoresJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_players_game_sessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "game_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rewards_game_sessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "game_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_answers_GameSessionId",
                table: "game_answers",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_game_players_GameSessionId_PlayerIndex",
                table: "game_players",
                columns: new[] { "GameSessionId", "PlayerIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_JoinCode",
                table: "game_sessions",
                column: "JoinCode",
                unique: true,
                filter: "\"JoinCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_GameSessionId",
                table: "rewards",
                column: "GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_answers");

            migrationBuilder.DropTable(
                name: "game_players");

            migrationBuilder.DropTable(
                name: "rewards");

            migrationBuilder.DropTable(
                name: "game_sessions");
        }
    }
}
