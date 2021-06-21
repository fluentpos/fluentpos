using FluentPOS.Modules.Catalog.Core.Features.Products.Commands;
using FluentPOS.Modules.Catalog.Core.Features.Products.Queries;
using FluentPOS.Shared.DTOs.Catalogs.Products;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FluentPOS.Modules.Catalog.Controllers
{
    internal class ProductsController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Products.ViewAll)]
        public async Task<IActionResult> GetAll([FromQuery] PaginatedProductFilter filter)
        {
            var brands = await Mediator.Send(new GetAllPagedProductsQuery(filter.PageNumber, filter.PageSize, filter.SearchString, filter.BrandId, filter.CategoryId));
            return Ok(brands);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Products.View)]
        public async Task<IActionResult> GetById(Guid id, bool bypassCache)
        {
            var brand = await Mediator.Send(new GetProductByIdQuery(id, bypassCache));
            return Ok(brand);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Products.Register)]
        public async Task<IActionResult> Register(RegisterProductCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        [Authorize(Policy = Permissions.Products.Update)]
        public async Task<IActionResult> Update(UpdateProductCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Products.Remove)]
        public async Task<IActionResult> Remove(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveProductCommand(id)));
        }
    }
}