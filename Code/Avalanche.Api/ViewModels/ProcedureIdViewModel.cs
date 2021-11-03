namespace Avalanche.Api.ViewModels
{
    public class ProcedureIdViewModel
    {
        public string Id { get; set; }
        public string RepositoryName { get; set; }

        public ProcedureIdViewModel(string id, string repositoryName)
        {
            Id = id;
            RepositoryName = repositoryName;
        }
    }
}
