namespace Avalanche.Shared.Domain.Models
{
    public class DepartmentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Does the API really care about IsNew?
        public bool? IsNew { get; set; }       
    }
}
