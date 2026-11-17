using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortHub.API.Migrations
{
    /// <inheritdoc />
    public partial class IndiceSlotsSoloActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_Pista_HorarioProgramado",
                table: "Slots");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_Pista_HorarioProgramado",
                table: "Slots",
                columns: new[] { "Pista", "HorarioProgramado" },
                unique: true,
                filter: "\"Estado\" <> 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_Pista_HorarioProgramado",
                table: "Slots");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_Pista_HorarioProgramado",
                table: "Slots",
                columns: new[] { "Pista", "HorarioProgramado" },
                unique: true);
        }
    }
}
