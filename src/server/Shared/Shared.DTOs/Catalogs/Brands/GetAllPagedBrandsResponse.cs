using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Brands
{
    public record GetAllPagedBrandsResponse(Guid Id, string Name, string Detail);
}