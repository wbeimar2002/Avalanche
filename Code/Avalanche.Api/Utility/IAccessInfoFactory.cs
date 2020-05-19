using Ism.IsmLogCommon.Core;
using System;

namespace Avalanche.Api.Utility
{
    public interface IAccessInfoFactory
    {
        AccessInfo GenerateAccessInfo(string details = null);
    }
}