using Avalanche.Shared.Domain.Models;
using System;
using System.Text.Json.Serialization;

namespace Avalanche.Api.ViewModels
{
    public abstract class FilterViewModelBase : ICloneable
    {
        public int Page { get; set; }
        public int Limit { get; set; }

        public FilterViewModelBase()
        {
            this.Page = 1;
            this.Limit = 100;
        }

        [JsonIgnore]
        public AccessInfo AccessInformation { get; set; }
        [JsonIgnore]
        public string CultureName { get; set; }

        public abstract object Clone();
    }
}
