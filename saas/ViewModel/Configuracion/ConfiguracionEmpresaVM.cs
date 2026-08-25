using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace saas.ViewModel.Configuracion
{
    public class ConfiguracionEmpresaVM
    {
        [Required(ErrorMessage = "Debe seleccionar una empresa.")]
        public int? EmpresaId { get; set; }

        public string EmpresaNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string? Cuit { get; set; }

        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La moneda es obligatoria.")]
        public string Moneda { get; set; } = "ARS";

        [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
        public decimal IvaPorcentaje { get; set; } = 21;

        [Display(Name = "Monto para considerar una venta importante")]
        [Range(0.01, 999999999999, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal? MontoVentaImportante { get; set; }

        public string? LogoRuta { get; set; }
        [Display(Name = "Nuevo logo")]
        public IFormFile? LogoArchivo { get; set; }

        public bool EliminarLogo { get; set; }

        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Monedas { get; set; } = new List<SelectListItem>();
    }
}