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
        /// Converts an api aliasIndex into a gRPC routing AliasIndex
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
        /// Helper for equality checking if 2 routing alias indices are equal
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool EqualsOther(this Ism.Routing.V1.Protos.AliasIndexMessage left, Ism.Routing.V1.Protos.AliasIndexMessage right)
        {
            ThrowIfNull(nameof(left), left);
            ThrowIfNull(nameof(right), right);

            return string.Equals(left.Alias, right.Alias, StringComparison.OrdinalIgnoreCase) && left.Index == right.Index;
        }

        /// <summary>
        /// Returns true if a proto aliasIndex equals an api video device
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool EqualsVideoDevice(this Ism.Routing.V1.Protos.AliasIndexMessage left, VideoDevice right)
        {
            ThrowIfNull(nameof(left), left);
            ThrowIfNull(nameof(right), right);

            return string.Equals(left.Alias, right.Id.Alias, StringComparison.OrdinalIgnoreCase) && left.Index == right.Id.Index;
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

        /// <summary>
        /// Turn a pgstimeout pgs video file message into an api model
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static PgsVideoFile ToApiPgsVideoFile(this Ism.PgsTimeout.V1.Protos.PgsVideoFileMessage message)
        {
            ThrowIfNull(nameof(message), message);
            return new PgsVideoFile
            {
                FilePath = message.FilePath,
                Name = message.Name,
                Index = message.VideoIndex
            };
        }

        /// <summary>
        /// Turns a api pgs file model into a pgs proto model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Ism.PgsTimeout.V1.Protos.PgsVideoFileMessage ToPlayerPgsVideoFile(this PgsVideoFile model)
        {
            ThrowIfNull(nameof(model), model);
            return new Ism.PgsTimeout.V1.Protos.PgsVideoFileMessage
            {
                FilePath = model.FilePath,
                Name = model.Name,
                VideoIndex = model.Index
            };
        }
    }
}
