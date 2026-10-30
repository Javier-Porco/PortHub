using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodigoVuelo = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoAerolinea = table.Column<string>(type: "TEXT", nullable: false),
                    HorarioProgramado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Pista = table.Column<int>(type: "INTEGER", nullable: false),
                    Gate = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slots_Pista_HorarioProgramado",
                table: "Slots",
                columns: new[] { "Pista", "HorarioProgramado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slots");
        }
    }
}
