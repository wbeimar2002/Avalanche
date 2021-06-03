namespace Avalanche.Shared.Domain.Models
{
    public class ProcedureTypeModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public bool? IsNew { get; set; }
    }
}
