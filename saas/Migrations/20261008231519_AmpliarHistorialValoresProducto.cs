using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarHistorialValoresProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoValor",
                table: "CambiosCostoProducto",
                type: "int",
                nullable: false,
                // Todos los registros previos pertenecen al historial de costo.
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoValor",
                table: "CambiosCostoProducto");
        }
    }
}
