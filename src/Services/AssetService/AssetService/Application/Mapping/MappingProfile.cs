using AutoMapper;
using AssetService.Domain.Entities;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Application.Features.Asset.Commands.CreateAsset;
using AssetService.Application.Features.Asset.Commands.UpdateAsset;

namespace AssetService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Asset, AssetDto>();
        CreateMap<Asset, AssetListDto>();
        CreateMap<Asset, AssetDetailDto>();

        // Command to Entity mappings
        CreateMap<CreateAssetCommand, Asset>();
        CreateMap<UpdateAssetCommand, Asset>();

        // DTO to Command mappings
        CreateMap<CreateAssetDto, CreateAssetCommand>();
        CreateMap<UpdateAssetDto, UpdateAssetCommand>();
    }
} 