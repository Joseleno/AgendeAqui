using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendeAqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "specialty",
                table: "professionals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "specialty",
                table: "professionals");
        }
    }
}
