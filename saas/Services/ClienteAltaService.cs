using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;

namespace saas.Services
{
    public sealed class ClienteAltaResultado
    {
        public Cliente? Cliente { get; init; }
        public Dictionary<string, string> Errores { get; } = new();
        public bool Exitoso => Cliente != null;
    }

    public sealed class ClienteAltaService
    {
        private readonly SaasDbContext _context;
        private readonly IFechaHoraService _fechaHora;

        public ClienteAltaService(
            SaasDbContext context,
            IFechaHoraService fechaHora)
        {
            _context = context;
            _fechaHora = fechaHora;
        }

        public async Task<ClienteAltaResultado> CrearAsync(
            ClienteCreateVM clienteVM,
            int empresaId)
        {
            var resultado = new ClienteAltaResultado();

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == empresaId && e.Estado);

            if (!empresaValida)
            {
                resultado.Errores[nameof(clienteVM.EmpresaId)] =
                    "La empresa seleccionada no es válida.";
                return resultado;
            }

            clienteVM.Nombre = clienteVM.Nombre.Trim();
            clienteVM.Apellido = NormalizarOpcional(clienteVM.Apellido);
            clienteVM.Documento = NormalizarOpcional(clienteVM.Documento);
            clienteVM.Email = NormalizarOpcional(clienteVM.Email);
            clienteVM.Telefono = NormalizarOpcional(clienteVM.Telefono);
            clienteVM.Direccion = NormalizarOpcional(clienteVM.Direccion);

            if (CuitValidator.TieneFormatoCuit(clienteVM.Documento))
            {
                clienteVM.Documento = CuitValidator.Normalizar(clienteVM.Documento);

                if (!CuitValidator.EsValido(clienteVM.Documento))
                {
                    resultado.Errores[nameof(clienteVM.Documento)] =
                        "El CUIT ingresado no es válido.";
                    return resultado;
                }
            }

            if (clienteVM.Documento != null)
            {
                bool existeDocumento = await _context.Clientes.AnyAsync(c =>
                    c.EmpresaId == empresaId &&
                    c.Documento == clienteVM.Documento);

                if (existeDocumento)
                {
                    resultado.Errores[nameof(clienteVM.Documento)] =
                        "Ya existe un cliente con ese documento para esta empresa.";
                    return resultado;
                }
            }

            var cliente = new Cliente
            {
                Nombre = clienteVM.Nombre,
                Apellido = clienteVM.Apellido,
                Documento = clienteVM.Documento,
                Email = clienteVM.Email,
                Telefono = clienteVM.Telefono,
                Direccion = clienteVM.Direccion,
                EmpresaId = empresaId,
                Estado = true,
                FechaAlta = _fechaHora.UtcAhora
            };

            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return new ClienteAltaResultado { Cliente = cliente };
            }
            catch
            {
                resultado.Errores[string.Empty] =
                    "Ocurrió un error al crear el cliente.";
                return resultado;
            }
        }

        private static string? NormalizarOpcional(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
