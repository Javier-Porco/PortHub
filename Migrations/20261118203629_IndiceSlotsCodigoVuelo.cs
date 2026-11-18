using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortHub.API.Migrations
{
    /// <inheritdoc />
    public partial class IndiceSlotsCodigoVuelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Slots_CodigoVuelo",
                table: "Slots",
                column: "CodigoVuelo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_CodigoVuelo",
                table: "Slots");
        }
    }
}
