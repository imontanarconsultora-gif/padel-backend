using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxAlumnos = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotifLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotifLogs_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TurnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaClase = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TokenCancelacion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordatorioEnviado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_Telefono",
                table: "Alumnos",
                column: "Telefono",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotifLogs_AlumnoId",
                table: "NotifLogs",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AlumnoId_TurnoId_FechaClase",
                table: "Reservas",
                columns: new[] { "AlumnoId", "TurnoId", "FechaClase" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_TokenCancelacion",
                table: "Reservas",
                column: "TokenCancelacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_TurnoId",
                table: "Reservas",
                column: "TurnoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotifLogs");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Alumnos");

            migrationBuilder.DropTable(
                name: "Turnos");
        }
    }
}
