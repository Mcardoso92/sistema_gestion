using saas.Models;
using saas.Models.Enums;

namespace saas.Helpers
{
    public static class ConfiguracionInicialEmpresa
    {
        public static List<MedioPago> CrearMediosPagoPredeterminados(
            int empresaId,
            DateTime fechaAlta)
        {
            return new List<MedioPago>
            {
                new MedioPago
                {
                    Nombre = "Efectivo",
                    Tipo = TipoMedioPago.Efectivo,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                },
                new MedioPago
                {
                    Nombre = "Transferencia",
                    Tipo = TipoMedioPago.Transferencia,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                },
                new MedioPago
                {
                    Nombre = "Tarjeta de débito",
                    Tipo = TipoMedioPago.TarjetaDebito,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                },
                new MedioPago
                {
                    Nombre = "Tarjeta de crédito",
                    Tipo = TipoMedioPago.TarjetaCredito,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                },
                new MedioPago
                {
                    Nombre = "QR",
                    Tipo = TipoMedioPago.QR,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                },
                new MedioPago
                {
                    Nombre = "Cheque",
                    Tipo = TipoMedioPago.Cheque,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                }
            };
        }
    }
}