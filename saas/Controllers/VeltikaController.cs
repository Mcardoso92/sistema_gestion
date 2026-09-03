using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace saas.Controllers
{
    public abstract class VeltikaController : Controller
    {
        private const int LongitudMaximaBusqueda = 100;

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            string busqueda =
                context.HttpContext.Request.Query["busqueda"].ToString();

            if (busqueda.Length > LongitudMaximaBusqueda)
            {
                TempData["Error"] =
                    $"La búsqueda no puede superar los {LongitudMaximaBusqueda} caracteres.";

                context.Result = RedirectToAction(
                    context.RouteData.Values["action"]?.ToString(),
                    context.RouteData.Values["controller"]?.ToString());

                return;
            }

            await next();
        }
    }
}
