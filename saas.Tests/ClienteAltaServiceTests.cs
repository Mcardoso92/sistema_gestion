using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel;

namespace saas.Tests;

public class ClienteAltaServiceTests
{
    [Fact]
    public async Task CrearAsync_ClienteValido_LoAsociaALaEmpresa()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresa = await CrearEmpresa(context, "Empresa uno");
        var service = new ClienteAltaService(
            context,
            new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(
            new ClienteCreateVM
            {
                Nombre = "  Ana  ",
                Apellido = "  Pérez  ",
                Documento = " 12345678 "
            },
            empresa.Id);

        Assert.True(resultado.Exitoso);
        Assert.Equal(empresa.Id, resultado.Cliente!.EmpresaId);
        Assert.Equal("Ana", resultado.Cliente.Nombre);
        Assert.Equal("Pérez", resultado.Cliente.Apellido);
        Assert.Equal("12345678", resultado.Cliente.Documento);
        Assert.True(resultado.Cliente.Estado);
    }

    [Fact]
    public async Task CrearAsync_DocumentoDuplicadoEnLaMismaEmpresa_NoCreaCliente()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresa = await CrearEmpresa(context, "Empresa uno");
        context.Clientes.Add(new Cliente
        {
            Nombre = "Existente",
            Documento = "12345678",
            EmpresaId = empresa.Id,
            Estado = true
        });
        await context.SaveChangesAsync();
        var service = new ClienteAltaService(
            context,
            new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(
            new ClienteCreateVM
            {
                Nombre = "Nuevo",
                Documento = "12345678"
            },
            empresa.Id);

        Assert.False(resultado.Exitoso);
        Assert.Contains(nameof(ClienteCreateVM.Documento), resultado.Errores.Keys);
        Assert.Equal(1, await context.Clientes.CountAsync());
    }

    [Fact]
    public async Task CrearAsync_MismoDocumentoEnOtraEmpresa_PermiteElAlta()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresaUno = await CrearEmpresa(context, "Empresa uno");
        var empresaDos = await CrearEmpresa(context, "Empresa dos");
        context.Clientes.Add(new Cliente
        {
            Nombre = "Empresa uno",
            Documento = "12345678",
            EmpresaId = empresaUno.Id,
            Estado = true
        });
        await context.SaveChangesAsync();
        var service = new ClienteAltaService(
            context,
            new FechaHoraServicePrueba());

        var resultado = await service.CrearAsync(
            new ClienteCreateVM
            {
                Nombre = "Empresa dos",
                Documento = "12345678"
            },
            empresaDos.Id);

        Assert.True(resultado.Exitoso);
        Assert.Equal(empresaDos.Id, resultado.Cliente!.EmpresaId);
    }

    private static async Task<Empresa> CrearEmpresa(
        SaasDbContext context,
        string nombre)
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
