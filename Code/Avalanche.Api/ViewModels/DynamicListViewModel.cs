using System.Collections.Generic;
using System.Dynamic;

namespace Avalanche.Api.ViewModels
{
    public class DynamicListViewModel 
    {
        public string TitleTranslationKey { get; set; }
        public string SourceKey { get; set; }
        public string Schema { get; set; }
        public bool InsertAllowed { get; set; }
        public bool EditAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public bool SaveAsFile { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; }
        public List<ExpandoObject> Data { get; set; }
        public ExpandoObject Entity { get; set; }
    }
}
