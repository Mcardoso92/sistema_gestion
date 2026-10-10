using saas.Models;
using saas.Services;

namespace saas.Tests;

public class OnboardingEmpresaServiceTests
{
    [Fact]
    public async Task ObtenerEstadoAsync_DerivaElProgresoDesdeOperacionesValidas()
    {
        await using var context = TestDbContextFactory.Crear();
        context.Empresas.Add(CrearEmpresa(1, onboardingFinalizado: false));
        context.Productos.Add(new Producto
        {
            Id = 1, EmpresaId = 1, CategoriaId = 1, Nombre = "Producto", Estado = true
        });
        context.Ventas.AddRange(
            new Venta { Id = 1, EmpresaId = 1, UsuarioId = "u", Total = 10, Estado = false },
            new Venta { Id = 2, EmpresaId = 1, UsuarioId = "u", Total = 10, Estado = true });
        await context.SaveChangesAsync();

        var service = new OnboardingEmpresaService(context);
        var estado = await service.ObtenerEstadoAsync(1);

        Assert.NotNull(estado);
        Assert.True(estado.ProductoCreado);
        Assert.True(estado.VentaRegistrada);
        Assert.True(estado.Completado);
        Assert.Equal(2, estado.PasosCompletados);
        Assert.Equal(100, estado.Porcentaje);
    }

    [Fact]
    public async Task ObtenerEstadoAsync_NoMuestraEmpresasQueYaFinalizaron()
    {
        await using var context = TestDbContextFactory.Crear();
        context.Empresas.Add(CrearEmpresa(1, onboardingFinalizado: true));
        await context.SaveChangesAsync();

        var service = new OnboardingEmpresaService(context);

        Assert.Null(await service.ObtenerEstadoAsync(1));
    }

    [Fact]
    public async Task FinalizarAsync_SoloFinalizaCuandoLosDosPasosEstanCompletos()
    {
        await using var context = TestDbContextFactory.Crear();
        context.Empresas.Add(CrearEmpresa(1, onboardingFinalizado: false));
        await context.SaveChangesAsync();

        var service = new OnboardingEmpresaService(context);

        Assert.False(await service.FinalizarAsync(1));
        Assert.False(context.Empresas.Single().OnboardingFinalizado);

        context.Productos.Add(new Producto
        {
            Id = 1, EmpresaId = 1, CategoriaId = 1, Nombre = "Producto", Estado = true
        });
        context.Ventas.Add(new Venta
        {
            Id = 1, EmpresaId = 1, UsuarioId = "u", Total = 10, Estado = true
        });
        await context.SaveChangesAsync();

        Assert.True(await service.FinalizarAsync(1));
        Assert.True(context.Empresas.Single().OnboardingFinalizado);
        Assert.Null(await service.ObtenerEstadoAsync(1));
    }

    [Fact]
    public void EmpresaNueva_PorDefectoNoRecibeOnboardingRetroactivo()
    {
        var empresa = new Empresa();

        Assert.True(empresa.OnboardingFinalizado);
    }

    private static Empresa CrearEmpresa(int id, bool onboardingFinalizado) => new()
    {
        Id = id,
        Nombre = $"Empresa {id}",
        Estado = true,
        FechaAlta = DateTime.UtcNow,
        OnboardingFinalizado = onboardingFinalizado
    };
}
