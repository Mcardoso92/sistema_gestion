namespace saas.Services
{
    public interface IFechaHoraService
    {
        DateTime UtcAhora { get; }
        DateTime HoraLocalAhora { get; }
        DateTime FechaLocalHoy { get; }

        DateTime ConvertirAHoraLocal(DateTime fechaUtc);
        DateTime? ConvertirAHoraLocal(DateTime? fechaUtc);
        DateTime ConvertirAUtc(DateTime fechaLocal);
    }
}
