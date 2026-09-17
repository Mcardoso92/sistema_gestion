using Microsoft.AspNetCore.Identity;

namespace saas.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "SuperAdmin",
                "AdminEmpresa",
                "Empleado"
            };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // Las cuentas administrativas se crean desde flujos controlados.
            // El seeder nunca debe contener ni recrear credenciales conocidas.
        }
    }
}
