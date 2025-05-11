using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public RoleService(RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<RoleDto>>> GetAllRolesAsync()
        {
            var roles = _roleManager.Roles.ToList();
            var mapped = _mapper.Map<IEnumerable<RoleDto>>(roles);
            return Result<IEnumerable<RoleDto>>.Ok(mapped, "Roles fetched successfully.");
        }

        public async Task<Result<RoleDto>> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return Result<RoleDto>.NotFound("Role not found");

            var mapped = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Ok(mapped, "Role fetched successfully.");
        }

        public async Task<Result<RoleDto>> AddRoleAsync(RoleDto roleDto)
        {
            var role = new IdentityRole { Name = roleDto.Name };
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                return Result<RoleDto>.Internal("Error while Delete");

            var mapped = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Ok(mapped, "Role created successfully.");
        }

        public async Task<Result<RoleDto>> UpdateRoleAsync(RoleDto roleDto)
        {
            var role = await _roleManager.FindByIdAsync(roleDto.Id);
            if (role == null)
                return Result<RoleDto>.NotFound("Role not found");

            role.Name = roleDto.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return Result<RoleDto>.Internal("Error while Delete");

            var mapped = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Ok(mapped, "Role updated successfully.");
        }

        public async Task<Result<bool>> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return Result<bool>.NotFound("Role not found");

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return Result<bool>.Internal("Error while Delete");

            return Result<bool>.Ok(true, "Role deleted successfully.");
        }
    }
}
