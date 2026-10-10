using System;
using System.Globalization;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    // Income is recorded as Debit and expense as Credit in JournalTransactions.
    public static class ReportDataAccess
    {
        private const string ExcludeOpeningBalance = "lower(COALESCE(pm.ParticularName, '')) NOT LIKE '%open balance%'";

        public static ReportTable GetMonthlyIncomeExpenseSummary(DateTime fromDate, DateTime toDate)
        {
            var table = NewTable("Month", "Transactions", "Income (Dr)", "Expense (Cr)", "Net");
            table.NumericColumns.UnionWith(new[] { 1, 2, 3, 4 });

            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT strftime('%Y-%m', jt.EntryDate), COUNT(*), SUM(jt.Debit), SUM(jt.Credit)
                FROM JournalTransactions jt
                LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                WHERE jt.EntryDate IS NOT NULL
                    AND date(jt.EntryDate) BETWEEN date(@from) AND date(@to)
                    AND {ExcludeOpeningBalance}
                GROUP BY 1
                ORDER BY 1";
            AddRange(command, fromDate, toDate);

            decimal totalIncome = 0m, totalExpense = 0m;
            var totalCount = 0;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var income = ToDecimal(reader, 2);
                    var expense = ToDecimal(reader, 3);
                    var count = reader.GetInt32(1);
                    var month = DateTime.ParseExact(reader.GetString(0), "yyyy-MM", CultureInfo.InvariantCulture);
                    table.AddRow(false, month.ToString("MMMM yyyy", CultureInfo.InvariantCulture), count.ToString(), Money(income), Money(expense), Money(income - expense));
                    totalIncome += income;
                    totalExpense += expense;
                    totalCount += count;
                }
            }

            table.AddRow(true, "Total", totalCount.ToString(), Money(totalIncome), Money(totalExpense), Money(totalIncome - totalExpense));
            return table;
        }

        public static ReportTable GetCategorySummary(DateTime fromDate, DateTime toDate)
        {
            var table = NewTable("Category", "Transactions", "Income (Dr)", "Expense (Cr)", "Net", "% of Activity");
            table.NumericColumns.UnionWith(new[] { 1, 2, 3, 4, 5 });

            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT COALESCE(cm.CategoryName, 'Uncategorized'), COUNT(*), SUM(jt.Debit), SUM(jt.Credit)
                FROM JournalTransactions jt
                LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                LEFT JOIN CategoryMaster cm ON cm.Id = jt.CategoryId
                WHERE jt.EntryDate IS NOT NULL
                    AND date(jt.EntryDate) BETWEEN date(@from) AND date(@to)
                    AND {ExcludeOpeningBalance}
                GROUP BY cm.Id, cm.CategoryName
                ORDER BY SUM(jt.Debit) + SUM(jt.Credit) DESC";
            AddRange(command, fromDate, toDate);

            var rows = new System.Collections.Generic.List<(string Name, int Count, decimal Income, decimal Expense)>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add((reader.GetString(0), reader.GetInt32(1), ToDecimal(reader, 2), ToDecimal(reader, 3)));
                }
            }

            var grandActivity = rows.Sum(r => r.Income + r.Expense);
            foreach (var row in rows)
            {
                var share = grandActivity == 0m ? 0m : (row.Income + row.Expense) / grandActivity * 100m;
                table.AddRow(false, row.Name, row.Count.ToString(), Money(row.Income), Money(row.Expense), Money(row.Income - row.Expense), share.ToString("0.0", CultureInfo.InvariantCulture) + "%");
            }

            var income = rows.Sum(r => r.Income);
            var expense = rows.Sum(r => r.Expense);
            table.AddRow(true, "Total", rows.Sum(r => r.Count).ToString(), Money(income), Money(expense), Money(income - expense), grandActivity == 0m ? "0.0%" : "100.0%");
            return table;
        }

        public static ReportTable GetDayBook(DateTime fromDate, DateTime toDate)
        {
            var table = NewTable("Date", "Category", "Particulars", "Dr (Income)", "Cr (Expense)");
            table.NumericColumns.UnionWith(new[] { 3, 4 });

            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT jt.EntryDate, COALESCE(cm.CategoryName, ''), COALESCE(pm.ParticularName, ''), jt.Debit, jt.Credit
                FROM JournalTransactions jt
                LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                LEFT JOIN CategoryMaster cm ON cm.Id = jt.CategoryId
                WHERE jt.EntryDate IS NOT NULL
                    AND date(jt.EntryDate) BETWEEN date(@from) AND date(@to)
                    AND {ExcludeOpeningBalance}
                ORDER BY date(jt.EntryDate), jt.Id";
            AddRange(command, fromDate, toDate);

            decimal totalDebit = 0m, totalCredit = 0m;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var debit = ToDecimal(reader, 3);
                    var credit = ToDecimal(reader, 4);
                    var date = DateTime.TryParse(reader.GetString(0), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                        ? parsed.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
                        : reader.GetString(0);
                    table.AddRow(false, date, reader.GetString(1), reader.GetString(2), debit == 0m ? string.Empty : Money(debit), credit == 0m ? string.Empty : Money(credit));
                    totalDebit += debit;
                    totalCredit += credit;
                }
            }

            table.AddRow(true, "Total", string.Empty, string.Empty, Money(totalDebit), Money(totalCredit));
            return table;
        }

        public static ReportTable GetTrialBalance(DateTime fromDate, DateTime toDate)
        {
            var table = NewTable("Particulars", "Opening Dr", "Opening Cr", "Period Dr", "Period Cr", "Closing Dr", "Closing Cr");
            table.NumericColumns.UnionWith(new[] { 1, 2, 3, 4, 5, 6 });

            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT trim(pm.ParticularName),
                    SUM(CASE WHEN date(jt.EntryDate) < date(@from) THEN jt.Debit - jt.Credit ELSE 0 END),
                    SUM(CASE WHEN date(jt.EntryDate) BETWEEN date(@from) AND date(@to) THEN jt.Debit ELSE 0 END),
                    SUM(CASE WHEN date(jt.EntryDate) BETWEEN date(@from) AND date(@to) THEN jt.Credit ELSE 0 END)
                FROM JournalTransactions jt
                JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                WHERE jt.EntryDate IS NOT NULL
                    AND date(jt.EntryDate) <= date(@to)
                    AND {ExcludeOpeningBalance}
                GROUP BY pm.Id, trim(pm.ParticularName)
                HAVING SUM(CASE WHEN date(jt.EntryDate) < date(@from) THEN jt.Debit - jt.Credit ELSE 0 END) <> 0
                    OR SUM(CASE WHEN date(jt.EntryDate) BETWEEN date(@from) AND date(@to) THEN jt.Debit + jt.Credit ELSE 0 END) <> 0
                ORDER BY 1";
            AddRange(command, fromDate, toDate);

            var totals = new decimal[6];
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var opening = ToDecimal(reader, 1);
                    var periodDebit = ToDecimal(reader, 2);
                    var periodCredit = ToDecimal(reader, 3);
                    var closing = opening + periodDebit - periodCredit;
                    var values = new[]
                    {
                        Math.Max(opening, 0m), Math.Max(-opening, 0m),
                        periodDebit, periodCredit,
                        Math.Max(closing, 0m), Math.Max(-closing, 0m)
                    };

                    for (var i = 0; i < values.Length; i++)
                    {
                        totals[i] += values[i];
                    }

                    table.AddRow(false, reader.GetString(0), Money(values[0]), Money(values[1]), Money(values[2]), Money(values[3]), Money(values[4]), Money(values[5]));
                }
            }

            table.AddRow(true, "Total", Money(totals[0]), Money(totals[1]), Money(totals[2]), Money(totals[3]), Money(totals[4]), Money(totals[5]));
            return table;
        }

        private static ReportTable NewTable(params string[] headers)
        {
            var table = new ReportTable();
            table.Headers.AddRange(headers);
            return table;
        }

        private static SqliteConnection Open()
        {
            var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();
            return connection;
        }

        private static void AddRange(SqliteCommand command, DateTime fromDate, DateTime toDate)
        {
            command.Parameters.AddWithValue("@from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@to", toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        private static decimal ToDecimal(SqliteDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? 0m : Convert.ToDecimal(reader.GetValue(index));
        }

        private static string Money(decimal value)
        {
            return value.ToString("#,##0.00", CultureInfo.InvariantCulture);
        }
    }
}
