using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class DetalleReintegroVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetallesReintegroVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReintegroVentaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesReintegroVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesReintegroVenta_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesReintegroVenta_ReintegrosVenta_ReintegroVentaId",
                        column: x => x.ReintegroVentaId,
                        principalTable: "ReintegrosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesReintegroVenta_ProductoId",
                table: "DetallesReintegroVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesReintegroVenta_ReintegroVentaId",
                table: "DetallesReintegroVenta",
                column: "ReintegroVentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesReintegroVenta");
        }
    }
}
