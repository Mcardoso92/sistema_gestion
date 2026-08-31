using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.ProductoImportacion
{
    public class ProductoImportacionArchivoVM
    {
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "Seleccione un archivo para importar.")]
        public IFormFile? Archivo { get; set; }
    }
}
