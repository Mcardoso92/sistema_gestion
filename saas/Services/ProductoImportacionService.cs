using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel.ProductoImportacion;

namespace saas.Services
{
    public class ProductoImportacionService : IProductoImportacionService
    {
        private const long TamanioMaximoArchivo = 5 * 1024 * 1024;
        private static readonly TimeSpan DuracionVistaPrevia = TimeSpan.FromMinutes(30);
        private readonly SaasDbContext _context;
        private readonly IMemoryCache _cache;

        public ProductoImportacionService(SaasDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public byte[] GenerarPlantilla()
        {
            using var libro = new XLWorkbook();
            IXLWorksheet productos = libro.Worksheets.Add("Productos");
            string[] encabezados = { "Nombre", "CodigoBarra", "Categoria", "PrecioCosto", "PrecioVenta", "StockInicial", "PuntoReposicion", "Descripcion" };

            for (int columna = 0; columna < encabezados.Length; columna++)
            {
                productos.Cell(1, columna + 1).Value = encabezados[columna];
            }

            productos.Row(1).Style.Font.Bold = true;
            productos.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1FF");
            productos.SheetView.FreezeRows(1);
            productos.Column(2).Style.NumberFormat.Format = "@";
            productos.Columns(1, 8).AdjustToContents();
            productos.Column(1).Width = Math.Max(productos.Column(1).Width, 24);
            productos.Column(8).Width = Math.Max(productos.Column(8).Width, 35);

            IXLWorksheet instrucciones = libro.Worksheets.Add("Instrucciones");
            instrucciones.Cell("A1").Value = "Cómo completar la plantilla";
            instrucciones.Cell("A1").Style.Font.Bold = true;
            instrucciones.Cell("A3").Value = "Nombre y PrecioVenta son obligatorios.";
            instrucciones.Cell("A4").Value = "Si Categoria está vacía se utilizará Sin categoría.";
            instrucciones.Cell("A5").Value = "PrecioCosto, StockInicial y PuntoReposicion vacíos se interpretan como 0.";
            instrucciones.Cell("A6").Value = "No cambie los nombres ni el orden de las columnas.";
            instrucciones.Cell("A7").Value = "No agregue imágenes; esta primera versión no las importa.";
            instrucciones.Column(1).AdjustToContents();

            using var flujo = new MemoryStream();
            libro.SaveAs(flujo);
            return flujo.ToArray();
        }

        public async Task<ProductoImportacionVistaPreviaVM> AnalizarAsync(IFormFile archivo, int empresaId, string usuarioId)
        {
            ValidarArchivo(archivo);

            List<CategoriaImportacion> categorias = await _context.Categorias.AsNoTracking().Where(c => c.EmpresaId == empresaId && c.Estado).Select(c => new CategoriaImportacion(c.Id, c.Nombre)).ToListAsync();
            HashSet<string> nombresExistentes = await _context.Productos.AsNoTracking().Where(p => p.EmpresaId == empresaId).Select(p => p.Nombre.ToLower()).ToHashSetAsync();
            HashSet<string> codigosExistentes = await _context.Productos.AsNoTracking().Where(p => p.EmpresaId == empresaId && p.CodigoBarra != null).Select(p => p.CodigoBarra!.ToLower()).ToHashSetAsync();
            CategoriaImportacion? sinCategoria = categorias.FirstOrDefault(c => c.Nombre.Equals("Sin categoría", StringComparison.OrdinalIgnoreCase));

            using Stream flujo = archivo.OpenReadStream();
            using var libro = new XLWorkbook(flujo);
            IXLWorksheet hoja = libro.Worksheets.FirstOrDefault(w => w.Name.Equals("Productos", StringComparison.OrdinalIgnoreCase)) ?? libro.Worksheet(1);
            ValidarEncabezados(hoja);

            var vistaPrevia = new ProductoImportacionVistaPreviaVM { Token = Guid.NewGuid().ToString("N"), EmpresaId = empresaId, NombreArchivo = Path.GetFileName(archivo.FileName) };
            var nombresArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var codigosArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

            for (int numeroFila = 2; numeroFila <= ultimaFila; numeroFila++)
            {
                IXLRow filaExcel = hoja.Row(numeroFila);
                if (filaExcel.Cells(1, 8).All(c => c.IsEmpty()))
                {
                    continue;
                }

                ProductoImportacionFilaVM fila = CrearFila(filaExcel, numeroFila);
                ValidarFila(fila, categorias, sinCategoria, nombresExistentes, codigosExistentes, nombresArchivo, codigosArchivo);
                vistaPrevia.Filas.Add(fila);
            }

            // La vista previa queda ligada a empresa y usuario para impedir confirmar análisis de otra sesión o comercio.
            _cache.Set(ClaveCache(vistaPrevia.Token), new ImportacionTemporal(empresaId, usuarioId, vistaPrevia), DuracionVistaPrevia);
            return vistaPrevia;
        }

        public async Task<int> ImportarAsync(string token, int empresaId, string usuarioId)
        {
            if (!TryObtenerVistaPrevia(token, empresaId, usuarioId, out ProductoImportacionVistaPreviaVM? vistaPrevia))
            {
                throw new InvalidOperationException("La vista previa venció o no pertenece al usuario actual.");
            }

            if (!vistaPrevia!.PuedeImportar)
            {
                throw new InvalidOperationException("La importación contiene errores y no puede confirmarse.");
            }

            string[] nombres = vistaPrevia.Filas.Select(f => f.Nombre.ToLower()).ToArray();
            string[] codigos = vistaPrevia.Filas.Where(f => !string.IsNullOrWhiteSpace(f.CodigoBarra)).Select(f => f.CodigoBarra!.ToLower()).ToArray();
            bool existenDuplicados = await _context.Productos.AnyAsync(p => p.EmpresaId == empresaId && (nombres.Contains(p.Nombre.ToLower()) || p.CodigoBarra != null && codigos.Contains(p.CodigoBarra.ToLower())));

            if (existenDuplicados)
            {
                throw new InvalidOperationException("Uno o más productos ya existen. Analice nuevamente el archivo para ver el detalle.");
            }

            int[] categorias = vistaPrevia.Filas.Select(f => f.CategoriaId!.Value).Distinct().ToArray();
            int categoriasValidas = await _context.Categorias.CountAsync(c => categorias.Contains(c.Id) && c.EmpresaId == empresaId && c.Estado);

            if (categoriasValidas != categorias.Length)
            {
                throw new InvalidOperationException("Una o más categorías dejaron de estar disponibles. Analice nuevamente el archivo.");
            }

            DateTime fecha = DateTime.Now;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (ProductoImportacionFilaVM fila in vistaPrevia.Filas)
                {
                    var producto = new Producto
                    {
                        Nombre = fila.Nombre,
                        CodigoBarra = fila.CodigoBarra,
                        CategoriaId = fila.CategoriaId!.Value,
                        PrecioCosto = fila.PrecioCosto,
                        PrecioVenta = fila.PrecioVenta,
                        Stock = fila.StockInicial,
                        PuntoReposicion = fila.PuntoReposicion,
                        Descripcion = fila.Descripcion,
                        Estado = true,
                        FechaAlta = fecha,
                        EmpresaId = empresaId
                    };

                    _context.Productos.Add(producto);

                    if (fila.StockInicial > 0)
                    {
                        // La relación con Producto permite guardar el alta y su trazabilidad juntas sin depender todavía del ID definitivo.
                        _context.MovimientosStock.Add(new MovimientoStock
                        {
                            Producto = producto,
                            EmpresaId = empresaId,
                            Tipo = TipoMovimientoStock.StockInicial,
                            Cantidad = fila.StockInicial,
                            StockAnterior = 0,
                            StockPosterior = fila.StockInicial,
                            Motivo = "Stock inicial por importación",
                            Fecha = fecha,
                            UsuarioId = usuarioId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                EliminarVistaPrevia(token);
                return vistaPrevia.TotalFilas;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public bool TryObtenerVistaPrevia(string token, int empresaId, string usuarioId, out ProductoImportacionVistaPreviaVM? vistaPrevia)
        {
            bool encontrada = _cache.TryGetValue(ClaveCache(token), out ImportacionTemporal? temporal);
            vistaPrevia = encontrada && temporal!.EmpresaId == empresaId && temporal.UsuarioId == usuarioId ? temporal.VistaPrevia : null;
            return vistaPrevia != null;
        }

        public void EliminarVistaPrevia(string token)
        {
            _cache.Remove(ClaveCache(token));
        }

        private static void ValidarArchivo(IFormFile archivo)
        {
            if (archivo.Length == 0) throw new InvalidDataException("El archivo está vacío.");
            if (archivo.Length > TamanioMaximoArchivo) throw new InvalidDataException("El archivo no puede superar los 5 MB.");
            if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("El archivo debe tener formato Excel .xlsx.");
        }

        private static void ValidarEncabezados(IXLWorksheet hoja)
        {
            string[] esperados = { "Nombre", "CodigoBarra", "Categoria", "PrecioCosto", "PrecioVenta", "StockInicial", "PuntoReposicion", "Descripcion" };

            for (int columna = 1; columna <= esperados.Length; columna++)
            {
                if (!hoja.Cell(1, columna).GetString().Trim().Equals(esperados[columna - 1], StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"La columna {columna} debe llamarse {esperados[columna - 1]}.");
                }
            }
        }

        private static ProductoImportacionFilaVM CrearFila(IXLRow filaExcel, int numeroFila)
        {
            var fila = new ProductoImportacionFilaVM
            {
                NumeroFila = numeroFila,
                Nombre = Texto(filaExcel.Cell(1)),
                CodigoBarra = TextoOpcional(filaExcel.Cell(2)),
                Categoria = TextoOpcional(filaExcel.Cell(3)),
                Descripcion = TextoOpcional(filaExcel.Cell(8))
            };

            fila.PrecioCosto = Decimal(filaExcel.Cell(4), "PrecioCosto", fila.Errores, obligatorio: false);
            fila.PrecioVenta = Decimal(filaExcel.Cell(5), "PrecioVenta", fila.Errores, obligatorio: true);
            fila.StockInicial = Entero(filaExcel.Cell(6), "StockInicial", fila.Errores);
            fila.PuntoReposicion = Entero(filaExcel.Cell(7), "PuntoReposicion", fila.Errores);
            return fila;
        }

        private static void ValidarFila(ProductoImportacionFilaVM fila, List<CategoriaImportacion> categorias, CategoriaImportacion? sinCategoria, HashSet<string> nombresExistentes, HashSet<string> codigosExistentes, HashSet<string> nombresArchivo, HashSet<string> codigosArchivo)
        {
            if (string.IsNullOrWhiteSpace(fila.Nombre))
            {
                fila.Errores.Add("El nombre es obligatorio.");
            }
            else
            {
                if (fila.Nombre.Length > 100) fila.Errores.Add("El nombre no puede superar los 100 caracteres.");
                if (!nombresArchivo.Add(fila.Nombre)) fila.Errores.Add("El nombre está repetido dentro del archivo.");
                if (nombresExistentes.Contains(fila.Nombre.ToLower())) fila.Errores.Add("Ya existe un producto con ese nombre en la empresa.");
            }

            if (!string.IsNullOrWhiteSpace(fila.CodigoBarra))
            {
                if (fila.CodigoBarra.Length > 100) fila.Errores.Add("El código de barras no puede superar los 100 caracteres.");
                if (!codigosArchivo.Add(fila.CodigoBarra)) fila.Errores.Add("El código de barras está repetido dentro del archivo.");
                if (codigosExistentes.Contains(fila.CodigoBarra.ToLower())) fila.Errores.Add("El código de barras ya existe en la empresa.");
            }

            CategoriaImportacion? categoria = string.IsNullOrWhiteSpace(fila.Categoria) ? sinCategoria : categorias.FirstOrDefault(c => c.Nombre.Equals(fila.Categoria, StringComparison.OrdinalIgnoreCase));
            if (categoria == null)
            {
                fila.Errores.Add(string.IsNullOrWhiteSpace(fila.Categoria) ? "La empresa no posee la categoría base Sin categoría." : "La categoría indicada no existe o está inactiva.");
            }
            else
            {
                fila.CategoriaId = categoria.Id;
                fila.Categoria = categoria.Nombre;
            }

            if (fila.Descripcion?.Length > 500) fila.Errores.Add("La descripción no puede superar los 500 caracteres.");
        }

        private static decimal Decimal(IXLCell celda, string campo, List<string> errores, bool obligatorio)
        {
            if (celda.IsEmpty())
            {
                if (obligatorio) errores.Add($"{campo} es obligatorio.");
                return 0;
            }

            if (!celda.TryGetValue(out decimal valor) || valor < 0 || valor > 999999999.99m)
            {
                errores.Add($"{campo} debe ser un número entre 0 y 999999999,99.");
                return 0;
            }

            return valor;
        }

        private static int Entero(IXLCell celda, string campo, List<string> errores)
        {
            if (celda.IsEmpty()) return 0;
            if (!celda.TryGetValue(out int valor) || valor < 0)
            {
                errores.Add($"{campo} debe ser un número entero mayor o igual a 0.");
                return 0;
            }

            return valor;
        }

        private static string Texto(IXLCell celda)
        {
            return celda.GetFormattedString().Trim();
        }

        private static string? TextoOpcional(IXLCell celda)
        {
            string valor = Texto(celda);
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        private static string ClaveCache(string token)
        {
            return $"importacion-productos:{token}";
        }

        private sealed record CategoriaImportacion(int Id, string Nombre);
        private sealed record ImportacionTemporal(int EmpresaId, string UsuarioId, ProductoImportacionVistaPreviaVM VistaPrevia);
    }
}
