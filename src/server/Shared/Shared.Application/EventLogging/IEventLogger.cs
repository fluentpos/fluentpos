using FluentPOS.Shared.Application.Domain;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Application.EventLogging
{
    public interface IEventLogger
    {
        Task Save<T>(T @event) where T : Event;
    }
}