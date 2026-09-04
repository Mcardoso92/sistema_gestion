using saas.Services;

namespace saas.Tests;

public class TextoPresentacionTests
{
    [Theory]
    [InlineData("SuperAdmin", "Superadministrador")]
    [InlineData("AdminEmpresa", "Administrador de empresa")]
    [InlineData("OtroRol", "OtroRol")]
    [InlineData(null, "")]
    public void Rol_DevuelveNombreAmigableSinModificarElValorInterno(string? rol, string esperado)
    {
        Assert.Equal(esperado, TextoPresentacion.Rol(rol));
    }
}
