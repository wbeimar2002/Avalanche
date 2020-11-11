using AutoFixture;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        readonly IStorageService _storageService;
        //TODO: Remove mock data when storage configuration service is running

        public MetadataManager(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<List<KeyValuePairViewModel>> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type)
        {
            switch (type)
            {
                case Shared.Domain.Enumerations.MetadataTypes.Sex:
                    return await GetSexsTypes();
                case Shared.Domain.Enumerations.MetadataTypes.ProcedureTypes:
                    return await GetProcedureTypes();
                case Shared.Domain.Enumerations.MetadataTypes.ContentTypes:
                    return await GetContentTypes();
                case Shared.Domain.Enumerations.MetadataTypes.SourceTypes:
                    return await GetSourceTypes();
                case Shared.Domain.Enumerations.MetadataTypes.Departments:
                    return await GetDepartments();
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        private async Task<List<KeyValuePairViewModel>> GetDepartments()
        {
            List<KeyValuePairViewModel> list = (await _storageService.GetJson<ListContainerViewModel>("Departments", 1)).Items;
            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetProcedureTypes()
        {
            List<KeyValuePairViewModel> list = (await _storageService.GetJson<ListContainerViewModel>("ProcedureTypes", 1)).Items;
            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetSexsTypes()
        {
            List<KeyValuePairViewModel> list = (await _storageService.GetJson<ListContainerViewModel>("SexTypes", 1)).Items;
            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetContentTypes()
        {
            List<KeyValuePairViewModel> list = (await _storageService.GetJson<ListContainerViewModel>("ContentTypes", 1)).Items;
            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetSourceTypes()
        {
            List<KeyValuePairViewModel> list = (await _storageService.GetJson<ListContainerViewModel>("SourceTypes", 1)).Items;
            return list;
        }
    }
}