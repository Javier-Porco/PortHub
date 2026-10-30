using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaEmbarques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Embarques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroTicket = table.Column<string>(type: "TEXT", nullable: false),
                    Gate = table.Column<string>(type: "TEXT", nullable: false),
                    HorarioAcceso = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embarques", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Embarques");
        }
    }
}
