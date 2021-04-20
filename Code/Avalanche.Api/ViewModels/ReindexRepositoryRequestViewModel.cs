namespace Avalanche.Api.ViewModels
{
    public class ReindexRepositoryRequestViewModel
    {
        public string RepositoryName { get; set; }

        public ReindexRepositoryRequestViewModel() { }

        public ReindexRepositoryRequestViewModel(string repositoryName)
        {
            RepositoryName = repositoryName;
        }
    }
}
