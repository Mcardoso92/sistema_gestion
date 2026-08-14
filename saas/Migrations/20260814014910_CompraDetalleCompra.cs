using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class CompraDetalleCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompraId",
                table: "MovimientosStock",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoComprobante = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioCostoAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioVentaAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PrecioVentaNuevo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_CompraId",
                table: "MovimientosStock",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_Fecha",
                table: "Compras",
                columns: new[] { "EmpresaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId_ProveedorId_TipoComprobante_NumeroComprobante",
                table: "Compras",
                columns: new[] { "EmpresaId", "ProveedorId", "TipoComprobante", "NumeroComprobante" });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProveedorId",
                table: "Compras",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_UsuarioAnulacionId",
                table: "Compras",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_UsuarioId",
                table: "Compras",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_CompraId",
                table: "DetallesCompra",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_CompraId_ProductoId",
                table: "DetallesCompra",
                columns: new[] { "CompraId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_ProductoId",
                table: "DetallesCompra",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_Compras_CompraId",
                table: "MovimientosStock",
                column: "CompraId",
                principalTable: "Compras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_Compras_CompraId",
                table: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "DetallesCompra");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_CompraId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "CompraId",
                table: "MovimientosStock");
        }
    }
}
