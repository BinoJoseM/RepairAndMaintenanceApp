namespace RepairAndMaintenanceApp.Entities
{
    public class JournalParticularBalance
    {
        public string Particulars { get; set; } = string.Empty;
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal MonthlyDebit { get; set; }
        public decimal MonthlyCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }
}
