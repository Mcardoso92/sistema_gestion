using saas.Services;

namespace saas.Tests;

public class NotificacionNovedadServiceTests
{
    private readonly NotificacionNovedadService _service = new();

    [Fact]
    public void CalcularCantidad_SinRevisionConsideraTodosLosAvisosNuevos()
    {
        OrigenNotificacion[] avisos =
        {
            new(1, new DateTime(2026, 9, 12, 10, 0, 0)),
            new(1, new DateTime(2026, 9, 12, 11, 0, 0))
        };

        int resultado = _service.CalcularCantidad(
            avisos,
            new Dictionary<int, DateTime>());

        Assert.Equal(2, resultado);
    }

    [Fact]
    public void CalcularCantidad_ExcluyeAvisosAnterioresOIgualesALaRevision()
    {
        DateTime revision = new(2026, 9, 12, 11, 0, 0);
        OrigenNotificacion[] avisos =
        {
            new(1, revision.AddMinutes(-1)),
            new(1, revision),
            new(1, revision.AddMinutes(1))
        };

        int resultado = _service.CalcularCantidad(
            avisos,
            new Dictionary<int, DateTime> { [1] = revision });

        Assert.Equal(1, resultado);
    }

    [Fact]
    public void CalcularCantidad_AplicaLaRevisionDeCadaEmpresa()
    {
        DateTime revisionEmpresaUno = new(2026, 9, 12, 12, 0, 0);
        DateTime revisionEmpresaDos = new(2026, 9, 12, 9, 0, 0);
        OrigenNotificacion[] avisos =
        {
            new(1, new DateTime(2026, 9, 12, 11, 0, 0)),
            new(2, new DateTime(2026, 9, 12, 11, 0, 0)),
            new(3, new DateTime(2026, 9, 12, 8, 0, 0))
        };

        int resultado = _service.CalcularCantidad(
            avisos,
            new Dictionary<int, DateTime>
            {
                [1] = revisionEmpresaUno,
                [2] = revisionEmpresaDos
            });

        Assert.Equal(2, resultado);
    }
}
