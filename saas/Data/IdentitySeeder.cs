using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // ==========================
            // EMPRESAS
            // ==========================

            var empresaVeltika = await context.Empresas.FirstOrDefaultAsync(e => e.Nombre == "Veltika Demo");

            if (empresaVeltika == null)
            {
                empresaVeltika = new Empresa
                {
                    Nombre = "Veltika Demo",
                    Estado = true,
                    FechaAlta = DateTime.Now
                };

                context.Empresas.Add(empresaVeltika);
            }

            var empresaKiosko = await context.Empresas.FirstOrDefaultAsync(e => e.Nombre == "Kiosko Don José");

            if (empresaKiosko == null)
            {
                empresaKiosko = new Empresa
                {
                    Nombre = "Kiosko Don José",
                    Estado = true,
                    FechaAlta = DateTime.Now
                };

                context.Empresas.Add(empresaKiosko);
            }

            var empresaFerreteria = await context.Empresas.FirstOrDefaultAsync(e => e.Nombre == "Ferretería Central");

            if (empresaFerreteria == null)
            {
                empresaFerreteria = new Empresa
                {
                    Nombre = "Ferretería Central",
                    Estado = true,
                    FechaAlta = DateTime.Now
                };

                context.Empresas.Add(empresaFerreteria);
            }

            await context.SaveChangesAsync();

            // ==========================
            // ROLES
            // ==========================

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

            // ==========================
            // SUPER ADMIN
            // ==========================

            var superAdmin = await userManager.FindByEmailAsync("admin@veltika.com");

            if (superAdmin == null)
            {
                superAdmin = new Usuario
                {
                    UserName = "admin@veltika.com",
                    Email = "admin@veltika.com",

                    Nombre = "Administrador",
                    Apellido = "General",

                    EmpresaId = empresaVeltika.Id,

                    Estado = true,
                    FechaAlta = DateTime.Now,
                    ImagenPerfil = ""
                };

                var resultado = await userManager.CreateAsync(superAdmin, "admin123");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            // ==========================
            // ADMIN KIOSKO
            // ==========================

            var adminKiosko = await userManager.FindByEmailAsync("admin@kiosko.com");

            if (adminKiosko == null)
            {
                adminKiosko = new Usuario
                {
                    UserName = "admin@kiosko.com",
                    Email = "admin@kiosko.com",

                    Nombre = "Juan",
                    Apellido = "Pérez",

                    EmpresaId = empresaKiosko.Id,

                    Estado = true,
                    FechaAlta = DateTime.Now,
                    ImagenPerfil = ""
                };

                var resultado = await userManager.CreateAsync(adminKiosko, "admin123");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminKiosko, "AdminEmpresa");
                }
            }

            // ==========================
            // ADMIN FERRETERÍA
            // ==========================

            var adminFerreteria = await userManager.FindByEmailAsync("admin@ferreteria.com");

            if (adminFerreteria == null)
            {
                adminFerreteria = new Usuario
                {
                    UserName = "admin@ferreteria.com",
                    Email = "admin@ferreteria.com",

                    Nombre = "María",
                    Apellido = "Gómez",

                    EmpresaId = empresaFerreteria.Id,

                    Estado = true,
                    FechaAlta = DateTime.Now,
                    ImagenPerfil = ""
                };

                var resultado = await userManager.CreateAsync(adminFerreteria, "admin123");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminFerreteria, "AdminEmpresa");
                }
            }
        }
    }
}