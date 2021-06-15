using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Categories
{
    public record GetAllPagedCategoriesResponse(Guid Id, string Name, string Detail);
}