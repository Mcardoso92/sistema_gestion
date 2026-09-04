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
    }
}
