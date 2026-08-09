namespace saas.ViewModel
{
    public class VentaIndexItemVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string ClienteNombre { get; set; } = "Cliente ocasional";

        public string UsuarioNombre { get; set; } = null!;

        public string EmpresaNombre { get; set; } = null!;

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public int TotalUnidades { get; set; }
    }
}