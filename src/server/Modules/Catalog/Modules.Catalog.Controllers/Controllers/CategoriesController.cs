using FluentPOS.Modules.Catalog.Core.Features.Categories.Commands;
using FluentPOS.Modules.Catalog.Core.Features.Categories.Queries;
using FluentPOS.Shared.DTOs.Catalogs.Categories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace FluentPOS.Modules.Catalog.Controllers
{
    internal class CategoriesController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Categories.ViewAll)]
        public async Task<IActionResult> GetAll([FromQuery] PaginatedCategoryFilter filter)
        {
            var categories = await Mediator.Send(new GetAllPagedCategoriesQuery(filter.PageNumber, filter.PageSize, filter.SearchString));
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Categories.View)]
        public async Task<IActionResult> GetById(Guid id, bool bypassCache)
        {
            var category = await Mediator.Send(new GetCategoryByIdQuery(id, bypassCache));
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Categories.Register)]
        public async Task<IActionResult> Register(RegisterCategoryCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPut]
        [Authorize(Policy = Permissions.Categories.Update)]
        public async Task<IActionResult> Update(UpdateCategoryCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Categories.Remove)]
        public async Task<IActionResult> Remove(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveCategoryCommand(id)));
        }
    }
}