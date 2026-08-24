using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class ConfiguracionEmpresa
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

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
        [StringLength(3)]
        public string Moneda { get; set; } = "ARS";

        [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
        public decimal IvaPorcentaje { get; set; } = 21;

        [Range(0.01, 999999999999, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal? MontoVentaImportante { get; set; }

        [StringLength(250)]
        public string? LogoRuta { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;
    }
}