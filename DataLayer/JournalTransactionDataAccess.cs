using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;
using RepairAndMaintenanceApp.DataAccess;
using RepairAndMaintenanceApp.Entities;

namespace RepairAndMaintenanceApp.DataLayer
{
    public class JournalTransactionDataAccess
    {
        public static List<JournalTransaction> GetAll(DateTime? selectedMonth = null, string? particulars = null)
        {
            var transactions = new List<JournalTransaction>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            var sql = @"
                SELECT Id, EntryDate, Particulars, Debit, Credit, SourceFile, SourceRow
                FROM JournalTransactions
                WHERE lower(Particulars) NOT LIKE '%open balance%'";
            if (selectedMonth.HasValue)
            {
                sql += @" AND (
                    (date(EntryDate) >= date(@monthStart) AND date(EntryDate) < date(@nextMonthStart))
                    OR (EntryDate IS NULL AND SourceFile LIKE @sourceFilePattern)
                )";
            }

            if (!string.IsNullOrWhiteSpace(particulars))
            {
                sql += " AND Particulars = @particulars";
            }

            sql += " ORDER BY date(EntryDate), Id";

            using var command = new SqliteCommand(sql, connection);
            if (selectedMonth.HasValue)
            {
                var monthStart = new DateTime(selectedMonth.Value.Year, selectedMonth.Value.Month, 1);
                command.Parameters.AddWithValue("@monthStart", monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@nextMonthStart", monthStart.AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@sourceFilePattern", $"%{monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}%");
            }

            if (!string.IsNullOrWhiteSpace(particulars))
            {
                command.Parameters.AddWithValue("@particulars", particulars);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                transactions.Add(new JournalTransaction
                {
                    Id = reader.GetInt32(0),
                    EntryDate = reader.IsDBNull(1)
                        ? null
                        : DateTime.TryParse(reader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.None, out var entryDate)
                            ? entryDate
                            : null,
                    Particulars = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Debit = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3)),
                    Credit = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4)),
                    SourceFile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    SourceRow = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                });
            }

            return transactions;
        }

        public static List<JournalParticularBalance> GetMonthlyBalances(DateTime selectedMonth, string? particulars = null)
        {
            var balances = new List<JournalParticularBalance>();
            var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                WITH ParticularTotals AS (
                    SELECT
                        trim(Particulars) AS Particulars,
                        SUM(CASE
                            WHEN EntryDate IS NOT NULL AND date(EntryDate) < date(@monthStart)
                            THEN Debit - Credit
                            ELSE 0
                        END) AS OpeningBalance,
                        SUM(CASE
                            WHEN (date(EntryDate) >= date(@monthStart) AND date(EntryDate) < date(@nextMonthStart))
                                OR (EntryDate IS NULL AND SourceFile LIKE @sourceFilePattern)
                            THEN Debit
                            ELSE 0
                        END) AS MonthlyDebit,
                        SUM(CASE
                            WHEN (date(EntryDate) >= date(@monthStart) AND date(EntryDate) < date(@nextMonthStart))
                                OR (EntryDate IS NULL AND SourceFile LIKE @sourceFilePattern)
                            THEN Credit
                            ELSE 0
                        END) AS MonthlyCredit
                    FROM JournalTransactions
                    WHERE trim(Particulars) <> ''
                        AND lower(Particulars) NOT LIKE '%open balance%'
                        AND (@particulars IS NULL OR trim(Particulars) = @particulars)
                    GROUP BY trim(Particulars)
                )
                SELECT Particulars, OpeningBalance, MonthlyDebit, MonthlyCredit
                FROM ParticularTotals
                WHERE OpeningBalance <> 0 OR MonthlyDebit <> 0 OR MonthlyCredit <> 0
                ORDER BY Particulars
            ";
            command.Parameters.AddWithValue("@monthStart", monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@nextMonthStart", nextMonthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@sourceFilePattern", $"%{monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}%");
            command.Parameters.AddWithValue("@particulars", string.IsNullOrWhiteSpace(particulars) ? DBNull.Value : particulars);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var openingBalance = Convert.ToDecimal(reader.GetValue(1));
                var monthlyDebit = Convert.ToDecimal(reader.GetValue(2));
                var monthlyCredit = Convert.ToDecimal(reader.GetValue(3));
                var closingBalance = openingBalance + monthlyDebit - monthlyCredit;

                balances.Add(new JournalParticularBalance
                {
                    Particulars = reader.GetString(0),
                    OpeningDebit = Math.Max(openingBalance, 0m),
                    OpeningCredit = Math.Max(-openingBalance, 0m),
                    MonthlyDebit = monthlyDebit,
                    MonthlyCredit = monthlyCredit,
                    ClosingDebit = Math.Max(closingBalance, 0m),
                    ClosingCredit = Math.Max(-closingBalance, 0m)
                });
            }

            return balances;
        }

        public static List<string> GetDistinctParticulars()
        {
            var particulars = new List<string>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT DISTINCT trim(Particulars) FROM JournalTransactions WHERE trim(Particulars) <> '' AND lower(Particulars) NOT LIKE '%open balance%' ORDER BY trim(Particulars)",
                connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                particulars.Add(reader.GetString(0));
            }

            return particulars;
        }
    }
}
