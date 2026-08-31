using Microsoft.EntityFrameworkCore;
using saas.Data;

namespace saas.Tests;

internal static class TestDbContextFactory
{
    // Crea una base independiente para que cada prueba sea repetible y no modifique Saas_DB.
    public static SaasDbContext Crear()
    {
        DbContextOptions<SaasDbContext> options = new DbContextOptionsBuilder<SaasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SaasDbContext(options);
    }
}
