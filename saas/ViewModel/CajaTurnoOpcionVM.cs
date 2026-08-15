namespace saas.ViewModel
{
    public class CajaTurnoOpcionVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public decimal FondoFijo { get; set; }

        public bool Disponible { get; set; }
    }
}