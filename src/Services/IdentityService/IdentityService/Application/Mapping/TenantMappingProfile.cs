using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mapping;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users.Count))
            .ForMember(dest => dest.RoleCount, opt => opt.MapFrom(src => src.Roles.Count));

        CreateMap<CreateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<UpdateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
} 