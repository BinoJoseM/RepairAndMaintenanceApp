using System;

namespace RepairAndMaintenanceApp.Entities
{
    public class BalanceSheetItems
    {
        public int Id { get; set; }
        public string StatementTitle { get; set; } = string.Empty;
        public DateTime? EntryDate { get; set; }
        public string Particulars { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string LineType { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public int SourceRow { get; set; }
    }
}
