namespace Avalanche.Api.ViewModels
{
    public class ProcedureAllocationViewModel
    {
        public ProcedureIdViewModel ProcedureId { get; set; }
        public string RelativePath { get; set; }

        public ProcedureAllocationViewModel(ProcedureIdViewModel procedureId, string relativePath)
        {
            ProcedureId = procedureId;
            RelativePath = relativePath;
        }
    }
}
