using Avalanche.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Shared.Domain.Enumerations;

namespace Avalanche.Api.Managers.Metadata
{
    public interface IMetadataManager
    {
        List<KeyValuePairViewModel> GetMetadata(MetadataTypes type);
    }
}
