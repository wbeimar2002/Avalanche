using Ism.IsmLogCommon.Core;

namespace Avalanche.Api.Utilities
{
    public interface IAccessInfoFactory
    {
        AccessInfo GenerateAccessInfo(string details = null);
    }
}