using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendeAqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsAndTenantContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "features_enabled",
                table: "tenants",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "labels",
                table: "tenants",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    leader_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_professionals_leader_id",
                        column: x => x.leader_id,
                        principalTable: "professionals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "team_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    professional_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    left_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_team_members_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_team_members_active_unique",
                table: "team_members",
                columns: new[] { "team_id", "professional_id" },
                unique: true,
                filter: "left_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_team_members_tenant_professional",
                table: "team_members",
                columns: new[] { "tenant_id", "professional_id" },
                filter: "left_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_team_members_tenant_team_dates",
                table: "team_members",
                columns: new[] { "tenant_id", "team_id", "joined_at", "left_at" });

            migrationBuilder.CreateIndex(
                name: "IX_teams_leader_id",
                table: "teams",
                column: "leader_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_tenant_active",
                table: "teams",
                columns: new[] { "tenant_id", "is_active" },
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_teams_tenant_id",
                table: "teams",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "team_members");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropColumn(
                name: "features_enabled",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "labels",
                table: "tenants");
        }
    }
}
