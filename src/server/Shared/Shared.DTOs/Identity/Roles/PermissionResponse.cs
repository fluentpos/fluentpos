using System;
using System.Collections.Generic;

namespace FluentPOS.Shared.DTOs.Identity.Roles
{
    public class PermissionResponse
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public List<RoleClaimResponse> RoleClaims { get; set; }
    }
}