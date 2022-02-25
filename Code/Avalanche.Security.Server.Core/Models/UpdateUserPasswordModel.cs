namespace Avalanche.Security.Server.Core.Models
{
    public class UpdateUserPasswordModel
    {
        /// <summary>
        /// Unhashed Password from user input.  Handle with care.
        /// </summary>
        public string Password { get; set; }

        public string UserName { get; set; }
    }
}
