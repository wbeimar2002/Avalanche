using Avalanche.Shared.Domain.Models.Media;
using Ism.Common.Core.Configuration;
using System.Collections.Generic;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    /// <summary>
    /// Represents a one source, many sinks construct. Used for pgs and timeout
    /// </summary>
    public class SingleSourceMultipleSinksConfiguration : IConfigurationPoco
    {
        public AliasIndexModel Source { get; set; }

        public List<AliasIndexModel> Sinks { get; set; }

        public virtual bool Validate()
        {
            //TODO: Pending to Resolve. In VSS context this validations fails
            //if (Source?.IsEmpty() ?? true)
            //    throw new ValidationException("Source cannot be empty");

            //if (Sinks == null)
            //    throw new ValidationException("Sinks cannot be null");

            //foreach (var sink in Sinks)
            //    if (sink?.IsEmpty() ?? true)
            //        throw new ValidationException("Sink cannot be empty");

            return true;
        }
    }
}
