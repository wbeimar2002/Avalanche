using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class CookieSettings
    {
        public string Path { get; set; }
        public long ExpirationSeconds { get; set; }
    }
}
