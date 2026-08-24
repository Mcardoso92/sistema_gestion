namespace saas.Services
{
    public interface IEmailService
    {
        Task EnviarAsync(
            string destinatario,
            string asunto,
            string contenidoHtml);
    }
}