using AutoMapper;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Tenants.DTOs;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Features.Permissions.DTOs;

namespace IdentityService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles will be loaded separately

        CreateMap<ApplicationUser, UserListDto>();

        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

        // Tenant mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.Users, opt => opt.Ignore()) // Users will be loaded separately
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles will be loaded separately

        CreateMap<Tenant, TenantListDto>();

        CreateMap<CreateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<UpdateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        // Role mappings
        CreateMap<ApplicationRole, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Permissions will be loaded separately

        CreateMap<ApplicationRole, RoleListDto>();

        CreateMap<CreateRoleDto, ApplicationRole>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

        CreateMap<UpdateRoleDto, ApplicationRole>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

        // Permission mappings
        CreateMap<Permission, PermissionDto>();

        CreateMap<Permission, PermissionListDto>();

        CreateMap<CreatePermissionDto, Permission>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdatePermissionDto, Permission>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore());
    }
} 