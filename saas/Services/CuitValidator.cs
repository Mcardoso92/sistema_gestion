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
    }
}
