using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class Proveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazonSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NombreFantasia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CUIT = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Localidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoPostal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proveedores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_EmpresaId_CUIT",
                table: "Proveedores",
                columns: new[] { "EmpresaId", "CUIT" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proveedores");
        }
    }
}
