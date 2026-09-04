namespace saas.Services
{
    public static class TextoPresentacion
    {
        public static string Rol(string? rol)
        {
            return rol switch
            {
                "SuperAdmin" => "Superadministrador",
                "AdminEmpresa" => "Administrador de empresa",
                _ => rol ?? string.Empty
            };
        }

        public static string ValorEnum<TEnum>(TEnum valor) where TEnum : struct, Enum
        {
            string nombre = valor.ToString();
            System.Reflection.FieldInfo? campo = typeof(TEnum).GetField(nombre);
            var atributo = campo == null
                ? null
                : Attribute.GetCustomAttribute(
                    campo,
                    typeof(System.ComponentModel.DataAnnotations.DisplayAttribute))
                    as System.ComponentModel.DataAnnotations.DisplayAttribute;

            return atributo?.GetName() ?? nombre;
        }
    }
}
