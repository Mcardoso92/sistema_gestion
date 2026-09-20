using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace saas.Helpers;

public static class NavegacionContextual
{
    // Conserva la ruta local completa y sus filtros para que una vista pueda volver al origen exacto.
    public static string ObtenerReturnUrl(HttpRequest request) =>
        $"{request.PathBase}{request.Path}{request.QueryString}";

    // Acepta únicamente destinos internos para impedir redirecciones abiertas.
    public static string? ObtenerReturnUrlLocal(IUrlHelper? url, string? returnUrl) =>
        url?.IsLocalUrl(returnUrl) is true ? returnUrl : null;
}
