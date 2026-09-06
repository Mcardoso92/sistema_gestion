namespace saas.Services
{
    public static class CuitValidator
    {
        public static string? Normalizar(string? cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit))
            {
                return null;
            }

            return new string(cuit.Where(char.IsDigit).ToArray());
        }

        public static bool EsValido(string? cuit)
        {
            string? normalizado = Normalizar(cuit);

            if (normalizado?.Length != 11)
            {
                return false;
            }

            int[] multiplicadores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < multiplicadores.Length; i++)
            {
                suma += (normalizado[i] - '0') * multiplicadores[i];
            }

            int digitoVerificador = 11 - suma % 11;

            if (digitoVerificador == 11)
            {
                digitoVerificador = 0;
            }
            else if (digitoVerificador == 10)
            {
                digitoVerificador = 9;
            }

            return digitoVerificador == normalizado[10] - '0';
        }

        public static bool TieneFormatoCuit(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            string sinSeparadores = new string(valor
                .Where(c => c != '-' && !char.IsWhiteSpace(c))
                .ToArray());

            return sinSeparadores.Length == 11 &&
                   sinSeparadores.All(char.IsDigit);
        }

        public static string? Formatear(string? cuit)
        {
            string? normalizado = Normalizar(cuit);

            if (normalizado?.Length != 11)
            {
                return cuit;
            }

            return $"{normalizado[..2]}-{normalizado.Substring(2, 8)}-{normalizado[10]}";
        }

        public static string? FormatearSiEsCuit(string? valor)
        {
            return TieneFormatoCuit(valor) && EsValido(valor)
                ? Formatear(valor)
                : valor;
        }
    }
}
