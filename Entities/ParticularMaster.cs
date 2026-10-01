namespace RepairAndMaintenanceApp.Entities
{
    public class ParticularMaster
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ParticularName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
