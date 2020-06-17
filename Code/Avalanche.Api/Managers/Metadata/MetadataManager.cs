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
                case Shared.Domain.Enumerations.MetadataTypes.Genders:
                    return await GetGenderTypes();
                case Shared.Domain.Enumerations.MetadataTypes.ProcedureTypes:
                    return await GetProcedureTypes();
                case Shared.Domain.Enumerations.MetadataTypes.ContentTypes:
                    return await GetContentTypes();
                case Shared.Domain.Enumerations.MetadataTypes.SourceTypes:
                    return await GetSourceTypes();
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        private async Task<List<KeyValuePairViewModel>> GetProcedureTypes()
        {
            List<KeyValuePairViewModel> list = await _storageService.GetJson<List<KeyValuePairViewModel>>("ProcedureTypes", 1);

            if (list == null || list.Count == 0)
            {
                var fixture = new Fixture();
                list = fixture.CreateMany<KeyValuePairViewModel>(10).ToList();

                await _storageService.SaveJson("ProcedureTypes", list);
            }

            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetGenderTypes()
        {
            List<KeyValuePairViewModel> list = await _storageService.GetJson<List<KeyValuePairViewModel>>("GenderTypes", 1);

            if (list == null || list.Count == 0)
            {
                list = new List<KeyValuePairViewModel>();

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "F",
                    Value = "Female",
                    TranslationKey = "genre.female"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "M",
                    Value = "Male",
                    TranslationKey = "genre.male"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "O",
                    Value = "Other",
                    TranslationKey = "genre.other"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "U",
                    Value = "Unspecified",
                    TranslationKey = "genre.unspecified"
                });

                await _storageService.SaveJson("GenderTypes", list);
            }

            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetContentTypes()
        {
            List<KeyValuePairViewModel> list = await _storageService.GetJson<List<KeyValuePairViewModel>>("ContentTypes", 1);

            if (list == null || list.Count == 0)
            {
                list = new List<KeyValuePairViewModel>();

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "G",
                    Value = "General",
                    TranslationKey = "pgsContentType.general"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "P",
                    Value = "Pediatric",
                    TranslationKey = "pgsContentType.pediatric"
                });

                await _storageService.SaveJson("ContentTypes", list);
            }

            return list;
        }

        private async Task<List<KeyValuePairViewModel>> GetSourceTypes()
        {
            List<KeyValuePairViewModel> list = await _storageService.GetJson<List<KeyValuePairViewModel>>("SourceTypes", 1);

            if (list == null || list.Count == 0)
            {
                list = new List<KeyValuePairViewModel>();

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "0",
                    Value = "Default"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "1",
                    Value = "Active Source"
                });

                list.Add(new KeyValuePairViewModel()
                {
                    Id = "1",
                    Value = "Checklist PDF"
                });

                await _storageService.SaveJson("SourceTypes", list);
            }

            return list;
        }
    }
}