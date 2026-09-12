using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;

namespace saas.Tests;

public class RevisionNotificacionServiceTests
{
    [Fact]
    public async Task RegistrarRevision_CreaYActualizaUnUnicoEstado()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, Empresa empresa, _) = await PrepararDatos(context);
        var fechaHora = new FechaHoraServicePrueba(
            new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        var service = new RevisionNotificacionService(context, fechaHora);

        await service.RegistrarRevisionAsync(usuario, empresa.Id, esSuperAdmin: false);

        fechaHora.UtcAhora = new DateTime(2026, 9, 12, 11, 30, 0, DateTimeKind.Utc);
        await service.RegistrarRevisionAsync(usuario, empresa.Id, esSuperAdmin: false);

        RevisionNotificacion revision =
            await context.RevisionesNotificacion.SingleAsync();

        Assert.Equal(usuario.Id, revision.UsuarioId);
        Assert.Equal(empresa.Id, revision.EmpresaId);
        Assert.Equal(fechaHora.UtcAhora, revision.FechaUltimaRevision);
    }

    [Fact]
    public async Task ObtenerUltimaRevision_MantieneEstadosIndependientesPorUsuario()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, Empresa empresa, Usuario otroUsuario) =
            await PrepararDatos(context);
        var fechaHora = new FechaHoraServicePrueba(
            new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        var service = new RevisionNotificacionService(context, fechaHora);

        await service.RegistrarRevisionAsync(usuario, empresa.Id, esSuperAdmin: false);

        Assert.Equal(
            fechaHora.UtcAhora,
            await service.ObtenerUltimaRevisionAsync(usuario, empresa.Id, esSuperAdmin: false));
        Assert.Null(
            await service.ObtenerUltimaRevisionAsync(otroUsuario, empresa.Id, esSuperAdmin: false));
    }

    [Fact]
    public async Task RegistrarRevision_SuperAdminMantieneEstadosIndependientesPorEmpresa()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, Empresa empresa, _) = await PrepararDatos(context);
        var otraEmpresa = new Empresa
        {
            Nombre = "Otra empresa",
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        context.Empresas.Add(otraEmpresa);
        await context.SaveChangesAsync();

        var fechaHora = new FechaHoraServicePrueba(
            new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        var service = new RevisionNotificacionService(context, fechaHora);

        await service.RegistrarRevisionAsync(usuario, empresa.Id, esSuperAdmin: true);
        fechaHora.UtcAhora = new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc);
        await service.RegistrarRevisionAsync(usuario, otraEmpresa.Id, esSuperAdmin: true);

        Assert.Equal(2, await context.RevisionesNotificacion.CountAsync());
        Assert.Equal(
            new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc),
            await service.ObtenerUltimaRevisionAsync(usuario, empresa.Id, esSuperAdmin: true));
        Assert.Equal(
            fechaHora.UtcAhora,
            await service.ObtenerUltimaRevisionAsync(usuario, otraEmpresa.Id, esSuperAdmin: true));
    }

    [Fact]
    public async Task RegistrarRevision_RechazaOtraEmpresaParaUsuarioComun()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, _, _) = await PrepararDatos(context);
        var otraEmpresa = new Empresa
        {
            Nombre = "Empresa no autorizada",
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        context.Empresas.Add(otraEmpresa);
        await context.SaveChangesAsync();

        var service = new RevisionNotificacionService(
            context,
            new FechaHoraServicePrueba(DateTime.UtcNow));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RegistrarRevisionAsync(
                usuario,
                otraEmpresa.Id,
                esSuperAdmin: false));

        Assert.Empty(context.RevisionesNotificacion);
    }

    [Fact]
    public void Modelo_DefineIndiceUnicoPorUsuarioYEmpresa()
    {
        using SaasDbContext context = TestDbContextFactory.Crear();

        var indice = context.Model
            .FindEntityType(typeof(RevisionNotificacion))!
            .GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name)
                .SequenceEqual(new[]
                {
                    nameof(RevisionNotificacion.UsuarioId),
                    nameof(RevisionNotificacion.EmpresaId)
                }));

        Assert.True(indice.IsUnique);
    }

    private static async Task<(Usuario Usuario, Empresa Empresa, Usuario OtroUsuario)>
        PrepararDatos(SaasDbContext context)
    {
        var empresa = new Empresa
        {
            Nombre = "Empresa de prueba",
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        context.Empresas.Add(empresa);
        await context.SaveChangesAsync();

        var usuario = CrearUsuario("usuario-1", "usuario1@prueba.com", empresa.Id);
        var otroUsuario = CrearUsuario("usuario-2", "usuario2@prueba.com", empresa.Id);
        context.Users.AddRange(usuario, otroUsuario);
        await context.SaveChangesAsync();

        return (usuario, empresa, otroUsuario);
    }

    private static Usuario CrearUsuario(string id, string email, int empresaId) =>
        new()
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Nombre = "Usuario",
            Apellido = "Prueba",
            EmpresaId = empresaId,
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };

    private sealed class FechaHoraServicePrueba(DateTime utcAhora) : IFechaHoraService
    {
        public DateTime UtcAhora { get; set; } = utcAhora;
        public DateTime HoraLocalAhora => UtcAhora;
        public DateTime FechaLocalHoy => UtcAhora.Date;
        public DateTime ConvertirAHoraLocal(DateTime fechaUtc) => fechaUtc;
        public DateTime? ConvertirAHoraLocal(DateTime? fechaUtc) => fechaUtc;
        public DateTime ConvertirAUtc(DateTime fechaLocal) => fechaLocal;
    }
}
