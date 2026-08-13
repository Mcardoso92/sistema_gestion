namespace saas.ViewModel
{
    public class ProveedorDetailsVM
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = null!;
        public string? NombreFantasia { get; set; }
        public string? CUIT { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Observaciones { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public string EmpresaNombre { get; set; } = null!;
    }
}