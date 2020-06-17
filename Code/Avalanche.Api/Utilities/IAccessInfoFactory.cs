using Ism.IsmLogCommon.Core;
using System;

namespace Avalanche.Api.Utilities
{
    public interface IAccessInfoFactory
    {
        AccessInfo GenerateAccessInfo(string details = null);
    }
}