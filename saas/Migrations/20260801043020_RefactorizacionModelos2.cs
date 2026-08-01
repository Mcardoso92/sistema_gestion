using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class RefactorizacionModelos2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_EmpresaId",
                table: "Categorias");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_CodigoBarra",
                table: "Productos",
                columns: new[] { "EmpresaId", "CodigoBarra" },
                unique: true,
                filter: "[CodigoBarra] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_Nombre",
                table: "Productos",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Nombre",
                table: "Empresas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId_Nombre",
                table: "Categorias",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_CodigoBarra",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_Nombre",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_Nombre",
                table: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_EmpresaId_Nombre",
                table: "Categorias");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId",
                table: "Productos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId",
                table: "Categorias",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Empresas_EmpresaId",
                table: "AspNetUsers",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
