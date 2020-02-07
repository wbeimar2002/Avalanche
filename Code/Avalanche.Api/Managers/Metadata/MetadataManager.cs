using AutoFixture;
using Avalanche.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        public List<KeyValuePairViewModel> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type)
        {
            switch (type)
            {
                case Shared.Domain.Enumerations.MetadataTypes.Genders:
                    return GetGenderTypes();
                case Shared.Domain.Enumerations.MetadataTypes.ProcedureTypes:
                    return GetProcedureTypes();
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        private List<KeyValuePairViewModel> GetProcedureTypes()
        {
            var fixture = new Fixture();
            return fixture.CreateMany<KeyValuePairViewModel>(10).ToList();
        }

        private List<KeyValuePairViewModel> GetGenderTypes()
        {
            var list = new List<KeyValuePairViewModel>();
            
            list.Add(new KeyValuePairViewModel()
            {
                Id = "F",
                Value = "Female"
            });

            list.Add(new KeyValuePairViewModel()
            {
                Id = "M",
                Value = "Female"
            });

            list.Add(new KeyValuePairViewModel()
            {
                Id = "O",
                Value = "Other"
            });

            list.Add(new KeyValuePairViewModel()
            {
                Id = "U",
                Value = "Unspecified"
            });

            return list;
        }
    }
}
