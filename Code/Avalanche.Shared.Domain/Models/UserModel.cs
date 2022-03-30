namespace Avalanche.Shared.Domain.Models
{
    public class UserModel
    {
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public bool? IsAdmin { get; set; }

        //For Configuration Context
        public string? DepartmentId { get; set; }
        public string? IdnId { get; set; }
        public string? SiteId { get; set; }
        public string? SystemId { get; set; }
        public bool? AutoLogin { get; set; }
    }
}
