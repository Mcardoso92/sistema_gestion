using Microsoft.AspNetCore.Http;
using saas.Helpers;

namespace saas.Tests;

public class NavegacionContextualTests
{
    [Fact]
    public void ObtenerReturnUrl_ConservaRutaYTodosLosParametrosDeConsulta()
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.PathBase = "/veltika";
        contexto.Request.Path = "/Producto";
        contexto.Request.QueryString = new QueryString("?categoriaId=2&busqueda=yerba&pagina=3");

        string returnUrl = NavegacionContextual.ObtenerReturnUrl(contexto.Request);

        Assert.Equal("/veltika/Producto?categoriaId=2&busqueda=yerba&pagina=3", returnUrl);
    }
}
