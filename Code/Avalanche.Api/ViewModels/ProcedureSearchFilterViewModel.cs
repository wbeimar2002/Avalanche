namespace Avalanche.Api.ViewModels
{
    public class ProcedureSearchFilterViewModel : FilterViewModelBase
    {
        public ProcedureSearchFilterViewModel() : base()
        {
        }

        public override object Clone()
        {
            return new ProcedureSearchFilterViewModel();
        }
    }
}
