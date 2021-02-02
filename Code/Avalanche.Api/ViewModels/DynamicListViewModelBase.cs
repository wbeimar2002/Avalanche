using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class DynamicListViewModelBase
    {
        public string TitleTranslationKey { get; set; }
        public string SourceKey { get; set; }
        public bool InsertAllowed { get; set; }
        public bool EditAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public bool SaveAsFile { get; set; }
        public string Action { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; }
    }
}
