using AutoMapper;
using Avalanche.Api.ViewModels;
using Ism.Library.V1.Protos;

namespace Avalanche.Api.Mapping
{
    public class MaintenanceMappingConfiguration : Profile
    {
        public MaintenanceMappingConfiguration() =>
            CreateMap<ReindexRepositoryResponse, ReindexStatusViewModel>();
    }
}
