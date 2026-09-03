using System.Globalization;
using System.Runtime.CompilerServices;

namespace saas.Configuracion
{
    internal static class CulturaAplicacion
    {
        [ModuleInitializer]
        internal static void Inicializar()
        {
            var culturaArgentina = new CultureInfo("es-AR");

            CultureInfo.DefaultThreadCurrentCulture = culturaArgentina;
            CultureInfo.DefaultThreadCurrentUICulture = culturaArgentina;
        }
    }
}
