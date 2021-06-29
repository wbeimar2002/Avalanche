using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureSearchFilterViewModel : FilterViewModelBase
    {
        public bool IsDescending { get; set; }
        public ProcedureIndexSortingColumns ProcedureIndexSortingColumn { get; set; }

        public override object Clone()
        {
            return new ProcedureSearchFilterViewModel(); //TODO: Finish this
        }
    }

}
