using System;

namespace Avalanche.Api.ViewModels
{
    public abstract class FilterViewModelBase : ICloneable
    {
        public int Page { get; set; }
        public int Limit { get; set; }

        public FilterViewModelBase()
        {
            this.Page = 1;
            this.Limit = 25;
        }

        public abstract object Clone();
    }
}
