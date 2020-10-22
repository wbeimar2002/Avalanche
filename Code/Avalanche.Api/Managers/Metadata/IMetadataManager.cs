using Avalanche.Api.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Metadata
{
    public interface IMetadataManager
    {
        Task<List<KeyValuePairViewModel>> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type, Avalanche.Shared.Domain.Models.User user);
    }
}
