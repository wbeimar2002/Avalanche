using AutoFixture;
using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Ism.Common.Core.Configuration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        readonly IStorageService _storageService;
        readonly IMapper _mapper;

        public MetadataManager(IStorageService storageService, IMapper mapper)
        {
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<List<KeyValuePairViewModel>> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type, Avalanche.Shared.Domain.Models.User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);

            switch (type)
            {
                case Shared.Domain.Enumerations.MetadataTypes.Sex:
                    return (await _storageService.GetJson<ListContainerViewModel>("SexTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.ProcedureTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("ProcedureTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.ContentTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("ContentTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.SourceTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("SourceTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.Departments:
                    return (await _storageService.GetJson<ListContainerViewModel>("Departments", 1, configurationContext)).Items;
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }
    }
}