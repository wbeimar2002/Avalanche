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

        public VideoAutoEditStatus? VideoAutoEditStatus { get; set; }

        public bool IsDescending { get; set; }
        public ProcedureIndexSortingColumns ProcedureIndexSortingColumn { get; set; }

        public override object Clone()
        {
            return new ProcedureSearchFilterViewModel(); //TODO: Finish this, add constructor with parameters
        }
    }

}
