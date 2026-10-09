using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.ProductoActualizacionMasiva
{
    public class ProductoActualizacionMasivaVM
    {
        public int EmpresaId { get; set; }
        public int? CategoriaId { get; set; }

        [StringLength(100, ErrorMessage = "La búsqueda no puede superar los 100 caracteres.")]
        public string? Busqueda { get; set; }

        [Required(ErrorMessage = "Seleccione qué precio desea modificar.")]
        public CampoAjustePrecioProducto Campo { get; set; } = CampoAjustePrecioProducto.PrecioVenta;

        [Required(ErrorMessage = "Seleccione el tipo de ajuste.")]
        public TipoAjustePrecioProducto TipoAjuste { get; set; } = TipoAjustePrecioProducto.Porcentaje;

        [Range(-999999999.99, 999999999.99, ErrorMessage = "El ajuste ingresado está fuera del rango permitido.")]
        public decimal ValorAjuste { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
