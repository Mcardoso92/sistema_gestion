using Microsoft.AspNetCore.Http;

namespace saas.Services
{
    public interface IImagenService
    {
        Task<ResultadoImagen> GuardarAsync(IFormFile archivo, int empresaId, string carpeta, string? identificador = null, string? rutaAnterior = null);

        void Eliminar(string? ruta);
    }

    public class ResultadoImagen
    {
        public bool Exito { get; set; }

        public string? Ruta { get; set; }

        public string? Error { get; set; }
    }
}