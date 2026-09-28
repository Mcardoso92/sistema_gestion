namespace saas.ViewModel
{
    public class EncabezadoOrdenableVM
    {
        public string Texto { get; set; } = string.Empty;
        public string Columna { get; set; } = string.Empty;
        public string ColumnaActual { get; set; } = string.Empty;
        public string DireccionActual { get; set; } = "asc";
        public string ParametroDireccion { get; set; } = "direccion";
        public string Clase { get; set; } = string.Empty;
    }
}
