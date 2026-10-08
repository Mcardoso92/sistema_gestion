using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReversionHistorialValoresProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CambioRevertidoId",
                table: "CambiosCostoProducto",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperacionId",
                table: "CambiosCostoProducto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CambiosCostoProducto_CambioRevertidoId",
                table: "CambiosCostoProducto",
                column: "CambioRevertidoId",
                unique: true,
                filter: "[CambioRevertidoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CambiosCostoProducto_CambiosCostoProducto_CambioRevertidoId",
                table: "CambiosCostoProducto",
                column: "CambioRevertidoId",
                principalTable: "CambiosCostoProducto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CambiosCostoProducto_CambiosCostoProducto_CambioRevertidoId",
                table: "CambiosCostoProducto");

            migrationBuilder.DropIndex(
                name: "IX_CambiosCostoProducto_CambioRevertidoId",
                table: "CambiosCostoProducto");

            migrationBuilder.DropColumn(
                name: "CambioRevertidoId",
                table: "CambiosCostoProducto");

            migrationBuilder.DropColumn(
                name: "OperacionId",
                table: "CambiosCostoProducto");
        }
    }
}
