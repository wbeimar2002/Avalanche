using Avalanche.Shared.Infrastructure.Configuration;
using System;

namespace Avalanche.Api.ViewModels
{
    public class HealthCheckViewModel
    {
        public DateTime UtcDateTime { get; set; }
        public DateTime LocalDateTime { get; set; }
        public FeaturesConfiguration? Features { get; set; }
    }
}
