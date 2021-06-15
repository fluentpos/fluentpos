using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Application.Settings;
using FluentPOS.Shared.DTOs.Mails;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Infrastructure.Services
{
    public class SmtpMailService : IMailService
    {
        private readonly MailSettings _settings;
        public ILogger<SmtpMailService> Logger { get; }

        public SmtpMailService(IOptions<MailSettings> settings, ILogger<SmtpMailService> logger)
        {
            _settings = settings.Value;
            Logger = logger;
        }

        public async Task SendAsync(MailRequest request)
        {
            try
            {
                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(request.From ?? _settings.From),
                    Subject = request.Subject,
                    Body = new BodyBuilder {HtmlBody = request.Body}.ToMessageBody()
                };
                email.To.Add(MailboxAddress.Parse(request.To));
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex.Message, ex);
            }
        }
    }
}