using Ism.IsmLogCommon.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class GrpcModelsMappingHelper
    {
        public static Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage GetAccessInfoMessage(AccessInfo accessInfo)
        {
            return new Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage()
            {
                ApplicationName = accessInfo.ApplicationName,
                Ip = accessInfo.Ip,
                Id = accessInfo.Id.ToString(),
                Details = accessInfo.Details,
                MachineName = accessInfo.MachineName,
                UserName = accessInfo.UserName
            };
        }
    }
}
