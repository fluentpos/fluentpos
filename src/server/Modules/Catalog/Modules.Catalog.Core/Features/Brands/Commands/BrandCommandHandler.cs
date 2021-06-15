using AutoMapper;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Core.Constants;
using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Modules.Catalog.Core.Exceptions;
using FluentPOS.Modules.Catalog.Core.Features.Brands.Events;
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

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Commands
{
    internal class BrandCommandHandler :
        IRequestHandler<RemoveBrandCommand, Result<Guid>>,
        IRequestHandler<RegisterBrandCommand, Result<Guid>>,
        IRequestHandler<UpdateBrandCommand, Result<Guid>>

    {
        private readonly IDistributedCache _cache;
        private readonly ICatalogDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;
        private readonly IStringLocalizer<BrandCommandHandler> _localizer;

        public BrandCommandHandler(ICatalogDbContext context, IMapper mapper, IUploadService uploadService, IStringLocalizer<BrandCommandHandler> localizer, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _uploadService = uploadService;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<Result<Guid>> Handle(RegisterBrandCommand command, CancellationToken cancellationToken)
        {
            var brand = _mapper.Map<Brand>(command);
            var uploadRequest = command.UploadRequest;
            if (uploadRequest != null)
            {
                uploadRequest.FileName = $"B-{command.Name}{uploadRequest.Extension}";
                brand.ImageUrl = _uploadService.UploadAsync(uploadRequest);
            }
            brand.AddDomainEvent(new BrandRegisteredEvent(brand.Id, brand.Name, brand.ImageUrl, brand.Detail));
            await _context.Brands.AddAsync(brand, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<Guid>.SuccessAsync(brand.Id, _localizer["Brand Saved"]);
        }

        public async Task<Result<Guid>> Handle(RemoveBrandCommand command, CancellationToken cancellationToken)
        {
            var isBrandUsed = await IsBrandUsed(command.Id);
            if (isBrandUsed) throw new CatalogException(_localizer["Deletion Not Allowed"]);
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);
            if (brand == null) throw new CatalogException(_localizer["Brand Not Found"]);
            _context.Brands.Remove(brand);
            brand.AddDomainEvent(new BrandRemovedEvent(command.Id));
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync(CatalogCacheKeys.GetBrandByIdCacheKey(command.Id), cancellationToken);
            return await Result<Guid>.SuccessAsync(brand.Id, _localizer["Brand Deleted"]);
        }

        public async Task<Result<Guid>> Handle(UpdateBrandCommand command, CancellationToken cancellationToken)
        {
            var brand = await _context.Brands.Where(b => b.Id == command.Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            if (brand == null) throw new CatalogException(_localizer["Brand Not Found!"]);
            brand = _mapper.Map<Brand>(command);
            var uploadRequest = command.UploadRequest;
            if (uploadRequest != null)
            {
                uploadRequest.FileName = $"B-{command.Name}{uploadRequest.Extension}";
                brand.ImageUrl = _uploadService.UploadAsync(uploadRequest);
            }
            brand.AddDomainEvent(new BrandUpdatedEvent(brand.Id, brand.Name, brand.ImageUrl, brand.Detail));
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync(CatalogCacheKeys.GetBrandByIdCacheKey(command.Id), cancellationToken);
            return await Result<Guid>.SuccessAsync(brand.Id, _localizer["Brand Updated"]);
        }

        public async Task<bool> IsBrandUsed(Guid brandId)
        {
            return await _context.Products.AnyAsync(b => b.BrandId == brandId);
        }
    }
}