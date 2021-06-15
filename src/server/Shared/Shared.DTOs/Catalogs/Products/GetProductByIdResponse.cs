using System;

namespace FluentPOS.Shared.DTOs.Catalogs.Products
{
    public record GetProductByIdResponse(Guid Id, string Name, string LocaleName, string BarcodeSymbology, string Detail, Guid BrandId, Guid CategoryId, decimal Price, decimal Cost, string ImageUrl, string Tax, string TaxMethod, bool IsAlert, decimal AlertQuantity);
}