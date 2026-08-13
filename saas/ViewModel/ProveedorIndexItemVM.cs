namespace saas.ViewModel
{
    public class ProveedorIndexItemVM
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = null!;
        public string? NombreFantasia { get; set; }
        public string? CUIT { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string EmpresaNombre { get; set; } = null!;
        public bool Estado { get; set; }
    }
}