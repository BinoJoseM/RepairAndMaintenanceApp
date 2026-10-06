using System;

namespace RepairAndMaintenanceApp.Entities
{
    public class JournalHeader
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Reference { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string? SourceDocumentReference { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
