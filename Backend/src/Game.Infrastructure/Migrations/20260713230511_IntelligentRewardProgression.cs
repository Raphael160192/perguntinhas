using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntelligentRewardProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActorPlayerId",
                table: "rewards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogVersion",
                table: "rewards",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionType",
                table: "rewards",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionValue",
                table: "rewards",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntensityLevel",
                table: "rewards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverPlayerId",
                table: "rewards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "rewards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateId",
                table: "rewards",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnsweredRoundNumber",
                table: "game_sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingRoundResultJson",
                table: "game_sessions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RewardProgressionJson",
                table: "game_sessions",
                type: "jsonb",
                nullable: false,
                defaultValue: "{\"currentLevel\":1,\"rewardsGeneratedInCurrentStage\":0,\"recentRewards\":[]}");

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "game_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "game_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Partidas normais não percorrem todo o catálogo de 60 perguntas;
            // portanto o índice atual permite recuperar a rodada já alcançada.
            migrationBuilder.Sql(
                "UPDATE game_sessions SET \"RoundNumber\" = \"CurrentQuestionIndex\" + 1;");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_GameSessionId_RoundNumber",
                table: "rewards",
                columns: new[] { "GameSessionId", "RoundNumber" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_rewards_IntensityLevel",
                table: "rewards",
                sql: "\"IntensityLevel\" IS NULL OR (\"IntensityLevel\" BETWEEN 1 AND 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_rewards_RoundNumber",
                table: "rewards",
                sql: "\"RoundNumber\" IS NULL OR \"RoundNumber\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_game_sessions_RoundNumber",
                table: "game_sessions",
                sql: "\"RoundNumber\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_game_sessions_Version",
                table: "game_sessions",
                sql: "\"Version\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rewards_GameSessionId_RoundNumber",
                table: "rewards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_rewards_IntensityLevel",
                table: "rewards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_rewards_RoundNumber",
                table: "rewards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_game_sessions_RoundNumber",
                table: "game_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_game_sessions_Version",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "ActorPlayerId",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "CatalogVersion",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "ExecutionType",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "ExecutionValue",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "IntensityLevel",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "ReceiverPlayerId",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "AnsweredRoundNumber",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "PendingRoundResultJson",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "RewardProgressionJson",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "game_sessions");
        }
    }
}
