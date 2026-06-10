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
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuración de relaciones
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Empresa)
                .WithMany(e => e.Ventas)
                .HasForeignKey(v => v.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Productos)
                .HasForeignKey(p => p.EmpresaId)
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

            // Configuración de precisión para campos decimales
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

        }
    }
}
