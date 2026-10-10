using saas.Models;
using saas.Services;

namespace saas.Tests;

public class BienvenidaUsuarioServiceTests
{
    [Fact]
    public async Task MarcarComoVisualizadaAsync_ActualizaSolamenteAlUsuarioIndicado()
    {
        await using var context = TestDbContextFactory.Crear();
        context.Users.AddRange(
            CrearUsuario("usuario-1", bienvenidaVisualizada: false),
            CrearUsuario("usuario-2", bienvenidaVisualizada: false));
        await context.SaveChangesAsync();

        var service = new BienvenidaUsuarioService(context);

        await service.MarcarComoVisualizadaAsync("usuario-1");

        Assert.True(context.Users.Single(u => u.Id == "usuario-1").BienvenidaVisualizada);
        Assert.False(context.Users.Single(u => u.Id == "usuario-2").BienvenidaVisualizada);
    }

    [Fact]
    public async Task MarcarComoVisualizadaAsync_EsIdempotente()
    {
        await using var context = TestDbContextFactory.Crear();
        context.Users.Add(CrearUsuario("usuario-1", bienvenidaVisualizada: false));
        await context.SaveChangesAsync();

        var service = new BienvenidaUsuarioService(context);

        await service.MarcarComoVisualizadaAsync("usuario-1");
        await service.MarcarComoVisualizadaAsync("usuario-1");

        Assert.True(context.Users.Single().BienvenidaVisualizada);
    }

    [Fact]
    public void UsuarioNuevo_PorDefectoNoRecibeBienvenidaRetroactiva()
    {
        var usuario = new Usuario();

        Assert.True(usuario.BienvenidaVisualizada);
    }

    private static Usuario CrearUsuario(string id, bool bienvenidaVisualizada) => new()
    {
        Id = id,
        UserName = $"{id}@veltika.test",
        Email = $"{id}@veltika.test",
        Nombre = "Usuario",
        Apellido = "Prueba",
        EmpresaId = 1,
        Estado = true,
        FechaAlta = DateTime.UtcNow,
        BienvenidaVisualizada = bienvenidaVisualizada
    };
}
