using AutoMapper;
using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.Medpresence;

namespace Avalanche.Api.Mapping
{
    public class MedpresenceMappingConfiguration : Profile
    {
        public MedpresenceMappingConfiguration() => _ = CreateMap<MedpresenceState, MedpresenceStateViewModel>();
    }
}
