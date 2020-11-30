using System;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class PagedCollectionViewModel<T> where T : class
    {
        public IEnumerable<T> Items { get; set; }
        public Uri NextPage { get; set; }
        public Uri PreviousPage { get; set; }
    }
}