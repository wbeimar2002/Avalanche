using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class DynamicListViewModel
    {
        public string TitleTranslationKey { get; set; }
        public string SourceKey { get; set; }
        public bool InsertAllowed { get; set; }
        public bool EditAllowed { get; set; }
        public bool RemoveAllowed { get; set; }
        public bool SaveAsFile { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; }
        public List<ExpandoObject> Data { get; set; }

    }
}
