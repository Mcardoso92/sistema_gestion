using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInventarioFisico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventarioFisicoId",
                table: "MovimientosStock",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventariosFisicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioInicioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsuarioConfirmacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosFisicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosFisicos_AspNetUsers_UsuarioConfirmacionId",
                        column: x => x.UsuarioConfirmacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosFisicos_AspNetUsers_UsuarioInicioId",
                        column: x => x.UsuarioInicioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosFisicos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesInventarioFisico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarioFisicoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    StockTeorico = table.Column<int>(type: "int", nullable: false),
                    StockContado = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesInventarioFisico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesInventarioFisico_InventariosFisicos_InventarioFisicoId",
                        column: x => x.InventarioFisicoId,
                        principalTable: "InventariosFisicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesInventarioFisico_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_InventarioFisicoId",
                table: "MovimientosStock",
                column: "InventarioFisicoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesInventarioFisico_InventarioFisicoId_ProductoId",
                table: "DetallesInventarioFisico",
                columns: new[] { "InventarioFisicoId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesInventarioFisico_ProductoId",
                table: "DetallesInventarioFisico",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFisicos_EmpresaId",
                table: "InventariosFisicos",
                column: "EmpresaId",
                unique: true,
                filter: "[Estado] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFisicos_UsuarioConfirmacionId",
                table: "InventariosFisicos",
                column: "UsuarioConfirmacionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosFisicos_UsuarioInicioId",
                table: "InventariosFisicos",
                column: "UsuarioInicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_InventariosFisicos_InventarioFisicoId",
                table: "MovimientosStock",
                column: "InventarioFisicoId",
                principalTable: "InventariosFisicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_InventariosFisicos_InventarioFisicoId",
                table: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "DetallesInventarioFisico");

            migrationBuilder.DropTable(
                name: "InventariosFisicos");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_InventarioFisicoId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "InventarioFisicoId",
                table: "MovimientosStock");
        }
    }
}
