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
        internal static (int JournalId, string SourceFile, int SourceRow) AddFromLedger(
            SqliteConnection connection,
            SqliteTransaction transaction,
            DateTime? entryDate,
            long? categoryId,
            long? particularId,
            decimal debit,
            decimal credit,
            string sourceFile)
        {
            using var nextRowCommand = connection.CreateCommand();
            nextRowCommand.Transaction = transaction;
            nextRowCommand.CommandText = "SELECT COALESCE(MAX(SourceRow), 0) + 1 FROM JournalTransactions WHERE SourceFile = @sourceFile";
            nextRowCommand.Parameters.AddWithValue("@sourceFile", sourceFile);
            var sourceRow = Convert.ToInt32(nextRowCommand.ExecuteScalar());

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT INTO JournalTransactions (EntryDate, CategoryId, ParticularId, Debit, Credit, SourceFile, SourceRow)
                VALUES (@entryDate, @categoryId, @particularId, @debit, @credit, @sourceFile, @sourceRow)
            ";
            command.Parameters.AddWithValue("@entryDate", entryDate.HasValue ? entryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@particularId", (object?)particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@debit", debit);
            command.Parameters.AddWithValue("@credit", credit);
            command.Parameters.AddWithValue("@sourceFile", sourceFile);
            command.Parameters.AddWithValue("@sourceRow", sourceRow);
            command.ExecuteNonQuery();

            using var idCommand = connection.CreateCommand();
            idCommand.Transaction = transaction;
            idCommand.CommandText = "SELECT last_insert_rowid()";
            return (Convert.ToInt32(idCommand.ExecuteScalar()), sourceFile, sourceRow);
        }

        internal static (int JournalId, string SourceFile, int SourceRow) UpdateFromLedger(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int journalId,
            DateTime? entryDate,
            long? categoryId,
            long? particularId,
            decimal debit,
            decimal credit)
        {
            string sourceFile;
            int sourceRow;
            using (var lookup = connection.CreateCommand())
            {
                lookup.Transaction = transaction;
                lookup.CommandText = "SELECT SourceFile, SourceRow FROM JournalTransactions WHERE Id = @id";
                lookup.Parameters.AddWithValue("@id", journalId);
                using var reader = lookup.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException($"Journal transaction {journalId} was not found.");
                }

                sourceFile = reader.GetString(0);
                sourceRow = reader.GetInt32(1);
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE JournalTransactions
                SET EntryDate = @entryDate,
                    CategoryId = @categoryId,
                    ParticularId = @particularId,
                    Debit = @debit,
                    Credit = @credit
                WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", journalId);
            command.Parameters.AddWithValue("@entryDate", entryDate.HasValue ? entryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", (object?)categoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@particularId", (object?)particularId ?? DBNull.Value);
            command.Parameters.AddWithValue("@debit", debit);
            command.Parameters.AddWithValue("@credit", credit);
            command.ExecuteNonQuery();

            return (journalId, sourceFile, sourceRow);
        }

        public static List<JournalTransaction> GetAll(DateTime? selectedMonth = null, string? particulars = null)
        {
            var transactions = new List<JournalTransaction>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            var sql = @"
                SELECT jt.Id, jt.CategoryId, jt.ParticularId, jt.EntryDate, COALESCE(pm.ParticularName, ''),
                    jt.Debit, jt.Credit, jt.SourceFile, jt.SourceRow
                FROM JournalTransactions jt
                LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                WHERE lower(COALESCE(pm.ParticularName, '')) NOT LIKE '%open balance%'";
            if (selectedMonth.HasValue)
            {
                sql += @" AND (
                    (date(EntryDate) >= date(@monthStart) AND date(EntryDate) < date(@nextMonthStart))
                    OR (EntryDate IS NULL AND SourceFile LIKE @sourceFilePattern)
                )";
            }

            if (!string.IsNullOrWhiteSpace(particulars))
            {
                sql += " AND pm.ParticularName = @particulars";
            }

            sql += " ORDER BY date(jt.EntryDate), jt.Id";

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
                    CategoryId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ParticularId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    EntryDate = reader.IsDBNull(3)
                        ? null
                        : DateTime.TryParse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.None, out var entryDate)
                            ? entryDate
                            : null,
                    Particulars = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Debit = reader.IsDBNull(5) ? 0m : Convert.ToDecimal(reader.GetValue(5)),
                    Credit = reader.IsDBNull(6) ? 0m : Convert.ToDecimal(reader.GetValue(6)),
                    SourceFile = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    SourceRow = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                });
            }

            return transactions;
        }

        public static List<DateTime> GetDistinctMonths()
        {
            var months = new List<DateTime>();

            using var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT strftime('%Y-%m', EntryDate)
                FROM JournalTransactions
                WHERE EntryDate IS NOT NULL AND strftime('%Y-%m', EntryDate) IS NOT NULL
                ORDER BY 1";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (DateTime.TryParseExact(reader.GetString(0), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
                {
                    months.Add(month);
                }
            }

            return months;
        }

        public static List<BalanceSheetItems> GetMonthlyBalanceSheet(DateTime selectedMonth)
        {
            var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            return BuildBalanceSheet(monthStart, GetAll(monthStart));
        }

        public static List<BalanceSheetItems> GetYearlyBalanceSheet(int year)
        {
            var yearRows = GetAll()
                .Where(x => x.EntryDate.HasValue && x.EntryDate.Value.Year == year)
                .ToList();
            return BuildBalanceSheet(new DateTime(year, 1, 1), yearRows);
        }

        public static List<int> GetDistinctYears()
        {
            return GetDistinctMonths().Select(month => month.Year).Distinct().OrderBy(year => year).ToList();
        }

        private static List<BalanceSheetItems> BuildBalanceSheet(DateTime monthStart, List<JournalTransaction> monthRows)
        {
            decimal openingNet;
            using (var connection = new SqliteConnection($"Data Source={SqliteDataAccess.DatabasePath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COALESCE(SUM(jt.Debit - jt.Credit), 0)
                    FROM JournalTransactions jt
                    LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                    WHERE jt.EntryDate IS NOT NULL
                        AND date(jt.EntryDate) < date(@monthStart)
                        AND lower(COALESCE(pm.ParticularName, '')) NOT LIKE '%open balance%'";
                command.Parameters.AddWithValue("@monthStart", monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                openingNet = Convert.ToDecimal(command.ExecuteScalar());
            }

            var monthlyDebit = monthRows.Sum(x => x.Debit);
            var monthlyCredit = monthRows.Sum(x => x.Credit);
            var totalDebit = Math.Max(openingNet, 0m) + monthlyDebit;
            var totalCredit = Math.Max(-openingNet, 0m) + monthlyCredit;
            var closingDifference = totalDebit - totalCredit;
            var closingDebit = Math.Max(-closingDifference, 0m);
            var closingCredit = Math.Max(closingDifference, 0m);

            var rows = new List<BalanceSheetItems>
            {
                new()
                {
                    EntryDate = monthStart,
                    Particulars = "Opening Balance",
                    Debit = Math.Max(openingNet, 0m),
                    Credit = Math.Max(-openingNet, 0m),
                    LineType = "Opening Balance"
                }
            };

            rows.AddRange(monthRows.Select(x => new BalanceSheetItems
            {
                JournalId = x.Id,
                ParticularId = x.ParticularId,
                EntryDate = x.EntryDate,
                Particulars = x.Particulars,
                Debit = x.Debit,
                Credit = x.Credit,
                LineType = x.Debit > 0m ? "Income" : "Expense",
                SourceFile = x.SourceFile,
                SourceRow = x.SourceRow
            }));

            rows.Add(new BalanceSheetItems { Particulars = "Total", Debit = totalDebit, Credit = totalCredit, LineType = "Total" });
            rows.Add(new BalanceSheetItems { Particulars = "Closing Balance", Debit = closingDebit, Credit = closingCredit, LineType = "Closing Balance" });
            rows.Add(new BalanceSheetItems
            {
                Particulars = "GRAND TOTAL",
                Debit = totalDebit + closingDebit,
                Credit = totalCredit + closingCredit,
                LineType = "Grand Total"
            });

            return rows;
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
                        trim(COALESCE(pm.ParticularName, '')) AS Particulars,
                        SUM(CASE
                            WHEN jt.EntryDate IS NOT NULL AND date(jt.EntryDate) < date(@monthStart)
                            THEN jt.Debit - jt.Credit
                            ELSE 0
                        END) AS OpeningBalance,
                        SUM(CASE
                            WHEN (date(jt.EntryDate) >= date(@monthStart) AND date(jt.EntryDate) < date(@nextMonthStart))
                                OR (jt.EntryDate IS NULL AND jt.SourceFile LIKE @sourceFilePattern)
                            THEN jt.Debit
                            ELSE 0
                        END) AS MonthlyDebit,
                        SUM(CASE
                            WHEN (date(jt.EntryDate) >= date(@monthStart) AND date(jt.EntryDate) < date(@nextMonthStart))
                                OR (jt.EntryDate IS NULL AND jt.SourceFile LIKE @sourceFilePattern)
                            THEN jt.Credit
                            ELSE 0
                        END) AS MonthlyCredit
                    FROM JournalTransactions jt
                    LEFT JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                    WHERE trim(COALESCE(pm.ParticularName, '')) <> ''
                        AND lower(pm.ParticularName) NOT LIKE '%open balance%'
                        AND (@particulars IS NULL OR trim(pm.ParticularName) = @particulars)
                    GROUP BY jt.ParticularId, trim(pm.ParticularName)
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
                @"SELECT DISTINCT trim(pm.ParticularName)
                  FROM JournalTransactions jt
                  JOIN ParticularMaster pm ON pm.Id = jt.ParticularId
                  WHERE trim(pm.ParticularName) <> ''
                    AND lower(pm.ParticularName) NOT LIKE '%open balance%'
                  ORDER BY trim(pm.ParticularName)",
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
