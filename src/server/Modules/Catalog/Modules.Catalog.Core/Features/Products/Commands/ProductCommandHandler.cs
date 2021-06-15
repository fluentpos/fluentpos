using AutoMapper;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Core.Constants;
using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Modules.Catalog.Core.Exceptions;
using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Application.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentPOS.Modules.Catalog.Core.Features.Products.Commands
{
    internal class ProductCommandHandler :
        IRequestHandler<RegisterProductCommand, Result<Guid>>,
        IRequestHandler<RemoveProductCommand, Result<Guid>>,
        IRequestHandler<UpdateProductCommand, Result<Guid>>

    {
        private readonly IDistributedCache _cache;
        private readonly ICatalogDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;
        private readonly IStringLocalizer<ProductCommandHandler> _localizer;

        public ProductCommandHandler(ICatalogDbContext context, IMapper mapper, IUploadService uploadService, IStringLocalizer<ProductCommandHandler> localizer, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<Result<Guid>> Handle(RegisterProductCommand command, CancellationToken cancellationToken)
        {
            if (await _context.Products.AnyAsync(p => p.BarcodeSymbology == command.BarcodeSymbology, cancellationToken))
            {
                throw new CatalogException(_localizer["Barcode already exists."]);
            }

            var product = _mapper.Map<Product>(command);
            var uploadRequest = command.UploadRequest;
            if (uploadRequest != null)
            {
                uploadRequest.FileName = $"P-{command.BarcodeSymbology}{uploadRequest.Extension}";
                product.ImageUrl = _uploadService.UploadAsync(uploadRequest);
            }
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(product.Id, _localizer["Product Saved"]);
        }

        public async Task<Result<Guid>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            if (await _context.Products.Where(p => p.Id != command.Id).AnyAsync(p => p.BarcodeSymbology == command.BarcodeSymbology, cancellationToken))
            {
                throw new CatalogException(_localizer["Barcode already exists."]);
            }

            var product = await _context.Products.Where(p => p.Id == command.Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            if (product != null)
            {
                product = _mapper.Map<Product>(command);
                var uploadRequest = command.UploadRequest;
                if (uploadRequest != null)
                {
                    uploadRequest.FileName = $"P-{command.BarcodeSymbology}{uploadRequest.Extension}";
                    product.ImageUrl = _uploadService.UploadAsync(uploadRequest);
                }
                _context.Products.Update(product);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.RemoveAsync(CatalogCacheKeys.GetProductByIdCacheKey(command.Id), cancellationToken);
                return await Result<Guid>.SuccessAsync(product.Id, _localizer["Product Updated"]);
            }
            else
            {
                throw new CatalogException(_localizer["Product Not Found!"]);
            }
        }

        public async Task<Result<Guid>> Handle(RemoveProductCommand command, CancellationToken cancellationToken)
        {
            var product = await _context.Products.Where(p => p.Id == command.Id).FirstOrDefaultAsync(cancellationToken);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.RemoveAsync(CatalogCacheKeys.GetProductByIdCacheKey(command.Id), cancellationToken);
                return await Result<Guid>.SuccessAsync(product.Id, _localizer["Product Deleted"]);
            }
            else
            {
                throw new CatalogException(_localizer["Product Not Found!"]);
            }
        }
    }
}