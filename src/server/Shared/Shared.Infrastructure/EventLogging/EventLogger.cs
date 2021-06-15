using FluentPOS.Shared.Application.Domain;
using FluentPOS.Shared.Application.EventLogging;
using FluentPOS.Shared.Application.Interfaces;
using FluentPOS.Shared.Application.Interfaces.Services.Identity;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Infrastructure.EventLogging
{
    internal class EventLogger : IEventLogger
    {
        private readonly ICurrentUser _user;
        private readonly IApplicationDbContext _context;

        public EventLogger(ICurrentUser user, IApplicationDbContext context)
        {
            _user = user;
            _context = context;
        }

        public async Task Save<T>(T @event) where T : Event
        {
            var serializedData = JsonConvert.SerializeObject(@event);

            var thisEvent = new EventLog(
                @event,
                serializedData,
                _user.Name ?? _user.GetUserEmail());
            _context.EventLogs.Add(thisEvent);
            await _context.SaveChangesAsync(default);
        }
    }
}