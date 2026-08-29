using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Helpers;
using saas.Models;
using saas.Models.Enums;

namespace saas.Services
{
    public class EmpresaInicializacionService
    {
        private readonly SaasDbContext _context;

        public EmpresaInicializacionService(SaasDbContext context)
        {
            _context = context;
        }

        public async Task InicializarAsync(int empresaId, DateTime fechaAlta)
        {
            var mediosExistentes = await _context.MediosPago.Where(m => m.EmpresaId == empresaId).ToListAsync();
            var mediosPredeterminados = ConfiguracionInicialEmpresa.CrearMediosPagoPredeterminados(empresaId, fechaAlta);

            foreach (var medio in mediosPredeterminados)
            {
                if (!mediosExistentes.Any(m => m.Tipo == medio.Tipo))
                {
                    _context.MediosPago.Add(medio);
                    mediosExistentes.Add(medio);
                }
            }

            bool existeCategoria = await _context.Categorias.AnyAsync(c => c.EmpresaId == empresaId && c.Nombre == "Sin categoría");

            if (!existeCategoria)
            {
                _context.Categorias.Add(new Categoria
                {
                    Nombre = "Sin categoría",
                    Estado = true,
                    EmpresaId = empresaId
                });
            }

            var cajaPrincipal = await _context.Cajas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Nombre == "Caja principal");

            if (cajaPrincipal == null)
            {
                cajaPrincipal = new Caja
                {
                    Nombre = "Caja principal",
                    Tipo = TipoCaja.Efectivo,
                    PermiteTurnos = true,
                    FondoFijo = 0,
                    Estado = true,
                    FechaAlta = fechaAlta,
                    EmpresaId = empresaId
                };

                _context.Cajas.Add(cajaPrincipal);
            }

            await _context.SaveChangesAsync();

            var medioEfectivo = mediosExistentes.First(m => m.Tipo == TipoMedioPago.Efectivo);
            bool existeVinculo = await _context.CajaMediosPago.AnyAsync(cm => cm.CajaId == cajaPrincipal.Id && cm.MedioPagoId == medioEfectivo.Id);

            if (!existeVinculo)
            {
                _context.CajaMediosPago.Add(new CajaMedioPago
                {
                    CajaId = cajaPrincipal.Id,
                    MedioPagoId = medioEfectivo.Id
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}