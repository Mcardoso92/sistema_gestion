using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace saas.Configuracion
{
    public sealed class HtmlDecimalModelBinderProvider : IModelBinderProvider
    {
        private static readonly IModelBinder Binder =
            new HtmlDecimalModelBinder();

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Type tipo = context.Metadata.ModelType;

            return tipo == typeof(decimal) || tipo == typeof(decimal?)
                ? Binder
                : null;
        }
    }
}
