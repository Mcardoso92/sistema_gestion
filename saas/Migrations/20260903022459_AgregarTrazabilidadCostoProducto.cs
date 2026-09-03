using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTrazabilidadCostoProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CambiosCostoProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    CostoAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoNuevo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Origen = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambiosCostoProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CambiosCostoProducto_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CambiosCostoProducto_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CambiosCostoProducto_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CambiosCostoProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CambiosCostoProducto_CompraId",
                table: "CambiosCostoProducto",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CambiosCostoProducto_EmpresaId_ProductoId_Fecha",
                table: "CambiosCostoProducto",
                columns: new[] { "EmpresaId", "ProductoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_CambiosCostoProducto_ProductoId",
                table: "CambiosCostoProducto",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CambiosCostoProducto_UsuarioId",
                table: "CambiosCostoProducto",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CambiosCostoProducto");
        }
    }
}
