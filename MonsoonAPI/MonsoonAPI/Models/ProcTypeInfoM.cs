using Newtonsoft.Json;

namespace MonsoonAPI.Models
{
    public class ProcTypeInfoM
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "department")]
        public string Department { get; set; }

        public ProcTypeInfoM()
        {
        }

        public ProcTypeInfoM(string name, string department)
        {
            Name = name;
            Department = department;
        }
    }
}
