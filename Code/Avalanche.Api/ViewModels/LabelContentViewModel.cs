using Avalanche.Shared.Domain.Enumerations;
using System;

namespace Avalanche.Api.ViewModels
{
    public class LabelContentViewModel
    {
        public ProcedureContentType ProcedureContentType { get; set; } //image or video
        public Guid ContentId { get; set; }
        public string Label { get; set; }
    }
}
