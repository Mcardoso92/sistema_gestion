using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace saas.Services
{
    public class ImagenService : IImagenService
    {
        private const long TamanioMaximo = 2 * 1024 * 1024;
        private const int DimensionMaximaProducto = 1200;
        private const int DimensionMaximaUsuario = 500;
        private const int DimensionMaximaLogo = 800;
        private const int CalidadWebp = 80;
        private readonly IWebHostEnvironment _environment;

        public ImagenService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ResultadoImagen> GuardarAsync(IFormFile archivo, int empresaId, string carpeta, string? identificador = null, string? rutaAnterior = null)
        {
            if (empresaId <= 0)
            {
                return new ResultadoImagen
                {
                    Error = "La empresa no es válida."
                };
            }

            if (archivo == null || archivo.Length == 0)
            {
                return new ResultadoImagen
                {
                    Error = "Debe seleccionar una imagen."
                };
            }

            if (archivo.Length > TamanioMaximo)
            {
                return new ResultadoImagen
                {
                    Error = "La imagen no puede superar los 2 MB."
                };
            }

            string? carpetaValida = carpeta.ToLowerInvariant() switch
            {
                "productos" => "productos",
                "usuarios" => "usuarios",
                "logos" => "logos",
                _ => null
            };

            if (carpetaValida == null)
            {
                return new ResultadoImagen
                {
                    Error = "La carpeta de imágenes no es válida."
                };
            }

            string? extension = await ObtenerExtensionAsync(archivo);

            if (extension == null)
            {
                return new ResultadoImagen
                {
                    Error = "El archivo debe ser una imagen JPEG, PNG o WebP válida."
                };
            }

            string webRoot = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            string directorio = Path.Combine(webRoot, "uploads", "empresas", empresaId.ToString(), carpetaValida);
            Directory.CreateDirectory(directorio);

            string nombreBase = carpetaValida switch
            {
                "productos" => "producto",
                "usuarios" => "usuario",
                "logos" => "logo",
                _ => "imagen"
            };

            string identificadorSeguro = string.IsNullOrWhiteSpace(identificador)
                ? ""
                : new string(identificador.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());

            if (!string.IsNullOrWhiteSpace(identificadorSeguro))
            {
                nombreBase = $"{nombreBase}-{identificadorSeguro}";
            }

            int dimensionMaxima = carpetaValida switch
            {
                "productos" => DimensionMaximaProducto,
                "usuarios" => DimensionMaximaUsuario,
                "logos" => DimensionMaximaLogo,
                _ => DimensionMaximaProducto
            };

            string nombreArchivo = $"{nombreBase}-{Guid.NewGuid():N}.webp";
            string rutaFisica = Path.Combine(directorio, nombreArchivo);

            try
            {
                await using Stream origen = archivo.OpenReadStream();
                using SKBitmap imagenOriginal = SKBitmap.Decode(origen) ?? throw new InvalidOperationException();
                SKBitmap? imagenRedimensionada = null;

                int anchoNuevo = imagenOriginal.Width;
                int altoNuevo = imagenOriginal.Height;

                if (imagenOriginal.Width > dimensionMaxima || imagenOriginal.Height > dimensionMaxima)
                {
                    decimal escala = Math.Min((decimal)dimensionMaxima / imagenOriginal.Width, (decimal)dimensionMaxima / imagenOriginal.Height);
                    anchoNuevo = (int)Math.Round(imagenOriginal.Width * escala);
                    altoNuevo = (int)Math.Round(imagenOriginal.Height * escala);
                    imagenRedimensionada = imagenOriginal.Resize(new SKImageInfo(anchoNuevo, altoNuevo), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                }

                using SKBitmap imagenFinal = imagenRedimensionada ?? imagenOriginal.Copy();
                using SKImage imagenSalida = SKImage.FromBitmap(imagenFinal);
                using SKData datosImagen = imagenSalida.Encode(SKEncodedImageFormat.Webp, CalidadWebp);

                await using FileStream destino = new FileStream(rutaFisica, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                datosImagen.SaveTo(destino);
            }
            catch
            {
                if (File.Exists(rutaFisica))
                {
                    File.Delete(rutaFisica);
                }

                return new ResultadoImagen
                {
                    Error = "No fue posible procesar y guardar la imagen."
                };
            }

            Eliminar(rutaAnterior);

            return new ResultadoImagen
            {
                Exito = true,
                Ruta = $"/uploads/empresas/{empresaId}/{carpetaValida}/{nombreArchivo}"
            };
        }

        public void Eliminar(string? ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta) ||
                !ruta.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string webRoot = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            string raizUploads = Path.GetFullPath(Path.Combine(webRoot, "uploads")) +
                Path.DirectorySeparatorChar;

            string rutaRelativa = ruta
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            string rutaFisica = Path.GetFullPath(Path.Combine(webRoot, rutaRelativa));

            if (!rutaFisica.StartsWith(raizUploads, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                if (File.Exists(rutaFisica))
                {
                    File.Delete(rutaFisica);
                }
            }
            catch
            {
                // La eliminación no debe interrumpir la operación principal.
            }
        }

        private static async Task<string?> ObtenerExtensionAsync(IFormFile archivo)
        {
            byte[] firma = new byte[12];

            await using var stream = archivo.OpenReadStream();
            int bytesLeidos = await stream.ReadAsync(firma.AsMemory(0, firma.Length));

            if (bytesLeidos >= 3 &&
                firma[0] == 0xFF &&
                firma[1] == 0xD8 &&
                firma[2] == 0xFF)
            {
                return ".jpg";
            }

            if (bytesLeidos >= 8 &&
                firma[0] == 0x89 &&
                firma[1] == 0x50 &&
                firma[2] == 0x4E &&
                firma[3] == 0x47 &&
                firma[4] == 0x0D &&
                firma[5] == 0x0A &&
                firma[6] == 0x1A &&
                firma[7] == 0x0A)
            {
                return ".png";
            }

            if (bytesLeidos >= 12 &&
                firma[0] == 0x52 &&
                firma[1] == 0x49 &&
                firma[2] == 0x46 &&
                firma[3] == 0x46 &&
                firma[8] == 0x57 &&
                firma[9] == 0x45 &&
                firma[10] == 0x42 &&
                firma[11] == 0x50)
            {
                return ".webp";
            }

            return null;
        }
    }
}