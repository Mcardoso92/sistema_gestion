using System.Linq.Expressions;

namespace saas.Helpers
{
    /// <summary>
    /// Centraliza las reglas comunes del ordenamiento para que todos los listados
    /// interpreten columnas y direcciones de la misma manera.
    /// </summary>
    public static class OrdenamientoConsulta
    {
        public const string Ascendente = "asc";
        public const string Descendente = "desc";

        public static string NormalizarColumna(string? columna, string columnaPredeterminada, params string[] columnasPermitidas)
        {
            string columnaNormalizada = columna?.Trim().ToLowerInvariant() ?? string.Empty;

            return columnasPermitidas.Contains(columnaNormalizada, StringComparer.OrdinalIgnoreCase)
                ? columnaNormalizada
                : columnaPredeterminada;
        }

        public static string NormalizarDireccion(string? direccion)
        {
            return string.Equals(direccion, Descendente, StringComparison.OrdinalIgnoreCase)
                ? Descendente
                : Ascendente;
        }

        public static IOrderedQueryable<T> Aplicar<T, TClave>(
            this IQueryable<T> consulta,
            Expression<Func<T, TClave>> selector,
            string direccion)
        {
            return direccion == Descendente
                ? consulta.OrderByDescending(selector)
                : consulta.OrderBy(selector);
        }
    }
}
