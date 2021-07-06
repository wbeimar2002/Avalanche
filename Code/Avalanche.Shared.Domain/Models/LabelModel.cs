namespace Avalanche.Shared.Domain.Models
{
    public class LabelModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ProcedureTypeId { get; set; }
        public bool? IsNew { get; set; }
    }
}
