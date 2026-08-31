using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using saas.Data;

namespace saas.Tests;

internal static class TestDbContextFactory
{
    // Crea una base independiente para que cada prueba sea repetible y no modifique Saas_DB.
    public static SaasDbContext Crear()
    {
        DbContextOptions<SaasDbContext> options = new DbContextOptionsBuilder<SaasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            // La base en memoria no implementa transacciones reales, pero permite verificar el resultado lógico del servicio.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SaasDbContext(options);
    }
}
