using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace saas.Models
{
    public class CajaMedioPago
    {
        public int Id { get; set; }

        public int CajaId { get; set; }

        public int MedioPagoId { get; set; }

        [ValidateNever]
        public Caja Caja { get; set; } = null!;

        [ValidateNever]
        public MedioPago MedioPago { get; set; } = null!;
    }
}