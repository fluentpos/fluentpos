using AutoMapper;
using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Exceptions;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Identity.Roles;
using FluentPOS.Shared.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Features.Roles.Events;
using System;

namespace FluentPOS.Modules.Identity.Infrastructure.Services
{
    internal class RoleService : IRoleService
    {
        private readonly RoleManager<FluentRole> _roleManager;
        private readonly UserManager<FluentUser> _userManager;
        private readonly IIdentityDbContext _context;
        private readonly IStringLocalizer<RoleService> _localizer;
        private readonly IMapper _mapper;

        public RoleService(
            RoleManager<FluentRole> roleManager,
            IMapper mapper,
            UserManager<FluentUser> userManager,
            IIdentityDbContext context,
            IStringLocalizer<RoleService> localizer)
        {
            _roleManager = roleManager;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        private static List<string> DefaultRoles()
        {
            return typeof(RoleConstants).GetAllPublicConstantValues<string>();
        }

        public async Task<Result<Guid>> DeleteAsync(Guid id)
        {
            var existingRole = await _roleManager.FindByIdAsync(id.ToString());
            if (existingRole == null) throw new IdentityException("Role Not Found", statusCode: System.Net.HttpStatusCode.NotFound);
            if (DefaultRoles().Contains(existingRole.Name))
            {
                return await Result<Guid>.FailAsync(string.Format(_localizer["Not allowed to delete {0} Role."], existingRole.Name));
            }
            bool roleIsNotUsed = true;
            var allUsers = await _userManager.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, existingRole.Name))
                {
                    roleIsNotUsed = false;
                }
            }
            if (roleIsNotUsed)
            {
                existingRole.AddDomainEvent(new RoleDeletedEvent(id));
                await _roleManager.DeleteAsync(existingRole);
                return await Result<Guid>.SuccessAsync(existingRole.Id, string.Format(_localizer["Role {0} Deleted."], existingRole.Name));
            }
            else
            {
                return await Result<Guid>.FailAsync(string.Format(_localizer["Not allowed to delete {0} Role as it is being used."], existingRole.Name));
            }
        }

        public async Task<Result<List<RoleResponse>>> GetAllAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var rolesResponse = _mapper.Map<List<RoleResponse>>(roles);
            return await Result<List<RoleResponse>>.SuccessAsync(rolesResponse);
        }

        public async Task<Result<RoleResponse>> GetByIdAsync(Guid id)
        {
            var roles = await _roleManager.Roles.SingleOrDefaultAsync(x => x.Id == id);
            var rolesResponse = _mapper.Map<RoleResponse>(roles);
            return await Result<RoleResponse>.SuccessAsync(rolesResponse);
        }

        public async Task<Result<Guid>> SaveAsync(RoleRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null) throw new IdentityException(_localizer["Similar Role already exists."], statusCode: System.Net.HttpStatusCode.BadRequest);
                var newRole = new FluentRole(request.Name, request.Description);
                var response = await _roleManager.CreateAsync(newRole);
                newRole.AddDomainEvent(new RoleAddedEvent(newRole));
                await _context.SaveChangesAsync();
                if (response.Succeeded)
                {
                    return await Result<Guid>.SuccessAsync(newRole.Id, string.Format(_localizer["Role {0} Created."], request.Name));
                }
                else
                {
                    return await Result<Guid>.FailAsync(response.Errors.Select(e => _localizer[e.Description].ToString()).ToList());
                }
            }
            else
            {
                var existingRole = await _roleManager.FindByIdAsync(request.Id);
                if (existingRole == null) return await Result<Guid>.FailAsync(_localizer["Role does not exist."]);
                if (DefaultRoles().Contains(existingRole.Name))
                {
                    return await Result<Guid>.SuccessAsync(string.Format(_localizer["Not allowed to modify {0} Role."], existingRole.Name));
                }
                existingRole.Name = request.Name;
                existingRole.NormalizedName = request.Name.ToUpper();
                existingRole.Description = request.Description;
                existingRole.AddDomainEvent(new RoleUpdatedEvent(existingRole));
                await _roleManager.UpdateAsync(existingRole);
                return await Result<Guid>.SuccessAsync(existingRole.Id, string.Format(_localizer["Role {0} Updated."], existingRole.Name));
            }
        }

        public async Task<int> GetCountAsync()
        {
            var count = await _roleManager.Roles.CountAsync();
            return count;
        }
    }
}