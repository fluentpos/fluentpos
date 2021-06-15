using FluentPOS.Shared.DTOs.Mails;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Application.Interfaces.Services
{
    public interface IMailService
    {
        Task SendAsync(MailRequest request);
    }
}