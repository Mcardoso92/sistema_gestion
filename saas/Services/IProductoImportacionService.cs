using saas.ViewModel.ProductoImportacion;

namespace saas.Services
{
    public interface IProductoImportacionService
    {
        byte[] GenerarPlantilla();
        Task<ProductoImportacionVistaPreviaVM> AnalizarAsync(IFormFile archivo, int empresaId, string usuarioId);
        Task<int> ImportarAsync(string token, int empresaId, string usuarioId);
        bool TryObtenerVistaPrevia(string token, int empresaId, string usuarioId, out ProductoImportacionVistaPreviaVM? vistaPrevia);
        void EliminarVistaPrevia(string token);
    }
}
