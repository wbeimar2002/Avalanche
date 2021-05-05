using System;

namespace Avalanche.Shared.Domain.Models
{
    public class LicenseModel
    {
        public string Key { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string SoftwareName { get; set; }
        public Version Version { get; set; }
    }
}
