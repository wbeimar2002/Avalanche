using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MonsoonAPI.Models
{
    public class LoggedInUserInfo
    {
        [JsonIgnore]
        public string Username { get; set; }

        [JsonProperty("username")]
        private string UrlEncodedUsername
        {
            get { return HttpUtility.UrlEncode(Username); }
            set { Username = HttpUtility.UrlDecode(value); }
        }


        [JsonIgnore]
        public string SourceIp { get; set; }

        [JsonProperty("sourceIp")]
        private string UrlEncodedSourceIp
        {
            get { return HttpUtility.UrlEncode(SourceIp); }
            set { SourceIp = HttpUtility.UrlDecode(value); }
        }

        public LoggedInUserInfo()
        {
        }

        public LoggedInUserInfo(string username, string sourceIp)
        {
            Username = username;
            SourceIp = sourceIp;
        }
    }
}
