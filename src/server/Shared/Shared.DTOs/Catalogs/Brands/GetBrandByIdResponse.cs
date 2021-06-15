using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Brands
{
    public record GetBrandByIdResponse(Guid Id, string Name, string Detail);
}