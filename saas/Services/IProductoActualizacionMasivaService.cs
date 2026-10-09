using saas.ViewModel.ProductoActualizacionMasiva;

namespace saas.Services
{
    public interface IProductoActualizacionMasivaService
    {
        Task<ProductoActualizacionMasivaVistaPreviaVM> PrepararVistaPreviaAsync(
            ProductoActualizacionMasivaVM modelo,
            int empresaId,
            string usuarioId);

        bool TryObtenerVistaPrevia(
            string token,
            int empresaId,
            string usuarioId,
            out ProductoActualizacionMasivaVistaPreviaVM? vistaPrevia);

        Task<int> AplicarAsync(string token, int empresaId, string usuarioId);
    }
}
