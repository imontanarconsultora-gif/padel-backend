using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGeneroATurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Turnos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Turnos");
        }
    }
}
