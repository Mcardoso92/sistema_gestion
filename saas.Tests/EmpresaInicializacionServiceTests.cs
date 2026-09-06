using Microsoft.EntityFrameworkCore;
using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class EmpresaInicializacionServiceTests
{
    [Fact]
    public async Task InicializarAsync_CreaCajaPrincipalSinTurnosYAsociadaAEfectivo()
    {
        await using var context = TestDbContextFactory.Crear();
        var empresa = new Empresa
        {
            Nombre = "Empresa nueva",
            Estado = true,
            FechaAlta = DateTime.Now
        };
        context.Empresas.Add(empresa);
        await context.SaveChangesAsync();
        var service = new EmpresaInicializacionService(context);

        await service.InicializarAsync(empresa.Id, empresa.FechaAlta);

        Caja caja = await context.Cajas.SingleAsync(c =>
            c.EmpresaId == empresa.Id &&
            c.Nombre == "Caja principal");
        MedioPago efectivo = await context.MediosPago.SingleAsync(m =>
            m.EmpresaId == empresa.Id &&
            m.Tipo == TipoMedioPago.Efectivo);

        Assert.True(caja.Estado);
        Assert.Equal(TipoCaja.Efectivo, caja.Tipo);
        Assert.False(caja.PermiteTurnos);
        Assert.True(await context.CajaMediosPago.AnyAsync(cm =>
            cm.CajaId == caja.Id &&
            cm.MedioPagoId == efectivo.Id));
    }
}
