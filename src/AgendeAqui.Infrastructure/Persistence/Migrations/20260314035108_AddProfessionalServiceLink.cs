using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendeAqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalServiceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professional_services",
                columns: table => new
                {
                    professional_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_services", x => new { x.professional_id, x.service_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_professional_services_tenant_professional",
                table: "professional_services",
                columns: new[] { "tenant_id", "professional_id" });

            migrationBuilder.CreateIndex(
                name: "ix_professional_services_tenant_service",
                table: "professional_services",
                columns: new[] { "tenant_id", "service_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professional_services");
        }
    }
}
