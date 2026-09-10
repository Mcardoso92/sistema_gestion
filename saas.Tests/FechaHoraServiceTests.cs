using Microsoft.Extensions.Options;
using saas.Services;
using saas.Settings;

namespace saas.Tests;

public class FechaHoraServiceTests
{
    private static readonly DateTimeOffset InstanteUtc =
        new(2026, 9, 9, 2, 2, 8, TimeSpan.Zero);

    [Fact]
    public void HoraLocalAhora_ConvierteUtcABuenosAires()
    {
        var service = CrearService();

        DateTime resultado = service.HoraLocalAhora;

        Assert.Equal(new DateTime(2026, 9, 8, 23, 2, 8), resultado);
        Assert.Equal(DateTimeKind.Unspecified, resultado.Kind);
    }

    [Fact]
    public void ConvertirAUtc_ConvierteHoraBuenosAiresAUtc()
    {
        var service = CrearService();

        DateTime resultado = service.ConvertirAUtc(
            new DateTime(2026, 9, 8, 23, 2, 8));

        Assert.Equal(new DateTime(2026, 9, 9, 2, 2, 8, DateTimeKind.Utc), resultado);
    }

    [Fact]
    public void FechaLocalHoy_UsaElDiaDeBuenosAires()
    {
        var service = CrearService();

        Assert.Equal(new DateTime(2026, 9, 8), service.FechaLocalHoy);
    }

    private static FechaHoraService CrearService() =>
        new(
            new TimeProviderPrueba(InstanteUtc),
            Options.Create(new ZonaHorariaSettings
            {
                Id = "America/Argentina/Buenos_Aires"
            }));

    private sealed class TimeProviderPrueba(DateTimeOffset utcAhora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcAhora;
    }
}
