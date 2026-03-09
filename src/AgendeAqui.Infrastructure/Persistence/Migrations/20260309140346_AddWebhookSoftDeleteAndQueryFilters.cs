using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendeAqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookSoftDeleteAndQueryFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "webhooks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "webhooks");
        }
    }
}
