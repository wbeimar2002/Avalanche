using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class AddDepartmentRequest
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "right")]
        public string Right { get; set; }

        public AddDepartmentRequest()
        {
        }

        public AddDepartmentRequest(string name, string right)
        {
            Name = name;
            Right = right;
        }
    }
}
