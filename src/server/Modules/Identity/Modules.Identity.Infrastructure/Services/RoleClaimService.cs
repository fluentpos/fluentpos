using AutoMapper;
using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Helpers;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Interfaces.Services.Identity;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Features.RoleClaims.Events;

namespace FluentPOS.Modules.Identity.Infrastructure.Services
{
    public class RoleClaimService : IRoleClaimService
    {
        private readonly RoleManager<FluentRole> _roleManager;
        private readonly UserManager<FluentUser> _userManager;
        private readonly ICurrentUser _currentUserService;
        private readonly IStringLocalizer<RoleClaimService> _localizer;
        private readonly IMapper _mapper;
        private readonly IIdentityDbContext _db;

        public RoleClaimService(
            RoleManager<FluentRole> roleManager,
            UserManager<FluentUser> userManager,
            ICurrentUser currentUserService,
            IStringLocalizer<RoleClaimService> localizer,
            IMapper mapper,
            IIdentityDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _mapper = mapper;
            _db = db;
        }

        public async Task<Result<List<RoleClaimResponse>>> GetAllAsync()
        {
            var roleClaims = await _db.RoleClaims.AsNoTracking().ToListAsync();
            var roleClaimsResponse = _mapper.Map<List<RoleClaimResponse>>(roleClaims);
            return await Result<List<RoleClaimResponse>>.SuccessAsync(roleClaimsResponse);
        }

        public async Task<int> GetCountAsync()
        {
            var count = await _db.RoleClaims.AsNoTracking().CountAsync();
            return count;
        }

        public async Task<Result<RoleClaimResponse>> GetByIdAsync(Guid id)
        {
            var roleClaim = await _db.RoleClaims.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);
            var roleClaimResponse = _mapper.Map<RoleClaimResponse>(roleClaim);
            return await Result<RoleClaimResponse>.SuccessAsync(roleClaimResponse);
        }

        public async Task<Result<List<RoleClaimResponse>>> GetAllByRoleIdAsync(Guid roleId)
        {
            var roleClaims = await _db.RoleClaims
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x => x.RoleId == roleId)
                .ToListAsync();
            var roleClaimsResponse = _mapper.Map<List<RoleClaimResponse>>(roleClaims);
            return await Result<List<RoleClaimResponse>>.SuccessAsync(roleClaimsResponse);
        }

        public async Task<Result<string>> SaveAsync(RoleClaimRequest request)
        {
            if (request.Id == Guid.Empty)
            {
                var existingRoleClaim =
                    await _db.RoleClaims
                        .SingleOrDefaultAsync(x =>
                            x.RoleId == request.RoleId && x.ClaimType == request.Type && x.ClaimValue == request.Value);
                if (existingRoleClaim != null)
                {
                    return await Result<string>.FailAsync(_localizer["Similar Role Claim already exists."]);
                }
                var roleClaim = _mapper.Map<FluentRoleClaim>(request);
                await _db.RoleClaims.AddAsync(roleClaim);
                roleClaim.AddDomainEvent(new RoleClaimAddedEvent(roleClaim));
                await _db.SaveChangesAsync();
                return await Result<string>.SuccessAsync(string.Format(_localizer["Role Claim {0} created."], request.Value));
            }
            else
            {
                var existingRoleClaim =
                    await _db.RoleClaims
                        .Include(x => x.Role)
                        .SingleOrDefaultAsync(x => x.Id == request.Id);
                if (existingRoleClaim == null)
                {
                    return await Result<string>.FailAsync(_localizer["Role Claim does not exist."]);
                }
                else
                {
                    existingRoleClaim.ClaimType = request.Type;
                    existingRoleClaim.ClaimValue = request.Value;
                    existingRoleClaim.Group = request.Group;
                    existingRoleClaim.Description = request.Description;
                    existingRoleClaim.RoleId = request.RoleId;
                    _db.RoleClaims.Update(existingRoleClaim);
                    existingRoleClaim.AddDomainEvent(new RoleClaimUpdatedEvent(existingRoleClaim));
                    await _db.SaveChangesAsync();
                    return await Result<string>.SuccessAsync(string.Format(_localizer["Role Claim {0} for Role {1} updated."], request.Value, existingRoleClaim.Role.Name));
                }
            }
        }

        public async Task<Result<string>> DeleteAsync(Guid id)
        {
            var existingRoleClaim = await _db.RoleClaims
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existingRoleClaim != null)
            {
                if (existingRoleClaim.Role?.Name == RoleConstants.SuperAdmin)
                {
                    return await Result<string>.FailAsync(string.Format(_localizer["Not allowed to delete Permissions for {0} Role."], existingRoleClaim.Role.Name));
                }

                _db.RoleClaims.Remove(existingRoleClaim);
                existingRoleClaim.AddDomainEvent(new RoleClaimDeletedEvent(id));
                await _db.SaveChangesAsync();
                return await Result<string>.SuccessAsync(string.Format(_localizer["Role Claim {0} for {1} Role deleted."], existingRoleClaim.ClaimValue, existingRoleClaim.Role?.Name));
            }
            else
            {
                return await Result<string>.FailAsync(_localizer["Role Claim does not exist."]);
            }
        }

        public async Task<Result<PermissionResponse>> GetAllPermissionsAsync(Guid roleId)
        {
            var response = new PermissionResponse();
            var allPermissions = new List<RoleClaimResponse>();
            allPermissions.GetAllPermissions();
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role != null)
            {
                response.RoleId = role.Id;
                response.RoleName = role.Name;
                var allRoleClaims = await GetAllAsync();
                var roleClaimsResult = await GetAllByRoleIdAsync(role.Id);
                if (roleClaimsResult.Succeeded)
                {
                    var roleClaims = roleClaimsResult.Data;
                    var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                    var roleClaimValues = roleClaims.Select(a => a.Value).ToList();
                    var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                    foreach (var permission in allPermissions)
                    {
                        permission.Id = allRoleClaims.Data?.SingleOrDefault(x => x.RoleId == roleId && x.Type == permission.Type && x.Value == permission.Value)?.Id ?? Guid.Empty;
                        permission.RoleId = roleId;
                        if (authorizedClaims.Any(a => a == permission.Value))
                        {
                            permission.Selected = true;
                            var roleClaim = roleClaims.SingleOrDefault(a => a.Type == permission.Type && a.Value == permission.Value);
                            if (roleClaim?.Description != null)
                            {
                                permission.Description = roleClaim.Description;
                            }
                            if (roleClaim?.Group != null)
                            {
                                permission.Group = roleClaim.Group;
                            }
                        }
                    }
                }
                else
                {
                    response.RoleClaims = new List<RoleClaimResponse>();
                    return await Result<PermissionResponse>.FailAsync(roleClaimsResult.Messages);
                }
            }
            response.RoleClaims = allPermissions;
            return await Result<PermissionResponse>.SuccessAsync(response);
        }

        public async Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RoleId))
                {
                    return await Result<string>.FailAsync(_localizer["Role is required."]);
                }

                var errors = new List<string>();
                var role = await _roleManager.FindByIdAsync(request.RoleId);
                if (role.Name == RoleConstants.SuperAdmin)
                {
                    var currentUser = await _userManager.Users.SingleAsync(x => x.Id == _currentUserService.GetUserId());
                    if (!await _userManager.IsInRoleAsync(currentUser, RoleConstants.SuperAdmin))
                    {
                        return await Result<string>.FailAsync(_localizer["Not allowed to modify Permissions for this Role."]);
                    }
                }

                var selectedClaims = request.RoleClaims.Where(a => a.Selected).ToList();
                if (role.Name == RoleConstants.SuperAdmin)
                {
                    if (selectedClaims.All(x => x.Value != Shared.Core.Constants.Permissions.Roles.View) ||
                        selectedClaims.All(x => x.Value != Shared.Core.Constants.Permissions.RoleClaims.View) ||
                        selectedClaims.All(x => x.Value != Shared.Core.Constants.Permissions.RoleClaims.Edit))
                    {
                        return await Result<string>.FailAsync(string.Format(
                            _localizer["Not allowed to deselect {0} or {1} or {2} for this Role."],
                            Shared.Core.Constants.Permissions.Roles.View, Shared.Core.Constants.Permissions.RoleClaims.View, Shared.Core.Constants.Permissions.RoleClaims.Edit));
                    }
                }

                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
                foreach (var claim in selectedClaims)
                {
                    var addResult = await _roleManager.AddPermissionClaim(role, claim.Value);
                    if (!addResult.Succeeded)
                    {
                        errors.AddRange(addResult.Errors.Select(e => _localizer[e.Description].ToString()));
                    }
                }

                var addedClaims = await GetAllByRoleIdAsync(role.Id);
                if (addedClaims.Succeeded)
                {
                    foreach (var claim in selectedClaims)
                    {
                        var addedClaim = addedClaims.Data.SingleOrDefault(x => x.Type == claim.Type && x.Value == claim.Value);
                        if (addedClaim != null)
                        {
                            claim.Id = addedClaim.Id;
                            claim.RoleId = addedClaim.RoleId;
                            var saveResult = await SaveAsync(claim);
                            if (!saveResult.Succeeded)
                            {
                                errors.AddRange(saveResult.Messages);
                            }
                        }
                    }
                }
                else
                {
                    errors.AddRange(addedClaims.Messages);
                }

                if (errors.Any())
                {
                    return await Result<string>.FailAsync(errors);
                }

                return await Result<string>.SuccessAsync(_localizer["Permissions Updated."]);
            }
            catch (Exception ex)
            {
                return await Result<string>.FailAsync(ex.Message);
            }
        }
    }
}