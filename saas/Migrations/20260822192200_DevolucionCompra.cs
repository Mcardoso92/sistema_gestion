using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class DevolucionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DevolucionCompraId",
                table: "MovimientosStock",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DevolucionesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesCompra_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesCompra_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesCompra_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesDevolucionCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevolucionCompraId = table.Column<int>(type: "int", nullable: false),
                    DetalleCompraId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesDevolucionCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucionCompra_DetallesCompra_DetalleCompraId",
                        column: x => x.DetalleCompraId,
                        principalTable: "DetallesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucionCompra_DevolucionesCompra_DevolucionCompraId",
                        column: x => x.DevolucionCompraId,
                        principalTable: "DevolucionesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesDevolucionCompra_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_DevolucionCompraId",
                table: "MovimientosStock",
                column: "DevolucionCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucionCompra_DetalleCompraId",
                table: "DetallesDevolucionCompra",
                column: "DetalleCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucionCompra_DevolucionCompraId",
                table: "DetallesDevolucionCompra",
                column: "DevolucionCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucionCompra_DevolucionCompraId_DetalleCompraId",
                table: "DetallesDevolucionCompra",
                columns: new[] { "DevolucionCompraId", "DetalleCompraId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesDevolucionCompra_ProductoId",
                table: "DetallesDevolucionCompra",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCompra_CompraId",
                table: "DevolucionesCompra",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCompra_EmpresaId",
                table: "DevolucionesCompra",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCompra_Fecha",
                table: "DevolucionesCompra",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCompra_UsuarioAnulacionId",
                table: "DevolucionesCompra",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCompra_UsuarioId",
                table: "DevolucionesCompra",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_DevolucionesCompra_DevolucionCompraId",
                table: "MovimientosStock",
                column: "DevolucionCompraId",
                principalTable: "DevolucionesCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_DevolucionesCompra_DevolucionCompraId",
                table: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "DetallesDevolucionCompra");

            migrationBuilder.DropTable(
                name: "DevolucionesCompra");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_DevolucionCompraId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "DevolucionCompraId",
                table: "MovimientosStock");
        }
    }
}
