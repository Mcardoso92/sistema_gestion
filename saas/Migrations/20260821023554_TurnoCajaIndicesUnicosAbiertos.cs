using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class TurnoCajaIndicesUnicosAbiertos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TurnosCaja_CajaId",
                table: "TurnosCaja");

            migrationBuilder.DropIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_CajaId",
                table: "TurnosCaja",
                column: "CajaId",
                unique: true,
                filter: "[Estado] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja",
                column: "UsuarioAperturaId",
                unique: true,
                filter: "[Estado] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TurnosCaja_CajaId",
                table: "TurnosCaja");

            migrationBuilder.DropIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_CajaId",
                table: "TurnosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja",
                column: "UsuarioAperturaId");
        }
    }
}
