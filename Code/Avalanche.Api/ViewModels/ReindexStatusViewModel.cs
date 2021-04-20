namespace Avalanche.Api.ViewModels
{
    public class ReindexStatusViewModel
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int TotalCount { get; set; }

        public ReindexStatusViewModel() { }

        public ReindexStatusViewModel(int successCount, int errorCount, int totalCount)
        {
            SuccessCount = successCount;
            ErrorCount = errorCount;
            TotalCount = totalCount;
        }
    }
}
