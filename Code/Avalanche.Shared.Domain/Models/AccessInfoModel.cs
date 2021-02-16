using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class AccessInfoModel
    {
        public string ApplicationName { get; set; }
        public string Details { get; set; }
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
    }
}
