using System;

namespace RepairAndMaintenanceApp.Entities
{
    public class IncomeLedgerEntries
    {
        public int Id { get; set; }
        public DateTime? EntryDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Particulars { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SourceFile { get; set; } = string.Empty;
        public string SourceCell { get; set; } = string.Empty;
    }
}
