using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, ErrorMessage = "La razón social no puede superar los 150 caracteres.")]
        public string RazonSocial { get; set; } = null!;

        [StringLength(150, ErrorMessage = "El nombre de fantasía no puede superar los 150 caracteres.")]
        public string? NombreFantasia { get; set; }

        [StringLength(11, ErrorMessage = "El CUIT no puede superar los 11 dígitos.")]
        public string? CUIT { get; set; }

        [EmailAddress(ErrorMessage = "El email ingresado no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres.")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(100, ErrorMessage = "La localidad no puede superar los 100 caracteres.")]
        public string? Localidad { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede superar los 100 caracteres.")]
        public string? Provincia { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede superar los 20 caracteres.")]
        public string? CodigoPostal { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaAlta { get; set; }

        public int EmpresaId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;
    }
}