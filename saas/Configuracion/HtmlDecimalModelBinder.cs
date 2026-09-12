using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace saas.Configuracion
{
    public sealed class HtmlDecimalModelBinder : IModelBinder
    {
        private static readonly CultureInfo CulturaArgentina =
            CultureInfo.GetCultureInfo("es-AR");

        private const NumberStyles EstilosSinMiles =
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingWhite |
            NumberStyles.AllowTrailingWhite;

        private const NumberStyles EstilosArgentinos =
            EstilosSinMiles |
            NumberStyles.AllowThousands;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var resultadoValor = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            if (resultadoValor == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(
                bindingContext.ModelName,
                resultadoValor);

            string? texto = resultadoValor.FirstValue;

            if (string.IsNullOrWhiteSpace(texto))
            {
                if (Nullable.GetUnderlyingType(bindingContext.ModelType) != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(null);
                }

                return Task.CompletedTask;
            }

            if (TryParse(texto, out decimal valor))
            {
                bindingContext.Result = ModelBindingResult.Success(valor);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Ingrese un importe válido con hasta dos decimales.");

            return Task.CompletedTask;
        }

        public static bool TryParse(string texto, out decimal valor)
        {
            bool usaComaDecimal = texto.Contains(',');
            CultureInfo cultura = usaComaDecimal
                ? CulturaArgentina
                : CultureInfo.InvariantCulture;
            NumberStyles estilos = usaComaDecimal
                ? EstilosArgentinos
                : EstilosSinMiles;

            if (!decimal.TryParse(texto, estilos, cultura, out valor))
            {
                return false;
            }

            int escala = (decimal.GetBits(valor)[3] >> 16) & 0x7F;
            return escala <= 2;
        }
    }
}
