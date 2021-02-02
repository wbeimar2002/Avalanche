using System.Collections.Generic;
using System.Dynamic;

namespace Avalanche.Api.ViewModels
{
    public class DynamicListViewModel : DynamicListViewModelBase
    {
        public List<ExpandoObject> Data { get; set; }
        public ExpandoObject Entity { get; set; }
    }
}
