using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBienvenidaVisualizadaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BienvenidaVisualizada",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                // La migración no debe activar el modal para cuentas que ya utilizan Veltika.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BienvenidaVisualizada",
                table: "AspNetUsers");
        }
    }
}
