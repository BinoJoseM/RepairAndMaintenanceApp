using System.Collections.Generic;

namespace RepairAndMaintenanceApp.Entities
{
    public class ReportTable
    {
        public List<string> Headers { get; } = new();

        public List<string[]> Rows { get; } = new();

        public HashSet<int> NumericColumns { get; } = new();

        public HashSet<int> TotalRows { get; } = new();

        public void AddRow(bool isTotal, params string[] values)
        {
            if (isTotal)
            {
                TotalRows.Add(Rows.Count);
            }

            Rows.Add(values);
        }
    }
}
