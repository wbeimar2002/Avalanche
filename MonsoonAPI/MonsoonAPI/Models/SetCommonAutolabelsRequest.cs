using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class SetCommonAutolabelsRequest
    {
        public string Department { get; set; }

        public List<string> Labels { get; set; }

        public SetCommonAutolabelsRequest() { }

        public SetCommonAutolabelsRequest(string department, List<string> labels)
        {
            Department = department;
            Labels = labels;
        }
    }
}
