using System;
using Avalanche.Shared.Domain.Enumerations;

namespace Avalanche.Api.ViewModels
{
    public class TaskViewModel
    {
        public TaskTypes Type { get; set; }
        public TaskStatuses Status { get; set; }
        public DateTime Created { get; set; }
    }
}
