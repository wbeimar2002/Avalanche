using System;

namespace Avalanche.Security.Server.Core.Models
{
    public class UserModel
    {
        private string _v;
        public UserModel()
        {

        }

        public UserModel(string v) => _v = v;

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
