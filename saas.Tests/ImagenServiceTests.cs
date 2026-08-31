using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using saas.Services;
using SkiaSharp;

namespace saas.Tests;

public class ImagenServiceTests
{
    [Fact]
    public async Task GuardarAsync_ConvierteProductoARedimensionWebp()
    {
        // Verifica que una imagen grande se convierta a WebP sin superar los 1200 píxeles.
        string raiz = CrearRaizTemporal();

        try
        {
            var service = new ImagenService(CrearEntorno(raiz));
            IFormFile archivo = CrearImagenPng(1600, 800);
            ResultadoImagen resultado = await service.GuardarAsync(archivo, 1, "productos", "15");

            Assert.True(resultado.Exito);
            Assert.EndsWith(".webp", resultado.Ruta);

            string rutaFisica = ObtenerRutaFisica(raiz, resultado.Ruta!);
            using SKBitmap imagenGuardada = SKBitmap.Decode(rutaFisica);
            Assert.Equal(1200, imagenGuardada.Width);
            Assert.Equal(600, imagenGuardada.Height);
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    [Fact]
    public async Task GuardarAsync_RechazaUnArchivoQueNoEsImagen()
    {
        // Impide guardar archivos arbitrarios aunque intenten cargarse desde un campo de imagen.
        string raiz = CrearRaizTemporal();

        try
        {
            var service = new ImagenService(CrearEntorno(raiz));
            using var contenido = new MemoryStream("contenido no válido"u8.ToArray());
            IFormFile archivo = new FormFile(contenido, 0, contenido.Length, "archivo", "archivo.jpg");
            ResultadoImagen resultado = await service.GuardarAsync(archivo, 1, "productos");

            Assert.False(resultado.Exito);
            Assert.Equal("El archivo debe ser una imagen JPEG, PNG o WebP válida.", resultado.Error);
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    [Fact]
    public async Task GuardarAsync_EliminaLaImagenAnteriorDespuesDelReemplazo()
    {
        // Evita acumular archivos sin uso cuando una empresa reemplaza una imagen existente.
        string raiz = CrearRaizTemporal();

        try
        {
            var service = new ImagenService(CrearEntorno(raiz));
            IFormFile primeraImagen = CrearImagenPng(400, 400);
            ResultadoImagen primera = await service.GuardarAsync(primeraImagen, 1, "productos", "15");
            string rutaAnterior = ObtenerRutaFisica(raiz, primera.Ruta!);
            Assert.True(File.Exists(rutaAnterior));

            IFormFile segundaImagen = CrearImagenPng(500, 500);
            ResultadoImagen segunda = await service.GuardarAsync(segundaImagen, 1, "productos", "15", primera.Ruta);

            Assert.True(segunda.Exito);
            Assert.False(File.Exists(rutaAnterior));
            Assert.True(File.Exists(ObtenerRutaFisica(raiz, segunda.Ruta!)));
        }
        finally
        {
            Directory.Delete(raiz, true);
        }
    }

    private static string CrearRaizTemporal()
    {
        string raiz = Path.Combine(Path.GetTempPath(), "veltika-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(raiz, "wwwroot"));
        return raiz;
    }

    private static IWebHostEnvironment CrearEntorno(string raiz)
    {
        return new EntornoWebPrueba { ContentRootPath = raiz, WebRootPath = Path.Combine(raiz, "wwwroot") };
    }

    private static IFormFile CrearImagenPng(int ancho, int alto)
    {
        using var bitmap = new SKBitmap(ancho, alto);
        bitmap.Erase(SKColors.CornflowerBlue);
        using SKImage imagen = SKImage.FromBitmap(bitmap);
        using SKData datos = imagen.Encode(SKEncodedImageFormat.Png, 100);
        var stream = new MemoryStream(datos.ToArray());
        return new FormFile(stream, 0, stream.Length, "imagen", "imagen.png");
    }

    private static string ObtenerRutaFisica(string raiz, string rutaWeb)
    {
        return Path.Combine(raiz, "wwwroot", rutaWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
    }

    private sealed class EntornoWebPrueba : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "saas.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = null!;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
