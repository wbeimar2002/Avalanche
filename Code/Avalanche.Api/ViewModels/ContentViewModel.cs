using Avalanche.Shared.Domain.Enumerations;
using System;

namespace Avalanche.Api.ViewModels
{
    public class ContentViewModel : LabelViewModel
    {
        public ProcedureContentType ProcedureContentType { get; set; } //image or video
        public Guid ContentId { get; set; }
    }
}
