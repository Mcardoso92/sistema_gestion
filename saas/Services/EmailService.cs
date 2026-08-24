using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using saas.Settings;

namespace saas.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(
            IOptions<EmailSettings> options)
        {
            _settings =
                options.Value;
        }

        public async Task EnviarAsync(
            string destinatario,
            string asunto,
            string contenidoHtml)
        {
            var mensaje =
                new MimeMessage();

            mensaje.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.FromEmail));

            mensaje.To.Add(
                MailboxAddress.Parse(
                    destinatario));

            mensaje.Subject =
                asunto;

            mensaje.Body =
                new TextPart("html")
                {
                    Text =
                        contenidoHtml
                };

            using var cliente =
                new SmtpClient();

            var seguridad =
                _settings.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

            await cliente.ConnectAsync(
                _settings.Host,
                _settings.Port,
                seguridad);

            await cliente.AuthenticateAsync(
                _settings.UserName,
                _settings.Password);

            await cliente.SendAsync(
                mensaje);

            await cliente.DisconnectAsync(
                true);
        }
    }
}