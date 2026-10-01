namespace RepairAndMaintenanceApp.Entities
{
    public class CategoryMaster
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
    }
}
