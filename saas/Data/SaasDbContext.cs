using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using saas.Models;

namespace saas.Data
{
    public class SaasDbContext : IdentityDbContext<Usuario>
    {
        public SaasDbContext(DbContextOptions<SaasDbContext> options) : base(options)
        {
        }
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Venta> Ventas { get; set; } = null!;
        public DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<MovimientoStock> MovimientosStock { get; set; } = null!;
        public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<Compra> Compras { get; set; } = null!;
        public DbSet<DetalleCompra> DetallesCompra { get; set; } = null!;
        public DbSet<Caja> Cajas { get; set; } = null!;
        public DbSet<MedioPago> MediosPago { get; set; } = null!;
        public DbSet<CajaMedioPago> CajaMediosPago { get; set; } = null!;
        public DbSet<CategoriaGasto> CategoriasGasto { get; set; } = null!;
        public DbSet<TurnoCaja> TurnosCaja { get; set; } = null!;
        public DbSet<CobroVenta> CobrosVenta { get; set; } = null!;
        public DbSet<PagoProveedor> PagosProveedor { get; set; } = null!;
        public DbSet<ReintegroVenta> ReintegrosVenta { get; set; } = null!;
        public DbSet<DetalleReintegroVenta> DetallesReintegroVenta { get; set; } = null!;
        public DbSet<ReintegroProveedor> ReintegrosProveedor { get; set; } = null!;
        public DbSet<TransferenciaCaja> TransferenciasCaja { get; set; } = null!;
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarRelaciones(modelBuilder);
            ConfigurarIndices(modelBuilder);
            ConfigurarPropiedades(modelBuilder);
        }

        private static void ConfigurarRelaciones(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Categorias)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Clientes)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Productos)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Empresa)
                .WithMany(e => e.Ventas)
                .HasForeignKey(v => v.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.MovimientosStock)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Empresa)
                .WithMany(e => e.MovimientosStock)
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientosStock)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Venta)
                .WithMany(v => v.MovimientosStock)
                .HasForeignKey(m => m.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Compra)
                .WithMany(c => c.MovimientosStock)
                .HasForeignKey(m => m.CompraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.ReintegroVenta)
                .WithMany(r => r.MovimientosStock)
                .HasForeignKey(m => m.ReintegroVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proveedor>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Proveedores)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Compras)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Compras)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.UsuarioAnulacion)
                .WithMany(u => u.ComprasAnuladas)
                .HasForeignKey(c => c.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Caja>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Cajas)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedioPago>()
                .HasOne(m => m.Empresa)
                .WithMany(e => e.MediosPago)
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoriaGasto>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.CategoriasGasto)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CajaMedioPago>()
                .HasOne(cm => cm.Caja)
                .WithMany(c => c.CajaMediosPago)
                .HasForeignKey(cm => cm.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CajaMedioPago>()
                .HasOne(cm => cm.MedioPago)
                .WithMany(m => m.CajaMediosPago)
                .HasForeignKey(cm => cm.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoCaja>()
                .HasOne(t => t.Empresa)
                .WithMany(e => e.TurnosCaja)
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoCaja>()
                .HasOne(t => t.Caja)
                .WithMany(c => c.TurnosCaja)
                .HasForeignKey(t => t.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoCaja>()
                .HasOne(t => t.UsuarioApertura)
                .WithMany(u => u.TurnosCajaApertura)
                .HasForeignKey(t => t.UsuarioAperturaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoCaja>()
                .HasOne(t => t.UsuarioCierre)
                .WithMany(u => u.TurnosCajaCierre)
                .HasForeignKey(t => t.UsuarioCierreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.Venta)
                .WithMany(v => v.CobrosVenta)
                .HasForeignKey(c => c.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.CobrosVenta)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.Caja)
                .WithMany()
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.MedioPago)
                .WithMany(m => m.CobrosVenta)
                .HasForeignKey(c => c.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.TurnoCaja)
                .WithMany()
                .HasForeignKey(c => c.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CobrosVenta)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CobroVenta>()
                .HasOne(c => c.UsuarioAnulacion)
                .WithMany(u => u.CobrosVentaAnulados)
                .HasForeignKey(c => c.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.Compra)
                .WithMany(c => c.PagosProveedor)
                .HasForeignKey(p => p.CompraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.PagosProveedor)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.Caja)
                .WithMany()
                .HasForeignKey(p => p.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.MedioPago)
                .WithMany(m => m.PagosProveedor)
                .HasForeignKey(p => p.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.TurnoCaja)
                .WithMany()
                .HasForeignKey(p => p.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.PagosProveedor)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagoProveedor>()
                .HasOne(p => p.UsuarioAnulacion)
                .WithMany(u => u.PagosProveedorAnulados)
                .HasForeignKey(p => p.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.Venta)
                .WithMany(v => v.ReintegrosVenta)
                .HasForeignKey(r => r.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.Empresa)
                .WithMany(e => e.ReintegrosVenta)
                .HasForeignKey(r => r.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.Caja)
                .WithMany()
                .HasForeignKey(r => r.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.MedioPago)
                .WithMany(m => m.ReintegrosVenta)
                .HasForeignKey(r => r.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.TurnoCaja)
                .WithMany()
                .HasForeignKey(r => r.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.ReintegrosVenta)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroVenta>()
                .HasOne(r => r.UsuarioAnulacion)
                .WithMany(u => u.ReintegrosVentaAnulados)
                .HasForeignKey(r => r.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleReintegroVenta>()
                .HasOne(d => d.ReintegroVenta)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.ReintegroVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleReintegroVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.Compra)
                .WithMany(c => c.ReintegrosProveedor)
                .HasForeignKey(r => r.CompraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.Empresa)
                .WithMany(e => e.ReintegrosProveedor)
                .HasForeignKey(r => r.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.Caja)
                .WithMany()
                .HasForeignKey(r => r.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.MedioPago)
                .WithMany(m => m.ReintegrosProveedor)
                .HasForeignKey(r => r.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.TurnoCaja)
                .WithMany()
                .HasForeignKey(r => r.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.ReintegrosProveedor)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasOne(r => r.UsuarioAnulacion)
                .WithMany(u => u.ReintegrosProveedorAnulados)
                .HasForeignKey(r => r.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.Empresa)
                .WithMany(e => e.TransferenciasCaja)
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.CajaOrigen)
                .WithMany(c => c.TransferenciasOrigen)
                .HasForeignKey(t => t.CajaOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.CajaDestino)
                .WithMany(c => c.TransferenciasDestino)
                .HasForeignKey(t => t.CajaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.TransferenciasCaja)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.TurnoCaja)
                .WithMany()
                .HasForeignKey(t => t.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasOne(t => t.UsuarioAnulacion)
                .WithMany(u => u.TransferenciasCajaAnuladas)
                .HasForeignKey(t => t.UsuarioAnulacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Empresa)
                .WithMany(e => e.MovimientosCaja)
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Caja)
                .WithMany(c => c.MovimientosCaja)
                .HasForeignKey(m => m.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientosCaja)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.MedioPago)
                .WithMany(mp => mp.MovimientosCaja)
                .HasForeignKey(m => m.MedioPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.TurnoCaja)
                .WithMany(t => t.MovimientosCaja)
                .HasForeignKey(m => m.TurnoCajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.CategoriaGasto)
                .WithMany(c => c.MovimientosCaja)
                .HasForeignKey(m => m.CategoriaGastoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.MovimientoOrigen)
                .WithMany(m => m.Reversiones)
                .HasForeignKey(m => m.MovimientoOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.CobroVenta)
                .WithOne(c => c.MovimientoCaja)
                .HasForeignKey<MovimientoCaja>(m => m.CobroVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.PagoProveedor)
                .WithOne(p => p.MovimientoCaja)
                .HasForeignKey<MovimientoCaja>(m => m.PagoProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.ReintegroVenta)
                .WithOne(r => r.MovimientoCaja)
                .HasForeignKey<MovimientoCaja>(m => m.ReintegroVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.ReintegroProveedor)
                .WithOne(r => r.MovimientoCaja)
                .HasForeignKey<MovimientoCaja>(m => m.ReintegroProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.TransferenciaCaja)
                .WithMany(t => t.MovimientosCaja)
                .HasForeignKey(m => m.TransferenciaCajaId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigurarIndices(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.Nombre)
                .IsUnique();

            modelBuilder.Entity<Categoria>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.Nombre
                })
                .IsUnique();
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.Documento
                })
                .IsUnique()
                .HasFilter("[Documento] IS NOT NULL");

            modelBuilder.Entity<Producto>()
                .HasIndex(p => new
                {
                    p.EmpresaId,
                    p.Nombre
                })
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => new
                {
                    p.EmpresaId,
                    p.CodigoBarra
                })
                .IsUnique()
                .HasFilter("[CodigoBarra] IS NOT NULL");

            modelBuilder.Entity<Venta>()
                .HasIndex(v => new
                {
                    v.EmpresaId,
                    v.Fecha
                });

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.ClienteId);

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.UsuarioId);

            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => m.ProductoId);

            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => m.EmpresaId);

            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => m.Fecha);

            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => m.VentaId);

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.EmpresaId);

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => new { p.EmpresaId, p.CUIT });

            modelBuilder.Entity<Compra>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.Fecha
                });

            modelBuilder.Entity<Compra>()
                .HasIndex(c => c.ProveedorId);

            modelBuilder.Entity<Compra>()
                .HasIndex(c => c.UsuarioId);

            modelBuilder.Entity<Compra>()
                .HasIndex(c => c.UsuarioAnulacionId);

            modelBuilder.Entity<DetalleCompra>()
                .HasIndex(d => d.CompraId);

            modelBuilder.Entity<DetalleCompra>()
                .HasIndex(d => d.ProductoId);

            modelBuilder.Entity<DetalleCompra>()
                .HasIndex(d => new
                {
                    d.CompraId,
                    d.ProductoId
                })
                .IsUnique();

            modelBuilder.Entity<MovimientoStock>()
                .HasIndex(m => m.CompraId);

            modelBuilder.Entity<Compra>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.ProveedorId,
                    c.TipoComprobante,
                    c.NumeroComprobante
                });

            // Caja
            modelBuilder.Entity<Caja>()
                .HasIndex(c => c.EmpresaId);

            modelBuilder.Entity<Caja>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.Nombre
                });

            // MedioPago
            modelBuilder.Entity<MedioPago>()
                .HasIndex(m => m.EmpresaId);

            modelBuilder.Entity<MedioPago>()
                .HasIndex(m => new
                {
                    m.EmpresaId,
                    m.Nombre
                });

            // CategoriaGasto
            modelBuilder.Entity<CategoriaGasto>()
                .HasIndex(c => c.EmpresaId);

            modelBuilder.Entity<CategoriaGasto>()
                .HasIndex(c => new
                {
                    c.EmpresaId,
                    c.Nombre
                });

            // CajaMedioPago
            modelBuilder.Entity<CajaMedioPago>()
                .HasIndex(cm => new
                {
                    cm.CajaId,
                    cm.MedioPagoId
                })
                .IsUnique();

            modelBuilder.Entity<TurnoCaja>()
                .HasIndex(t => new
                {
                    t.EmpresaId,
                    t.FechaApertura
                });

            modelBuilder.Entity<TurnoCaja>()
                .HasIndex(t => t.CajaId);

            modelBuilder.Entity<TurnoCaja>()
                .HasIndex(t => t.UsuarioAperturaId);

            modelBuilder.Entity<TurnoCaja>()
                .HasIndex(t => t.Estado);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.VentaId);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.EmpresaId);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.CajaId);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.MedioPagoId);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.TurnoCajaId);

            modelBuilder.Entity<CobroVenta>()
                .HasIndex(c => c.Fecha);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.CompraId);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.EmpresaId);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.CajaId);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.MedioPagoId);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.TurnoCajaId);

            modelBuilder.Entity<PagoProveedor>()
                .HasIndex(p => p.Fecha);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.VentaId);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.EmpresaId);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.CajaId);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.MedioPagoId);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.TurnoCajaId);

            modelBuilder.Entity<ReintegroVenta>()
                .HasIndex(r => r.Fecha);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.CompraId);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.EmpresaId);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.CajaId);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.MedioPagoId);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.TurnoCajaId);

            modelBuilder.Entity<ReintegroProveedor>()
                .HasIndex(r => r.Fecha);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasIndex(t => t.EmpresaId);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasIndex(t => t.CajaOrigenId);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasIndex(t => t.CajaDestinoId);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasIndex(t => t.TurnoCajaId);

            modelBuilder.Entity<TransferenciaCaja>()
                .HasIndex(t => t.Fecha);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => new
                {
                    m.EmpresaId,
                    m.Fecha
                });

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => new
                {
                    m.CajaId,
                    m.Fecha
                });

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.UsuarioId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.TurnoCajaId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.MedioPagoId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.CategoriaGastoId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.Tipo);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.MovimientoOrigenId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.CobroVentaId)
                .IsUnique()
                .HasFilter("[CobroVentaId] IS NOT NULL");

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.PagoProveedorId)
                .IsUnique()
                .HasFilter("[PagoProveedorId] IS NOT NULL");

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.ReintegroVentaId)
                .IsUnique()
                .HasFilter("[ReintegroVentaId] IS NOT NULL");

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.ReintegroProveedorId)
                .IsUnique()
                .HasFilter("[ReintegroProveedorId] IS NOT NULL");

            modelBuilder.Entity<MovimientoCaja>()
                .HasIndex(m => m.TransferenciaCajaId);
        }

        private static void ConfigurarPropiedades(
            ModelBuilder modelBuilder)
        {
            //DECIMALES
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCosto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Compra>()
                .Property(c => c.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioCostoAnterior)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioVentaAnterior)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioVentaNuevo)
                .HasPrecision(18, 2);

            //MovmientoStock
            modelBuilder.Entity<MovimientoStock>()
                .Property(m => m.Motivo)
                .HasMaxLength(250);

            // Caja
            modelBuilder.Entity<Caja>()
                .Property(c => c.FondoFijo)
                .HasPrecision(18, 2);

            // TurnoCaja
            modelBuilder.Entity<TurnoCaja>()
                .Property(t => t.FondoFijoAplicado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TurnoCaja>()
                .Property(t => t.EfectivoEsperado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TurnoCaja>()
                .Property(t => t.EfectivoContado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TurnoCaja>()
                .Property(t => t.Diferencia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TurnoCaja>()
                .Property(t => t.ImporteRendido)
                .HasPrecision(18, 2);

            // CobroVenta
            modelBuilder.Entity<CobroVenta>()
                .Property(c => c.Importe)
                .HasPrecision(18, 2);

            // PagoProveedor
            modelBuilder.Entity<PagoProveedor>()
                .Property(p => p.Importe)
                .HasPrecision(18, 2);

            // Reintegros
            modelBuilder.Entity<ReintegroVenta>()
                .Property(r => r.Importe)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ReintegroProveedor>()
                .Property(r => r.Importe)
                .HasPrecision(18, 2);

            // TransferenciaCaja
            modelBuilder.Entity<TransferenciaCaja>()
                .Property(t => t.Importe)
                .HasPrecision(18, 2);

            // MovimientoCaja
            modelBuilder.Entity<MovimientoCaja>()
                .Property(m => m.Importe)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MovimientoCaja>()
                .Property(m => m.Concepto)
                .HasMaxLength(250);

            modelBuilder.Entity<MovimientoCaja>()
                .Property(m => m.Observaciones)
                .HasMaxLength(500);

            modelBuilder.Entity<TransferenciaCaja>()
                .Property(t => t.Motivo)
                .HasMaxLength(250);

            modelBuilder.Entity<TransferenciaCaja>()
                .Property(t => t.MotivoAnulacion)
                .HasMaxLength(500);

            //Reintegro Venta

            modelBuilder.Entity<DetalleReintegroVenta>(entity =>
            {
                entity.Property(e => e.PrecioUnitario)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Subtotal)
                    .HasPrecision(18, 2);
            });
        }
    }
}
