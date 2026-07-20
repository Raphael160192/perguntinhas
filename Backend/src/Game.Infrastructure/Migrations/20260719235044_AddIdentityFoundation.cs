using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessChannel",
                table: "game_sessions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "anonymous");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "game_sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "game_players",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "game_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoogleSubject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    AuthProvider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_consents_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_UserId",
                table: "game_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_game_players_UserId",
                table: "game_players",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_game_events_UserId",
                table: "game_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_consents_UserId",
                table: "user_consents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_GoogleSubject",
                table: "users",
                column: "GoogleSubject",
                unique: true,
                filter: "\"GoogleSubject\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_game_events_users_UserId",
                table: "game_events",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_game_players_users_UserId",
                table: "game_players",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_game_sessions_users_UserId",
                table: "game_sessions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_events_users_UserId",
                table: "game_events");

            migrationBuilder.DropForeignKey(
                name: "FK_game_players_users_UserId",
                table: "game_players");

            migrationBuilder.DropForeignKey(
                name: "FK_game_sessions_users_UserId",
                table: "game_sessions");

            migrationBuilder.DropTable(
                name: "user_consents");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_game_sessions_UserId",
                table: "game_sessions");

            migrationBuilder.DropIndex(
                name: "IX_game_players_UserId",
                table: "game_players");

            migrationBuilder.DropIndex(
                name: "IX_game_events_UserId",
                table: "game_events");

            migrationBuilder.DropColumn(
                name: "AccessChannel",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "game_players");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "game_events");
        }
    }
}
