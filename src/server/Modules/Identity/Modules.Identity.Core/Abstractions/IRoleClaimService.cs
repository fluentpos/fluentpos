using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Identity.Roles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentPOS.Modules.Identity.Core.Abstractions
{
    public interface IRoleClaimService
    {
        Task<Result<List<RoleClaimResponse>>> GetAllAsync();

        Task<int> GetCountAsync();

        Task<Result<RoleClaimResponse>> GetByIdAsync(Guid id);

        Task<Result<List<RoleClaimResponse>>> GetAllByRoleIdAsync(Guid roleId);

        Task<Result<string>> SaveAsync(RoleClaimRequest request);

        Task<Result<string>> DeleteAsync(Guid id);

        Task<Result<PermissionResponse>> GetAllPermissionsAsync(Guid roleId);

        Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request);
    }
}