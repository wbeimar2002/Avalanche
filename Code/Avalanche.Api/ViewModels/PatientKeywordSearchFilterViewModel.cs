using Avalanche.Shared.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class PatientKeywordSearchFilterViewModel : FilterViewModelBase
    {
        public string Term { get; set; }

        public PatientKeywordSearchFilterViewModel() : base()
        {
        }

        public override object Clone()
        {
            var jsonString = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject(jsonString, this.GetType());
        }
    }
}
