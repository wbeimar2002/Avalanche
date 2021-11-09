using System.Collections.Generic;

namespace Avalanche.Security.Server.Controllers.Resources
{
    public class UserResource
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
