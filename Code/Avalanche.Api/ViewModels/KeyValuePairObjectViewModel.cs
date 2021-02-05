using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class KeyValuePairObjectViewModel : KeyValuePairViewModel
    {
        public ExpandoObject RelatedObject { get; set; }
    }
}
