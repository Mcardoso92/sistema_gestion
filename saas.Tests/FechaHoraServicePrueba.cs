using saas.Services;

namespace saas.Tests;

internal sealed class FechaHoraServicePrueba : IFechaHoraService
{
    public DateTime UtcAhora { get; init; } =
        new(2026, 9, 9, 2, 2, 8, DateTimeKind.Utc);

    public DateTime HoraLocalAhora => UtcAhora.AddHours(-3);

    public DateTime FechaLocalHoy => HoraLocalAhora.Date;

    public DateTime ConvertirAHoraLocal(DateTime fechaUtc) =>
        DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc).AddHours(-3);

    public DateTime? ConvertirAHoraLocal(DateTime? fechaUtc) =>
        fechaUtc.HasValue
            ? ConvertirAHoraLocal(fechaUtc.Value)
            : null;

    public DateTime ConvertirAUtc(DateTime fechaLocal) =>
        DateTime.SpecifyKind(fechaLocal.AddHours(3), DateTimeKind.Utc);
}
