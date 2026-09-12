using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;

namespace saas.Services
{
    public sealed class ProveedorAltaResultado
    {
        public Proveedor? Proveedor { get; init; }
        public Dictionary<string, string> Errores { get; } = new();
        public bool Exitoso => Proveedor != null;
    }

    public sealed class ProveedorAltaService
    {
        private readonly SaasDbContext _context;
        private readonly IFechaHoraService _fechaHora;

        public ProveedorAltaService(
            SaasDbContext context,
            IFechaHoraService fechaHora)
        {
            _context = context;
            _fechaHora = fechaHora;
        }

        public async Task<ProveedorAltaResultado> CrearAsync(
            ProveedorCreateVM proveedorVM,
            int empresaId)
        {
            var resultado = new ProveedorAltaResultado();

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == empresaId && e.Estado);

            if (!empresaValida)
            {
                resultado.Errores[nameof(proveedorVM.EmpresaId)] =
                    "La empresa seleccionada no es válida.";
                return resultado;
            }

            proveedorVM.RazonSocial = proveedorVM.RazonSocial.Trim();
            proveedorVM.NombreFantasia = NormalizarOpcional(proveedorVM.NombreFantasia);
            proveedorVM.Email = NormalizarOpcional(proveedorVM.Email);
            proveedorVM.Telefono = NormalizarOpcional(proveedorVM.Telefono);
            proveedorVM.Direccion = NormalizarOpcional(proveedorVM.Direccion);
            proveedorVM.Localidad = NormalizarOpcional(proveedorVM.Localidad);
            proveedorVM.Provincia = NormalizarOpcional(proveedorVM.Provincia);
            proveedorVM.CodigoPostal = NormalizarOpcional(proveedorVM.CodigoPostal);
            proveedorVM.Observaciones = NormalizarOpcional(proveedorVM.Observaciones);

            string? cuitNormalizado = null;

            if (!string.IsNullOrWhiteSpace(proveedorVM.CUIT))
            {
                cuitNormalizado = CuitValidator.Normalizar(proveedorVM.CUIT);

                if (!CuitValidator.EsValido(cuitNormalizado))
                {
                    resultado.Errores[nameof(proveedorVM.CUIT)] =
                        "El CUIT ingresado no es válido.";
                    return resultado;
                }

                bool existeCuit = await _context.Proveedores.AnyAsync(p =>
                    p.EmpresaId == empresaId &&
                    p.CUIT == cuitNormalizado &&
                    p.Estado);

                if (existeCuit)
                {
                    resultado.Errores[nameof(proveedorVM.CUIT)] =
                        "Ya existe un proveedor activo con ese CUIT para esta empresa.";
                    return resultado;
                }
            }

            var proveedor = new Proveedor
            {
                RazonSocial = proveedorVM.RazonSocial,
                NombreFantasia = proveedorVM.NombreFantasia,
                CUIT = cuitNormalizado,
                Email = proveedorVM.Email,
                Telefono = proveedorVM.Telefono,
                Direccion = proveedorVM.Direccion,
                Localidad = proveedorVM.Localidad,
                Provincia = proveedorVM.Provincia,
                CodigoPostal = proveedorVM.CodigoPostal,
                Observaciones = proveedorVM.Observaciones,
                Estado = true,
                FechaAlta = _fechaHora.UtcAhora,
                EmpresaId = empresaId
            };

            try
            {
                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();
                return new ProveedorAltaResultado { Proveedor = proveedor };
            }
            catch
            {
                resultado.Errores[string.Empty] =
                    "Ocurrió un error al crear el proveedor.";
                return resultado;
            }
        }

        private static string? NormalizarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
