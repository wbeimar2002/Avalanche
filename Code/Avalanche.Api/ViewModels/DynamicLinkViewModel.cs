namespace Avalanche.Api.ViewModels
{
    public class DynamicLinkViewModel
    {
        public string ParentIdKey { get; set; }
        public string? ParentIdName { get; set; }
        public string Key { get; set; }
        public string? Metadata { get; set; }
        public string? Source { get; set; }
        public string TranslationKey { get; set; }
    }
}
