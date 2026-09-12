using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel;

namespace saas.Tests;

public class ProveedorAltaServiceTests
{
    [Fact]
    public async Task CrearAsync_ProveedorValido_LoAsociaALaEmpresa()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresa = await CrearEmpresa(context, "Empresa uno");
        var service = new ProveedorAltaService(context, new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(new ProveedorCreateVM
        {
            RazonSocial = "  Distribuidora Sur  ",
            NombreFantasia = "  Sur  ",
            CUIT = "20-12345678-6"
        }, empresa.Id);

        Assert.True(resultado.Exitoso);
        Assert.Equal(empresa.Id, resultado.Proveedor!.EmpresaId);
        Assert.Equal("Distribuidora Sur", resultado.Proveedor.RazonSocial);
        Assert.Equal("Sur", resultado.Proveedor.NombreFantasia);
        Assert.Equal("20123456786", resultado.Proveedor.CUIT);
        Assert.True(resultado.Proveedor.Estado);
    }

    [Fact]
    public async Task CrearAsync_CuitDuplicadoEnLaMismaEmpresa_NoCreaProveedor()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresa = await CrearEmpresa(context, "Empresa uno");
        context.Proveedores.Add(new Proveedor
        {
            RazonSocial = "Existente",
            CUIT = "20123456786",
            EmpresaId = empresa.Id,
            Estado = true
        });
        await context.SaveChangesAsync();
        var service = new ProveedorAltaService(context, new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(new ProveedorCreateVM
        {
            RazonSocial = "Nuevo",
            CUIT = "20-12345678-6"
        }, empresa.Id);

        Assert.False(resultado.Exitoso);
        Assert.Contains(nameof(ProveedorCreateVM.CUIT), resultado.Errores.Keys);
        Assert.Equal(1, await context.Proveedores.CountAsync());
    }

    [Fact]
    public async Task CrearAsync_MismoCuitEnOtraEmpresa_PermiteElAlta()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresaUno = await CrearEmpresa(context, "Empresa uno");
        var empresaDos = await CrearEmpresa(context, "Empresa dos");
        context.Proveedores.Add(new Proveedor
        {
            RazonSocial = "Empresa uno",
            CUIT = "20123456786",
            EmpresaId = empresaUno.Id,
            Estado = true
        });
        await context.SaveChangesAsync();
        var service = new ProveedorAltaService(context, new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(new ProveedorCreateVM
        {
            RazonSocial = "Empresa dos",
            CUIT = "20-12345678-6"
        }, empresaDos.Id);

        Assert.True(resultado.Exitoso);
        Assert.Equal(empresaDos.Id, resultado.Proveedor!.EmpresaId);
    }

    private static async Task<Empresa> CrearEmpresa(SaasDbContext context, string nombre)
    {
        var empresa = new Empresa
        {
            Nombre = nombre,
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };

        context.Empresas.Add(empresa);
        await context.SaveChangesAsync();
        return empresa;
    }
}
