using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortHub.API.Migrations
{
    /// <inheritdoc />
    public partial class EmbarqueTicketUnicoYVuelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoVuelo",
                table: "Embarques",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Embarques_NumeroTicket",
                table: "Embarques",
                column: "NumeroTicket",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Embarques_NumeroTicket",
                table: "Embarques");

            migrationBuilder.DropColumn(
                name: "CodigoVuelo",
                table: "Embarques");
        }
    }
}
