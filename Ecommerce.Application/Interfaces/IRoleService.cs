using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Result<IEnumerable<RoleDto>>> GetAllRolesAsync();
        Task<Result<RoleDto>> GetRoleByIdAsync(string id);
        Task<Result<RoleDto>> AddRoleAsync(RoleDto roleDto);
        Task<Result<RoleDto>> UpdateRoleAsync(RoleDto roleDto);
        Task<Result<bool>> DeleteRoleAsync(string id);
    }
}
