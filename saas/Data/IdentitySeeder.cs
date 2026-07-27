using Microsoft.AspNetCore.Identity;
using saas.Models;

namespace saas.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<Usuario>>();
            var context = services.GetRequiredService<SaasDbContext>();

            // Verificar si existe la empresa principal
            var empresa = context.Empresas.FirstOrDefault();

            if (empresa == null)
            {
                empresa = new Empresa
                {
                    Nombre = "Veltika Demo",
                    Estado = true,
                    FechaAlta = DateTime.Now
                };

                context.Empresas.Add(empresa);
                await context.SaveChangesAsync();
            }

            // Roles
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

            // Usuario administrador
            var email = "admin@veltika.com";

            var usuario = await userManager.FindByEmailAsync(email);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    UserName = email,
                    Email = email,

                    Nombre = "Administrador",
                    Apellido = "General",

                    Estado = true,
                    FechaAlta = DateTime.Now,

                    EmpresaId = empresa.Id
                };

                var resultado = await userManager.CreateAsync(usuario, "admin123");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(usuario, "SuperAdmin");
                }
            }
        }
    }
}