using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class PgsSinkStateViewModel 
    {
        public AliasIndexViewModel Sink { get; set; }
        public bool Enabled { get; set; }
    }
}
