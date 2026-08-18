using saas.Models.Enums;
namespace saas.ViewModel

{
    public class MedioPagoOpcionSimpleVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public TipoMedioPago Tipo { get; set; }
    }
}