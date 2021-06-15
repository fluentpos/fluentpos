using AutoMapper;
using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Modules.Catalog.Core.Exceptions;
using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Brands;
using FluentPOS.Shared.Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Queries
{
    internal class BrandQueryHandler :
        IRequestHandler<GetAllPagedBrandsQuery, PaginatedResult<GetAllPagedBrandsResponse>>,
        IRequestHandler<GetBrandByIdQuery, Result<GetBrandByIdResponse>>,
        IRequestHandler<GetBrandImageQuery, Result<string>>
    {
        private readonly ICatalogDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<BrandQueryHandler> _localizer;

        public BrandQueryHandler(ICatalogDbContext context, IMapper mapper, IStringLocalizer<BrandQueryHandler> localizer)
        {
            _context = context;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetAllPagedBrandsResponse>> Handle(GetAllPagedBrandsQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Brand, GetAllPagedBrandsResponse>> expression = e => new GetAllPagedBrandsResponse(e.Id, e.Name, e.Detail);

            var queryable = _context.Brands.OrderBy(x => x.Id).AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchString)) queryable = queryable.Where(b => b.Name.Contains(request.SearchString) || b.Detail.Contains(request.SearchString));

            var brandList = await queryable
                .Select(expression)
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            if (brandList == null) throw new CatalogException(_localizer["Brand Not Found!"]);

            var mappedBrands = _mapper.Map<PaginatedResult<GetAllPagedBrandsResponse>>(brandList);

            return mappedBrands;
        }

        public async Task<Result<GetBrandByIdResponse>> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
        {
            var brand = await _context.Brands.Where(b => b.Id == query.Id).FirstOrDefaultAsync(cancellationToken);
            if (brand == null) throw new CatalogException(_localizer["Brand Not Found!"]);
            var mappedBrand = _mapper.Map<GetBrandByIdResponse>(brand);
            return await Result<GetBrandByIdResponse>.SuccessAsync(mappedBrand);
        }

        public async Task<Result<string>> Handle(GetBrandImageQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.Brands.Where(b => b.Id == request.Id).Select(a => a.ImageUrl).FirstOrDefaultAsync(cancellationToken);
            return await Result<string>.SuccessAsync(data: data);
        }
    }
}