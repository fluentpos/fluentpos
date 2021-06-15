using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Categories
{
    public record GetCategoryByIdResponse(Guid Id, string Name, string Detail);
}