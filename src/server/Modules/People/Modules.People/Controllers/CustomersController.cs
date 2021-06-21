using FluentPOS.Modules.People.Core.Features.Customers.Commands;
using FluentPOS.Modules.People.Core.Features.Customers.Queries;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.DTOs.People.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FluentPOS.Modules.People.Controllers
{
    internal class CustomersController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Customers.ViewAll)]
        public async Task<IActionResult> GetAll([FromQuery] PaginatedCustomerFilter filter)
        {
            var brands = await Mediator.Send(new GetAllPagedCustomersQuery(filter.PageNumber, filter.PageSize, filter.SearchString));
            return Ok(brands);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Customers.View)]
        public async Task<IActionResult> GetById(Guid id, bool bypassCache)
        {
            var brand = await Mediator.Send(new GetCustomerByIdQuery(id, bypassCache));
            return Ok(brand);
        }

        [Authorize(Policy = Permissions.Customers.Register)]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterCustomerCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [Authorize(Policy = Permissions.Customers.Update)]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateCustomerCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [Authorize(Policy = Permissions.Customers.Remove)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            return Ok(await Mediator.Send(new RemoveCustomerCommand(id)));
        }
    }
}