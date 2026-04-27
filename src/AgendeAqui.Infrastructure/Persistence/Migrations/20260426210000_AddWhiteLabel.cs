using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendeAqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWhiteLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "custom_domain",
                table: "tenants",
                type: "character varying(253)",
                maxLength: 253,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "trial_ends_at",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "theme",
                table: "tenants",
                type: "jsonb",
                nullable: false,
                defaultValue: "{\"primaryColor\":\"#6366f1\"}");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_custom_domain",
                table: "tenants",
                column: "custom_domain",
                unique: true,
                filter: "custom_domain IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_custom_domain",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "custom_domain",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "trial_ends_at",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "theme",
                table: "tenants");
        }
    }
}
