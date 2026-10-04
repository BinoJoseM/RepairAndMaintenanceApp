using System;

namespace RepairAndMaintenanceApp.Entities
{
    public class JournalTransaction
    {
        public int Id { get; set; }
        public DateTime? EntryDate { get; set; }
        public string Particulars { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string SourceFile { get; set; } = string.Empty;
        public int SourceRow { get; set; }
    }
}
