namespace Avalanche.Security.Server.Core.Entities
{
    public class UserEntity
    {
        public string FirstName { get; set; }
        public int Id { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string UserName { get; set; }
    }
}
