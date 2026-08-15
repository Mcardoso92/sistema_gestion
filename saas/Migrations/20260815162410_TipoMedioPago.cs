using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class TipoMedioPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "MediosPago",
                type: "int",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.Sql(@"
                UPDATE MediosPago
                SET Tipo =
                    CASE
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) = 'efectivo' THEN 1
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) = 'transferencia' THEN 2
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) IN ('tarjeta de débito', 'tarjeta de debito') THEN 3
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) IN ('tarjeta de crédito', 'tarjeta de credito') THEN 4
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) = 'qr' THEN 5
                        WHEN LOWER(LTRIM(RTRIM(Nombre))) = 'cheque' THEN 6
                        ELSE 7
                    END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "MediosPago");
        }
    }
}
