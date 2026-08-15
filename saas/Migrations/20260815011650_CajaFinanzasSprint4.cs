using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saas.Migrations
{
    /// <inheritdoc />
    public partial class CajaFinanzasSprint4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    PermiteTurnos = table.Column<bool>(type: "bit", nullable: false),
                    FondoFijo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cajas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasGasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasGasto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasGasto_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediosPago_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurnosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAperturaId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCierreId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CierreForzado = table.Column<bool>(type: "bit", nullable: false),
                    MotivoCierreForzado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FondoFijoAplicado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EfectivoEsperado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EfectivoContado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ImporteRendido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_AspNetUsers_UsuarioAperturaId",
                        column: x => x.UsuarioAperturaId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_AspNetUsers_UsuarioCierreId",
                        column: x => x.UsuarioCierreId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CajaMediosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajaMediosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CajaMediosPago_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CajaMediosPago_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CobrosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: false),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: false),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosProveedor_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReintegrosProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: false),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReintegrosProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosProveedor_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReintegrosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: false),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReintegrosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferenciasCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaOrigenId = table.Column<int>(type: "int", nullable: false),
                    CajaDestinoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_AspNetUsers_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_Cajas_CajaDestinoId",
                        column: x => x.CajaDestinoId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_Cajas_CajaOrigenId",
                        column: x => x.CajaOrigenId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferenciasCaja_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<int>(type: "int", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: true),
                    TurnoCajaId = table.Column<int>(type: "int", nullable: true),
                    CategoriaGastoId = table.Column<int>(type: "int", nullable: true),
                    Concepto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MovimientoOrigenId = table.Column<int>(type: "int", nullable: true),
                    CobroVentaId = table.Column<int>(type: "int", nullable: true),
                    PagoProveedorId = table.Column<int>(type: "int", nullable: true),
                    ReintegroVentaId = table.Column<int>(type: "int", nullable: true),
                    ReintegroProveedorId = table.Column<int>(type: "int", nullable: true),
                    TransferenciaCajaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_CategoriasGasto_CategoriaGastoId",
                        column: x => x.CategoriaGastoId,
                        principalTable: "CategoriasGasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_CobrosVenta_CobroVentaId",
                        column: x => x.CobroVentaId,
                        principalTable: "CobrosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_MovimientosCaja_MovimientoOrigenId",
                        column: x => x.MovimientoOrigenId,
                        principalTable: "MovimientosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_PagosProveedor_PagoProveedorId",
                        column: x => x.PagoProveedorId,
                        principalTable: "PagosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_ReintegrosProveedor_ReintegroProveedorId",
                        column: x => x.ReintegroProveedorId,
                        principalTable: "ReintegrosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_ReintegrosVenta_ReintegroVentaId",
                        column: x => x.ReintegroVentaId,
                        principalTable: "ReintegrosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_TransferenciasCaja_TransferenciaCajaId",
                        column: x => x.TransferenciaCajaId,
                        principalTable: "TransferenciasCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CajaMediosPago_CajaId_MedioPagoId",
                table: "CajaMediosPago",
                columns: new[] { "CajaId", "MedioPagoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CajaMediosPago_MedioPagoId",
                table: "CajaMediosPago",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_EmpresaId",
                table: "Cajas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_EmpresaId_Nombre",
                table: "Cajas",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasGasto_EmpresaId",
                table: "CategoriasGasto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasGasto_EmpresaId_Nombre",
                table: "CategoriasGasto",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_CajaId",
                table: "CobrosVenta",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_EmpresaId",
                table: "CobrosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_Fecha",
                table: "CobrosVenta",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_MedioPagoId",
                table: "CobrosVenta",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_TurnoCajaId",
                table: "CobrosVenta",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_UsuarioAnulacionId",
                table: "CobrosVenta",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_UsuarioId",
                table: "CobrosVenta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVenta_VentaId",
                table: "CobrosVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediosPago_EmpresaId",
                table: "MediosPago",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediosPago_EmpresaId_Nombre",
                table: "MediosPago",
                columns: new[] { "EmpresaId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaId_Fecha",
                table: "MovimientosCaja",
                columns: new[] { "CajaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CategoriaGastoId",
                table: "MovimientosCaja",
                column: "CategoriaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CobroVentaId",
                table: "MovimientosCaja",
                column: "CobroVentaId",
                unique: true,
                filter: "[CobroVentaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_EmpresaId_Fecha",
                table: "MovimientosCaja",
                columns: new[] { "EmpresaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_MedioPagoId",
                table: "MovimientosCaja",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_MovimientoOrigenId",
                table: "MovimientosCaja",
                column: "MovimientoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_PagoProveedorId",
                table: "MovimientosCaja",
                column: "PagoProveedorId",
                unique: true,
                filter: "[PagoProveedorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_ReintegroProveedorId",
                table: "MovimientosCaja",
                column: "ReintegroProveedorId",
                unique: true,
                filter: "[ReintegroProveedorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_ReintegroVentaId",
                table: "MovimientosCaja",
                column: "ReintegroVentaId",
                unique: true,
                filter: "[ReintegroVentaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_Tipo",
                table: "MovimientosCaja",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_TransferenciaCajaId",
                table: "MovimientosCaja",
                column: "TransferenciaCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_TurnoCajaId",
                table: "MovimientosCaja",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_CajaId",
                table: "PagosProveedor",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_CompraId",
                table: "PagosProveedor",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_EmpresaId",
                table: "PagosProveedor",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_Fecha",
                table: "PagosProveedor",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_MedioPagoId",
                table: "PagosProveedor",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_TurnoCajaId",
                table: "PagosProveedor",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_UsuarioAnulacionId",
                table: "PagosProveedor",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosProveedor_UsuarioId",
                table: "PagosProveedor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_CajaId",
                table: "ReintegrosProveedor",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_CompraId",
                table: "ReintegrosProveedor",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_EmpresaId",
                table: "ReintegrosProveedor",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_Fecha",
                table: "ReintegrosProveedor",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_MedioPagoId",
                table: "ReintegrosProveedor",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_TurnoCajaId",
                table: "ReintegrosProveedor",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_UsuarioAnulacionId",
                table: "ReintegrosProveedor",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosProveedor_UsuarioId",
                table: "ReintegrosProveedor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_CajaId",
                table: "ReintegrosVenta",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_EmpresaId",
                table: "ReintegrosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_Fecha",
                table: "ReintegrosVenta",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_MedioPagoId",
                table: "ReintegrosVenta",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_TurnoCajaId",
                table: "ReintegrosVenta",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_UsuarioAnulacionId",
                table: "ReintegrosVenta",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_UsuarioId",
                table: "ReintegrosVenta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosVenta_VentaId",
                table: "ReintegrosVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_CajaDestinoId",
                table: "TransferenciasCaja",
                column: "CajaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_CajaOrigenId",
                table: "TransferenciasCaja",
                column: "CajaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_EmpresaId",
                table: "TransferenciasCaja",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_Fecha",
                table: "TransferenciasCaja",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_TurnoCajaId",
                table: "TransferenciasCaja",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_UsuarioAnulacionId",
                table: "TransferenciasCaja",
                column: "UsuarioAnulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasCaja_UsuarioId",
                table: "TransferenciasCaja",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_CajaId",
                table: "TurnosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_EmpresaId_FechaApertura",
                table: "TurnosCaja",
                columns: new[] { "EmpresaId", "FechaApertura" });

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_Estado",
                table: "TurnosCaja",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja",
                column: "UsuarioAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioCierreId",
                table: "TurnosCaja",
                column: "UsuarioCierreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CajaMediosPago");

            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "CategoriasGasto");

            migrationBuilder.DropTable(
                name: "CobrosVenta");

            migrationBuilder.DropTable(
                name: "PagosProveedor");

            migrationBuilder.DropTable(
                name: "ReintegrosProveedor");

            migrationBuilder.DropTable(
                name: "ReintegrosVenta");

            migrationBuilder.DropTable(
                name: "TransferenciasCaja");

            migrationBuilder.DropTable(
                name: "MediosPago");

            migrationBuilder.DropTable(
                name: "TurnosCaja");

            migrationBuilder.DropTable(
                name: "Cajas");
        }
    }
}
