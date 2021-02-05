using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Avalanche.Api.ViewModels
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DynamicListViewModel 
    {
        public string TitleTranslationKey { get; set; }
        public string SourceKey { get; set; }
        public string Schema { get; set; }
        public bool InsertAllowed { get; set; }
        public bool EditAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public bool SaveAsFile { get; set; }
        public string DefaultSortProperty { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; }
        public List<ExpandoObject> Data { get; set; }
        public ExpandoObject Entity { get; set; }
        public List<ExpandoObject> NewData { get; set; }
        public List<ExpandoObject> DeletedData { get; set; }
    }
}
