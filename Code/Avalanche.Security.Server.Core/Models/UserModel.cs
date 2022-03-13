namespace Avalanche.Security.Server.Core.Models
{
    public abstract class UserBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
    }

    public class UserModel : PersistedUserBase
    {
        /// <summary>
        /// Hashed Password from persistence
        /// Used only to compare with provided password to verify login.
        /// Does not leave security service
        /// </summary>
        public string PasswordHash { get; set; }
    }

    public abstract class PersistedUserBase : UserBase
    {
        public int Id { get; set; }

        public bool IsAdmin { get; set; }
    }

    public class NewUserModel : UserBase
    {
        /// <summary>
        /// Unhashed Password from user input.  Handle with care.
        /// </summary>
        public string Password { get; set; }
    }

    public class UpdateUserModel : UserBase
    { }
}
