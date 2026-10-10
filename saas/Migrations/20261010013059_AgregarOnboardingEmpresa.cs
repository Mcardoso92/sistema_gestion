using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOnboardingEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnboardingFinalizado",
                table: "Empresas",
                type: "bit",
                nullable: false,
                // El checklist es una experiencia inicial y no debe aparecer
                // retroactivamente en comercios que ya utilizan Veltika.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingFinalizado",
                table: "Empresas");
        }
    }
}
