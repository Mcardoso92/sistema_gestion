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

            modelBuilder.Entity<Proveedor>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Proveedores)
                .HasForeignKey(p => p.EmpresaId)
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

            //MovmientoStock
            modelBuilder.Entity<MovimientoStock>()
                .Property(m => m.Motivo)
                .HasMaxLength(250);
        }
    }
}
