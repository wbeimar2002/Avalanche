using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Extensions
{
    /// <summary>
    /// Contains various extension methods for converting between different domain models
    /// </summary>
    public static class VideoRoutingExtensions
    {
        /// <summary>
        /// Converts an api aliasIndex into a fRPC routing AliasIndex
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Ism.Routing.V1.Protos.AliasIndexMessage ToRoutingAliasIndex(this AliasIndexApiModel model)
        {
            ThrowIfNull(nameof(model), model);
            return new Ism.Routing.V1.Protos.AliasIndexMessage
            {
                Alias = model.Alias,
                Index = model.Index
            };
        }

        /// <summary>
        /// Converts an api aliasIndex into a gRPC avidis aliasIndex
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AvidisDeviceInterface.V1.Protos.AliasIndexMessage ToAvidisAliasIndex(this AliasIndexApiModel model)
        {
            ThrowIfNull(nameof(model), model);
            return new AvidisDeviceInterface.V1.Protos.AliasIndexMessage
            {
                Alias = model.Alias,
                Index = model.Index
            };
        }
    }
}
