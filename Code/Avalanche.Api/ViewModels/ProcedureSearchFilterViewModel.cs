using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureSearchFilterViewModel : FilterViewModelBase
    {
        public string Keyword { get; set; }

        public DateTime? StartCreationTime { get; set; }
        public DateTime? EndCreationTime { get; set; }

        public bool? HasPendingEdits { get; set; }

        public bool IsDescending { get; set; }
        public ProcedureIndexSortingColumns ProcedureIndexSortingColumn { get; set; }

        public ProcedureSearchFilterViewModel()
        {

        }

        public ProcedureSearchFilterViewModel(string keyword, DateTime? startCreationTime, 
            DateTime? endCreationTime, bool? hasPendingEdits, bool isDescending, ProcedureIndexSortingColumns procedureIndexSortingColumn)
        {
            this.Keyword = keyword;
            this.StartCreationTime = startCreationTime;
            this.EndCreationTime = endCreationTime;
            this.HasPendingEdits = hasPendingEdits;
            this.IsDescending = IsDescending;
            this.ProcedureIndexSortingColumn = procedureIndexSortingColumn;
        }

        public override object Clone()
        {
            return new ProcedureSearchFilterViewModel(this.Keyword, this.StartCreationTime, this.EndCreationTime, this.HasPendingEdits, this.IsDescending, this.ProcedureIndexSortingColumn);
        }
    }

}
