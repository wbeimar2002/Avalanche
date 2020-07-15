using Ism.IsmLogCommon.Core;
using Ism.Storage.Common.Core.PatientList.Proto;
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

        internal static SexMessage GetGender(string gender)
        {
            switch (gender)
            {
                case "F":
                    return SexMessage.F;
                case "M":
                    return SexMessage.M;
                case "O":
                case "U":
                default:
                    return SexMessage.U;
            }
        }
    }
}
