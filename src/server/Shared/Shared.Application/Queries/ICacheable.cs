using System;

namespace FluentPOS.Shared.Application.Queries
{
    public interface ICacheable
    {
        bool BypassCache { get; }
        string CacheKey { get; }
        TimeSpan? SlidingExpiration { get; }
    }
}