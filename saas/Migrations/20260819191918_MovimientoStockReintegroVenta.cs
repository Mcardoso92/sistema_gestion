using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class MovimientoStockReintegroVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReintegroVentaId",
                table: "MovimientosStock",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ReintegroVentaId",
                table: "MovimientosStock",
                column: "ReintegroVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_ReintegrosVenta_ReintegroVentaId",
                table: "MovimientosStock",
                column: "ReintegroVentaId",
                principalTable: "ReintegrosVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_ReintegrosVenta_ReintegroVentaId",
                table: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_ReintegroVentaId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "ReintegroVentaId",
                table: "MovimientosStock");
        }
    }
}
