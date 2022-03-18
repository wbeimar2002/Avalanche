namespace Avalanche.Api.ViewModels
{
    public class ProcedureIdViewModel
    {
        public string Id { get; set; }
        public string RepositoryId { get; set; }

        public ProcedureIdViewModel(string id, string repositoryName)
        {
            Id = id;
            RepositoryId = repositoryName;
        }
    }
}
