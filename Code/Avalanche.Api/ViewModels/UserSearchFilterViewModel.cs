namespace Avalanche.Api.ViewModels
{
    public class UserSearchFilterViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Keyword { get; set; }

        public UserSearchFilterViewModel()
        {
        }

        public UserSearchFilterViewModel(string? firstName, string? lastName, string? userName, string? keyword)
        {
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            Keyword = keyword;
        }
    }
}
