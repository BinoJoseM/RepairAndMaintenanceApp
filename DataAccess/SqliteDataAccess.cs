using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Data.Sqlite;

namespace RepairAndMaintenanceApp.DataAccess
{
    public static class SqliteDataAccess
    {
        public static string DatabasePath { get; } = ResolveDatabasePath();

        private static string ResolveDatabasePath()
        {
            var explicitPath = Environment.GetEnvironmentVariable("REPAIR_MAINTENANCE_DB_PATH");
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                return Path.GetFullPath(explicitPath);
            }

            var projectRootCandidate = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "RepairMaintenanceAccounting.db"));
            var directCandidate = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RepairMaintenanceAccounting.db"));

            if (File.Exists(projectRootCandidate))
            {
                return projectRootCandidate;
            }

            if (Directory.Exists(Path.GetDirectoryName(projectRootCandidate)!))
            {
                return projectRootCandidate;
            }

            return directCandidate;
        }

        private static void SyncDatabaseCopiesIfNeeded()
        {
            var candidateRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "RepairMaintenanceAccounting.db"));
            var runtimeDb = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RepairMaintenanceAccounting.db"));

            if (string.Equals(candidateRoot, runtimeDb, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var projectExists = File.Exists(candidateRoot);
            var runtimeExists = File.Exists(runtimeDb);

            if (!projectExists && runtimeExists)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(candidateRoot)!);
                File.Copy(runtimeDb, candidateRoot, overwrite: true);
                return;
            }

            if (projectExists && runtimeExists)
            {
                var projectWrite = File.GetLastWriteTimeUtc(candidateRoot);
                var runtimeWrite = File.GetLastWriteTimeUtc(runtimeDb);

                if (runtimeWrite > projectWrite)
                {
                    File.Copy(runtimeDb, candidateRoot, overwrite: true);
                }
                else if (projectWrite > runtimeWrite)
                {
                    File.Copy(candidateRoot, runtimeDb, overwrite: true);
                }
            }
        }

        public static void Initialize()
        {
            SyncDatabaseCopiesIfNeeded();
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath) ?? AppDomain.CurrentDomain.BaseDirectory);

            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    FullName TEXT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS JournalHeader (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NOT NULL,
                    Reference TEXT NULL,
                    Description TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Draft' CHECK (Status IN ('Draft', 'Posted', 'Void')),
                    SourceDocumentReference TEXT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS LedgerEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS BankTransactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Type TEXT NOT NULL,
                    Balance REAL NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS CategoryMaster (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryName TEXT NOT NULL UNIQUE,
                    CategoryType TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS WorkbookSheets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SheetName TEXT NOT NULL UNIQUE,
                    SourceFile TEXT NOT NULL,
                    ImportedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS WorkbookCellValues (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SheetName TEXT NOT NULL,
                    RowNumber INTEGER NOT NULL,
                    ColumnReference TEXT NOT NULL,
                    CellReference TEXT NOT NULL,
                    CellValue TEXT NULL,
                    CellType TEXT NULL,
                    ImportedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UNIQUE(SheetName, CellReference)
                );

                CREATE TABLE IF NOT EXISTS AccountingWorkbookSheets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SourceFile TEXT NOT NULL,
                    SheetName TEXT NOT NULL,
                    ImportedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UNIQUE(SourceFile, SheetName)
                );

                CREATE TABLE IF NOT EXISTS AccountingWorkbookCellValues (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SourceFile TEXT NOT NULL,
                    SheetName TEXT NOT NULL,
                    RowNumber INTEGER NOT NULL,
                    ColumnReference TEXT NOT NULL,
                    CellReference TEXT NOT NULL,
                    CellValue TEXT NULL,
                    CellType TEXT NULL,
                    ImportedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UNIQUE(SourceFile, SheetName, CellReference)
                );

                CREATE TABLE IF NOT EXISTS AccountingWorkbookImports (
                    SourceFile TEXT PRIMARY KEY,
                    LastWriteUtc TEXT NOT NULL,
                    ImportedAt TEXT NOT NULL DEFAULT (datetime('now'))
                );

                CREATE TABLE IF NOT EXISTS JournalTransactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NULL,
                    CategoryId INTEGER NULL REFERENCES CategoryMaster(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    Debit REAL NOT NULL DEFAULT 0,
                    Credit REAL NOT NULL DEFAULT 0,
                    SourceFile TEXT NOT NULL,
                    SourceRow INTEGER NOT NULL,
                    UNIQUE(SourceFile, SourceRow)
                );

                CREATE TABLE IF NOT EXISTS ExpenseLedgerEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JournalId INTEGER NULL REFERENCES JournalTransactions(Id),
                    EntryDate TEXT NULL,
                    CategoryId INTEGER NULL REFERENCES CategoryMaster(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    Amount REAL NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceCell TEXT NOT NULL,
                    UNIQUE(SourceFile, SourceCell)
                );

                CREATE TABLE IF NOT EXISTS IncomeLedgerEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JournalId INTEGER NULL REFERENCES JournalTransactions(Id),
                    EntryDate TEXT NULL,
                    CategoryId INTEGER NULL REFERENCES CategoryMaster(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    Amount REAL NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceCell TEXT NOT NULL,
                    UNIQUE(SourceFile, SourceCell)
                );

                CREATE TABLE IF NOT EXISTS BalanceSheetItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JournalId INTEGER NULL REFERENCES JournalTransactions(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    StatementTitle TEXT NOT NULL,
                    EntryDate TEXT NULL,
                    Debit REAL NOT NULL DEFAULT 0,
                    Credit REAL NOT NULL DEFAULT 0,
                    LineType TEXT NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceRow INTEGER NOT NULL,
                    UNIQUE(SourceFile, SourceRow)
                );

                CREATE TABLE IF NOT EXISTS BankReconciliationEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StatementTitle TEXT NOT NULL,
                    EntryDate TEXT NULL,
                    Particulars TEXT NOT NULL,
                    Debit REAL NOT NULL DEFAULT 0,
                    Credit REAL NOT NULL DEFAULT 0,
                    LineType TEXT NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceRow INTEGER NOT NULL,
                    UNIQUE(SourceFile, SourceRow)
                );

                CREATE TABLE IF NOT EXISTS ParticularMaster (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryId INTEGER NOT NULL REFERENCES CategoryMaster(Id),
                    ParticularName TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UNIQUE(CategoryId, ParticularName)
                );
            ";
            cmd.ExecuteNonQuery();

            MigrateJournalHeader(connection);
            MigrateCategorySchema(connection);

            SeedDefaultUserIfNeeded();
            SeedDefaultCategoriesIfNeeded();

            foreach (var workbookPath in GetWorkbookPaths())
            {
                ImportWorkbookValuesIfNeeded(workbookPath);
            }

            foreach (var workbookPath in GetWorkbookPaths())
            {
                ImportSemanticWorkbookDataIfNeeded(workbookPath);
            }

            BackfillJournalTransactionParticularIds();
            BackfillJournalTransactionCategoryIds();
            BackfillBalanceSheetParticularIds();
            BackfillBalanceSheetJournalIds();
            BackfillBalanceSheetParticularIds();
            MoveIncomeAmountsToDebit();
            SyncCategoriesFromLedgerEntries();
            SyncRuntimeDatabaseCopy();
        }

        private static void SyncRuntimeDatabaseCopy()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REPAIR_MAINTENANCE_DB_PATH")))
            {
                return;
            }

            var runtimeDatabasePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RepairMaintenanceAccounting.db"));
            if (!string.Equals(DatabasePath, runtimeDatabasePath, StringComparison.OrdinalIgnoreCase)
                && File.Exists(DatabasePath))
            {
                File.Copy(DatabasePath, runtimeDatabasePath, overwrite: true);
            }
        }

        private static void BackfillJournalTransactionParticularIds()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE JournalTransactions
                SET ParticularId = (
                    SELECT CASE WHEN COUNT(*) = 1 THEN MAX(linked.ParticularId) END
                    FROM (
                        SELECT ParticularId FROM IncomeLedgerEntries
                        WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL
                        UNION ALL
                        SELECT ParticularId FROM ExpenseLedgerEntries
                        WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL
                    ) linked
                )
                WHERE ParticularId IS NULL
                    AND (SELECT COUNT(*) FROM IncomeLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL)
                        +
                        (SELECT COUNT(*) FROM ExpenseLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL) = 1

                ;

                UPDATE JournalTransactions
                SET ParticularId = (
                    SELECT CASE WHEN COUNT(*) = 1 THEN MIN(candidates.ParticularId) END
                    FROM (
                        SELECT e.ParticularId
                        FROM IncomeLedgerEntries e
                        WHERE e.SourceFile = JournalTransactions.SourceFile
                            AND COALESCE(date(e.EntryDate), '') = COALESCE(date(JournalTransactions.EntryDate), '')
                            AND round(e.Amount, 2) = round(JournalTransactions.Credit, 2)
                            AND round(JournalTransactions.Debit, 2) = 0
                            AND e.ParticularId IS NOT NULL
                        UNION
                        SELECT e.ParticularId
                        FROM ExpenseLedgerEntries e
                        WHERE e.SourceFile = JournalTransactions.SourceFile
                            AND COALESCE(date(e.EntryDate), '') = COALESCE(date(JournalTransactions.EntryDate), '')
                            AND round(e.Amount, 2) = round(JournalTransactions.Debit, 2)
                            AND round(JournalTransactions.Credit, 2) = 0
                            AND e.ParticularId IS NOT NULL
                    ) candidates
                )
                WHERE ParticularId IS NULL
                    AND (
                        SELECT COUNT(*)
                        FROM (
                            SELECT e.ParticularId
                            FROM IncomeLedgerEntries e
                            WHERE e.SourceFile = JournalTransactions.SourceFile
                                AND COALESCE(date(e.EntryDate), '') = COALESCE(date(JournalTransactions.EntryDate), '')
                                AND round(e.Amount, 2) = round(JournalTransactions.Credit, 2)
                                AND round(JournalTransactions.Debit, 2) = 0
                                AND e.ParticularId IS NOT NULL
                            UNION
                            SELECT e.ParticularId
                            FROM ExpenseLedgerEntries e
                            WHERE e.SourceFile = JournalTransactions.SourceFile
                                AND COALESCE(date(e.EntryDate), '') = COALESCE(date(JournalTransactions.EntryDate), '')
                                AND round(e.Amount, 2) = round(JournalTransactions.Debit, 2)
                                AND round(JournalTransactions.Credit, 2) = 0
                                AND e.ParticularId IS NOT NULL
                        )
                    ) = 1
            ";
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        // Idempotent: only touches income-linked rows that still hold the amount in Credit.
        private static void MoveIncomeAmountsToDebit()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE BalanceSheetItems
                SET Debit = Credit, Credit = 0
                WHERE Credit > 0 AND Debit = 0
                    AND JournalId IN (SELECT JournalId FROM IncomeLedgerEntries WHERE JournalId IS NOT NULL);

                UPDATE JournalTransactions
                SET Debit = Credit, Credit = 0
                WHERE Credit > 0 AND Debit = 0
                    AND Id IN (SELECT JournalId FROM IncomeLedgerEntries WHERE JournalId IS NOT NULL);
            ";
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        private static void BackfillBalanceSheetJournalIds()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE BalanceSheetItems
                SET JournalId = (
                    SELECT jt.Id
                    FROM JournalTransactions jt
                    WHERE jt.SourceFile = BalanceSheetItems.SourceFile
                        AND COALESCE(date(jt.EntryDate), '') = COALESCE(date(BalanceSheetItems.EntryDate), '')
                        AND jt.ParticularId = BalanceSheetItems.ParticularId
                        AND round(jt.Debit, 2) = round(BalanceSheetItems.Debit, 2)
                        AND round(jt.Credit, 2) = round(BalanceSheetItems.Credit, 2)
                )
                WHERE JournalId IS NULL
                    AND 1 = (
                        SELECT COUNT(*)
                        FROM JournalTransactions jt
                        WHERE jt.SourceFile = BalanceSheetItems.SourceFile
                            AND COALESCE(date(jt.EntryDate), '') = COALESCE(date(BalanceSheetItems.EntryDate), '')
                            AND jt.ParticularId = BalanceSheetItems.ParticularId
                            AND round(jt.Debit, 2) = round(BalanceSheetItems.Debit, 2)
                            AND round(jt.Credit, 2) = round(BalanceSheetItems.Credit, 2)
                    )
            ";
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        private static void BackfillBalanceSheetParticularIds()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE BalanceSheetItems
                SET ParticularId = (
                    SELECT CASE WHEN
                        (SELECT COUNT(*) FROM IncomeLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL)
                        +
                        (SELECT COUNT(*) FROM ExpenseLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL) = 1
                    THEN COALESCE(
                        (SELECT ParticularId FROM IncomeLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL LIMIT 1),
                        (SELECT ParticularId FROM ExpenseLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL LIMIT 1)
                    ) END
                )
                WHERE ParticularId IS NULL
                    AND JournalId IS NOT NULL
                    AND (SELECT COUNT(*) FROM IncomeLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL)
                        +
                        (SELECT COUNT(*) FROM ExpenseLedgerEntries
                         WHERE JournalId = BalanceSheetItems.JournalId AND ParticularId IS NOT NULL) = 1
            ";
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        private static void MigrateJournalHeader(SqliteConnection connection)
        {
            using var transaction = connection.BeginTransaction();

            if (TableExists(connection, transaction, "JournalEntries"))
            {
                if (!TableExists(connection, transaction, "JournalEntries_Legacy"))
                {
                    using var copyLegacyRows = connection.CreateCommand();
                    copyLegacyRows.Transaction = transaction;
                    copyLegacyRows.CommandText = @"
                        INSERT OR IGNORE INTO JournalHeader
                            (Id, EntryDate, Reference, Description, Status, SourceDocumentReference, CreatedAt)
                        SELECT Id, EntryDate, NULL, Description, 'Draft', NULL, CreatedAt
                        FROM JournalEntries;

                        ALTER TABLE JournalEntries RENAME TO JournalEntries_Legacy;
                    ";
                    copyLegacyRows.ExecuteNonQuery();
                }
                else
                {
                    throw new InvalidOperationException(
                        "Both JournalEntries and JournalEntries_Legacy exist. Resolve the legacy journal tables before continuing.");
                }
            }

            transaction.Commit();
        }

        public static void SyncCategoriesFromLedgerEntries()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                WITH LedgerCategories AS (
                    SELECT cm.CategoryName, 'Expense' AS CategoryType
                    FROM ExpenseLedgerEntries e
                    JOIN CategoryMaster cm ON cm.Id = e.CategoryId
                    UNION ALL
                    SELECT cm.CategoryName, 'Income' AS CategoryType
                    FROM IncomeLedgerEntries e
                    JOIN CategoryMaster cm ON cm.Id = e.CategoryId
                ), DistinctCategories AS (
                    SELECT CategoryName,
                           CASE WHEN MAX(CASE WHEN CategoryType = 'Expense' THEN 1 ELSE 0 END) = 1
                                THEN 'Expense' ELSE 'Income' END AS CategoryType
                    FROM LedgerCategories
                    GROUP BY CategoryName
                )
                INSERT OR IGNORE INTO CategoryMaster (CategoryName, CategoryType, IsActive)
                SELECT CategoryName, CategoryType, 1
                FROM DistinctCategories;

            ";
            command.ExecuteNonQuery();
        }

        private static void MigrateCategorySchema(SqliteConnection connection)
        {
            using var transaction = connection.BeginTransaction();

            if (TableExists(connection, transaction, "Categories"))
            {
                using var copyCategories = connection.CreateCommand();
                copyCategories.Transaction = transaction;
                copyCategories.CommandText = @"
                    INSERT OR IGNORE INTO CategoryMaster (Id, CategoryName, CategoryType, IsActive, CreatedAt, UpdatedAt)
                    SELECT Id, CategoryName, CategoryType, IsActive, CreatedAt, UpdatedAt
                    FROM Categories;
                ";
                copyCategories.ExecuteNonQuery();
            }

            EnsureLegacyLedgerCategories(connection, transaction, "ExpenseLedgerEntries", "Expense");
            EnsureLegacyLedgerCategories(connection, transaction, "IncomeLedgerEntries", "Income");
            MigrateLedgerCategoryColumn(connection, transaction, "ExpenseLedgerEntries");
            MigrateLedgerCategoryColumn(connection, transaction, "IncomeLedgerEntries");
            EnsureLegacyLedgerParticulars(connection, transaction, "ExpenseLedgerEntries");
            EnsureLegacyLedgerParticulars(connection, transaction, "IncomeLedgerEntries");
            MigrateLedgerParticularColumn(connection, transaction, "ExpenseLedgerEntries");
            MigrateLedgerParticularColumn(connection, transaction, "IncomeLedgerEntries");
            EnsureLedgerJournalIdColumns(connection, transaction);
            MigrateJournalTransactionParticularColumn(connection, transaction);
            EnsureJournalTransactionCategoryIdColumn(connection, transaction);
            EnsureBalanceSheetEntryDateColumn(connection, transaction);
            EnsureBalanceSheetJournalIdColumn(connection, transaction);
            MigrateBalanceSheetParticularColumn(connection, transaction);

            if (TableExists(connection, transaction, "Categories"))
            {
                using var dropOldCategories = connection.CreateCommand();
                dropOldCategories.Transaction = transaction;
                dropOldCategories.CommandText = "DROP TABLE Categories";
                dropOldCategories.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static void EnsureLedgerJournalIdColumns(SqliteConnection connection, SqliteTransaction transaction)
        {
            foreach (var tableName in new[] { "ExpenseLedgerEntries", "IncomeLedgerEntries" })
            {
                if (!TableExists(connection, transaction, tableName)
                    || HasColumn(connection, transaction, tableName, "JournalId"))
                {
                    continue;
                }

                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN JournalId INTEGER NULL REFERENCES JournalTransactions(Id)";
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureBalanceSheetEntryDateColumn(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (!TableExists(connection, transaction, "BalanceSheetItems"))
            {
                return;
            }

            if (!HasColumn(connection, transaction, "BalanceSheetItems", "EntryDate"))
            {
                using var addColumn = connection.CreateCommand();
                addColumn.Transaction = transaction;
                addColumn.CommandText = "ALTER TABLE BalanceSheetItems ADD COLUMN EntryDate TEXT NULL";
                addColumn.ExecuteNonQuery();
            }

            var rowsToBackfill = new List<(long Id, string StatementTitle)>();
            using (var select = connection.CreateCommand())
            {
                select.Transaction = transaction;
                select.CommandText = "SELECT Id, StatementTitle FROM BalanceSheetItems WHERE EntryDate IS NULL";
                using var reader = select.ExecuteReader();
                while (reader.Read())
                {
                    rowsToBackfill.Add((reader.GetInt64(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1)));
                }
            }

            foreach (var row in rowsToBackfill)
            {
                var entryDate = ParseStatementDate(row.StatementTitle);
                if (!entryDate.HasValue)
                {
                    continue;
                }

                using var update = connection.CreateCommand();
                update.Transaction = transaction;
                update.CommandText = "UPDATE BalanceSheetItems SET EntryDate = @entryDate WHERE Id = @id";
                update.Parameters.AddWithValue("@entryDate", entryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                update.Parameters.AddWithValue("@id", row.Id);
                update.ExecuteNonQuery();
            }
        }

        private static void MigrateJournalTransactionParticularColumn(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (!TableExists(connection, transaction, "JournalTransactions")
                || !HasColumn(connection, transaction, "JournalTransactions", "Particulars"))
            {
                return;
            }

            var hasParticularId = HasColumn(connection, transaction, "JournalTransactions", "ParticularId");
            if (!hasParticularId)
            {
                using var addParticularId = connection.CreateCommand();
                addParticularId.Transaction = transaction;
                addParticularId.CommandText = "ALTER TABLE JournalTransactions ADD COLUMN ParticularId INTEGER NULL REFERENCES ParticularMaster(Id)";
                addParticularId.ExecuteNonQuery();
            }

            var particularIdByLedgerExpression = @"
                (
                    SELECT CASE WHEN
                        (SELECT COUNT(*) FROM IncomeLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL)
                        +
                        (SELECT COUNT(*) FROM ExpenseLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL) = 1
                    THEN COALESCE(
                        (SELECT ParticularId FROM IncomeLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL LIMIT 1),
                        (SELECT ParticularId FROM ExpenseLedgerEntries
                         WHERE JournalId = JournalTransactions.Id AND ParticularId IS NOT NULL LIMIT 1)
                    ) END
                )";
            var particularIdByNameExpression = @"(
                SELECT CASE WHEN COUNT(*) = 1 THEN MIN(pm.Id) END
                FROM ParticularMaster pm
                WHERE lower(trim(pm.ParticularName)) = lower(trim(JournalTransactions.Particulars))
            )";

            using (var backfill = connection.CreateCommand())
            {
                backfill.Transaction = transaction;
                backfill.CommandText = $@"
                    UPDATE JournalTransactions
                    SET ParticularId = COALESCE(
                        {(hasParticularId ? "ParticularId," : string.Empty)}
                        {particularIdByLedgerExpression},
                        {particularIdByNameExpression}
                    )
                    WHERE ParticularId IS NULL
                ";
                backfill.ExecuteNonQuery();
            }

            using var dropLegacyColumn = connection.CreateCommand();
            dropLegacyColumn.Transaction = transaction;
            dropLegacyColumn.CommandText = "ALTER TABLE JournalTransactions DROP COLUMN Particulars";
            dropLegacyColumn.ExecuteNonQuery();
        }

        private static void EnsureJournalTransactionCategoryIdColumn(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (!TableExists(connection, transaction, "JournalTransactions")
                || HasColumn(connection, transaction, "JournalTransactions", "CategoryId"))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "ALTER TABLE JournalTransactions ADD COLUMN CategoryId INTEGER NULL REFERENCES CategoryMaster(Id)";
            command.ExecuteNonQuery();
        }

        private static void BackfillJournalTransactionCategoryIds()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE JournalTransactions
                SET CategoryId = COALESCE(
                    (
                        SELECT CASE WHEN COUNT(*) = 1 THEN MAX(linked.CategoryId) END
                        FROM (
                            SELECT CategoryId FROM IncomeLedgerEntries
                            WHERE JournalId = JournalTransactions.Id AND CategoryId IS NOT NULL
                            UNION
                            SELECT CategoryId FROM ExpenseLedgerEntries
                            WHERE JournalId = JournalTransactions.Id AND CategoryId IS NOT NULL
                        ) linked
                    ),
                    (
                        SELECT pm.CategoryId
                        FROM ParticularMaster pm
                        WHERE pm.Id = JournalTransactions.ParticularId
                    )
                )
                WHERE CategoryId IS NULL
            ";
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        private static void EnsureBalanceSheetJournalIdColumn(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (!TableExists(connection, transaction, "BalanceSheetItems")
                || HasColumn(connection, transaction, "BalanceSheetItems", "JournalId"))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "ALTER TABLE BalanceSheetItems ADD COLUMN JournalId INTEGER NULL REFERENCES JournalTransactions(Id)";
            command.ExecuteNonQuery();
        }

        private static void MigrateBalanceSheetParticularColumn(SqliteConnection connection, SqliteTransaction transaction)
        {
            if (!TableExists(connection, transaction, "BalanceSheetItems")
                || (HasColumn(connection, transaction, "BalanceSheetItems", "ParticularId")
                    && !HasColumn(connection, transaction, "BalanceSheetItems", "Particulars")))
            {
                return;
            }

            const string migratedTableName = "BalanceSheetItems_ParticularMigration";
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
                DROP TABLE IF EXISTS {migratedTableName};
                CREATE TABLE {migratedTableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JournalId INTEGER NULL REFERENCES JournalTransactions(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    StatementTitle TEXT NOT NULL,
                    EntryDate TEXT NULL,
                    Debit REAL NOT NULL DEFAULT 0,
                    Credit REAL NOT NULL DEFAULT 0,
                    LineType TEXT NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceRow INTEGER NOT NULL,
                    UNIQUE(SourceFile, SourceRow)
                );
            ";
            command.ExecuteNonQuery();

            var hasParticularId = HasColumn(connection, transaction, "BalanceSheetItems", "ParticularId");
            var hasParticulars = HasColumn(connection, transaction, "BalanceSheetItems", "Particulars");
            var particularIdByNameExpression = hasParticulars
                ? @"(
                    SELECT CASE WHEN COUNT(*) = 1 THEN MIN(pm.Id) END
                    FROM ParticularMaster pm
                    WHERE lower(trim(pm.ParticularName)) = lower(trim(old.Particulars))
                )"
                    : "NULL";
            var particularIdByLedgerExpression = @"
                (
                    SELECT CASE WHEN
                        (SELECT COUNT(*) FROM IncomeLedgerEntries
                         WHERE JournalId = old.JournalId AND ParticularId IS NOT NULL)
                        +
                        (SELECT COUNT(*) FROM ExpenseLedgerEntries
                         WHERE JournalId = old.JournalId AND ParticularId IS NOT NULL) = 1
                    THEN COALESCE(
                        (SELECT ParticularId FROM IncomeLedgerEntries
                         WHERE JournalId = old.JournalId AND ParticularId IS NOT NULL LIMIT 1),
                        (SELECT ParticularId FROM ExpenseLedgerEntries
                         WHERE JournalId = old.JournalId AND ParticularId IS NOT NULL LIMIT 1)
                    ) END
                )";
            var particularIdExpression = hasParticularId
                ? $"COALESCE(old.ParticularId, {particularIdByLedgerExpression}, {particularIdByNameExpression})"
                : $"COALESCE({particularIdByLedgerExpression}, {particularIdByNameExpression})";

            using (var copy = connection.CreateCommand())
            {
                copy.Transaction = transaction;
                copy.CommandText = $@"
                    INSERT INTO {migratedTableName}
                        (Id, JournalId, ParticularId, StatementTitle, EntryDate, Debit, Credit, LineType, SourceFile, SourceRow)
                    SELECT
                        old.Id,
                        old.JournalId,
                        {particularIdExpression},
                        old.StatementTitle,
                        old.EntryDate,
                        old.Debit,
                        old.Credit,
                        old.LineType,
                        old.SourceFile,
                        old.SourceRow
                    FROM BalanceSheetItems old;

                    DROP TABLE BalanceSheetItems;
                    ALTER TABLE {migratedTableName} RENAME TO BalanceSheetItems;
                ";
                copy.ExecuteNonQuery();
            }
        }

        private static DateTime? ParseStatementDate(string statementTitle)
        {
            if (DateTime.TryParse(statementTitle, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var date)
                || DateTime.TryParse(statementTitle, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }

            var datePattern = @"\b(?:\d{4}[./-]\d{1,2}[./-]\d{1,2}|\d{1,2}[./-]\d{1,2}[./-]\d{2,4}|\d{1,2}\s+[A-Za-z]{3,9}\s+\d{4}|[A-Za-z]{3,9}\s+\d{1,2},?\s+\d{4}|[A-Za-z]{3,9}\s+\d{4})\b";
            foreach (Match match in Regex.Matches(statementTitle, datePattern))
            {
                if (DateTime.TryParse(match.Value, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out date)
                    || DateTime.TryParse(match.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
            }

            return null;
        }

        private static void EnsureLegacyLedgerCategories(SqliteConnection connection, SqliteTransaction transaction, string tableName, string categoryType)
        {
            if (!HasColumn(connection, transaction, tableName, "Category"))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
                INSERT OR IGNORE INTO CategoryMaster (CategoryName, CategoryType, IsActive)
                SELECT DISTINCT TRIM(Category), @categoryType, 1
                FROM {tableName}
                WHERE TRIM(Category) <> '';
            ";
            command.Parameters.AddWithValue("@categoryType", categoryType);
            command.ExecuteNonQuery();
        }

        private static void MigrateLedgerCategoryColumn(SqliteConnection connection, SqliteTransaction transaction, string tableName)
        {
            if (!HasColumn(connection, transaction, tableName, "Category") || HasColumn(connection, transaction, tableName, "CategoryId"))
            {
                return;
            }

            var migratedTableName = $"{tableName}_CategoryMigration";
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
                CREATE TABLE {migratedTableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NULL,
                    CategoryId INTEGER NULL REFERENCES CategoryMaster(Id),
                    Particulars TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceCell TEXT NOT NULL,
                    UNIQUE(SourceFile, SourceCell)
                );

                INSERT INTO {migratedTableName} (Id, EntryDate, CategoryId, Particulars, Amount, SourceFile, SourceCell)
                SELECT old.Id, old.EntryDate, cm.Id, old.Particulars, old.Amount, old.SourceFile, old.SourceCell
                FROM {tableName} old
                LEFT JOIN CategoryMaster cm ON cm.CategoryName = TRIM(old.Category);

                DROP TABLE {tableName};
                ALTER TABLE {migratedTableName} RENAME TO {tableName};
            ";
            command.ExecuteNonQuery();
        }

        private static void EnsureLegacyLedgerParticulars(SqliteConnection connection, SqliteTransaction transaction, string tableName)
        {
            if (!HasColumn(connection, transaction, tableName, "Particulars") || !HasColumn(connection, transaction, tableName, "CategoryId"))
            {
                return;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
                INSERT OR IGNORE INTO ParticularMaster (CategoryId, ParticularName)
                SELECT DISTINCT CategoryId, TRIM(Particulars)
                FROM {tableName}
                WHERE CategoryId IS NOT NULL AND TRIM(Particulars) <> '';
            ";
            command.ExecuteNonQuery();
        }

        private static void MigrateLedgerParticularColumn(SqliteConnection connection, SqliteTransaction transaction, string tableName)
        {
            if (!HasColumn(connection, transaction, tableName, "Particulars") || HasColumn(connection, transaction, tableName, "ParticularId"))
            {
                return;
            }

            var migratedTableName = $"{tableName}_ParticularMigration";
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
                CREATE TABLE {migratedTableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EntryDate TEXT NULL,
                    CategoryId INTEGER NULL REFERENCES CategoryMaster(Id),
                    ParticularId INTEGER NULL REFERENCES ParticularMaster(Id),
                    Amount REAL NOT NULL,
                    SourceFile TEXT NOT NULL,
                    SourceCell TEXT NOT NULL,
                    UNIQUE(SourceFile, SourceCell)
                );

                INSERT INTO {migratedTableName} (Id, EntryDate, CategoryId, ParticularId, Amount, SourceFile, SourceCell)
                SELECT old.Id, old.EntryDate, old.CategoryId, particular.Id, old.Amount, old.SourceFile, old.SourceCell
                FROM {tableName} old
                LEFT JOIN ParticularMaster particular
                    ON particular.CategoryId = old.CategoryId
                    AND particular.ParticularName = TRIM(old.Particulars);

                DROP TABLE {tableName};
                ALTER TABLE {migratedTableName} RENAME TO {tableName};
            ";
            command.ExecuteNonQuery();
        }

        private static bool TableExists(SqliteConnection connection, SqliteTransaction transaction, string tableName)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName";
            command.Parameters.AddWithValue("@tableName", tableName);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private static bool HasColumn(SqliteConnection connection, SqliteTransaction transaction, string tableName, string columnName)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"PRAGMA table_info({tableName})";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static long GetOrCreateCategoryId(SqliteConnection connection, SqliteTransaction transaction, string categoryName, string categoryType)
        {
            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT OR IGNORE INTO CategoryMaster (CategoryName, CategoryType, IsActive)
                    VALUES (@categoryName, @categoryType, 1)
                ";
                insert.Parameters.AddWithValue("@categoryName", categoryName.Trim());
                insert.Parameters.AddWithValue("@categoryType", categoryType);
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.Transaction = transaction;
            lookup.CommandText = "SELECT Id FROM CategoryMaster WHERE CategoryName = @categoryName";
            lookup.Parameters.AddWithValue("@categoryName", categoryName.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }

        private static long GetOrCreateParticularId(SqliteConnection connection, SqliteTransaction transaction, long categoryId, string particularName)
        {
            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT OR IGNORE INTO ParticularMaster (CategoryId, ParticularName)
                    VALUES (@categoryId, @particularName)
                ";
                insert.Parameters.AddWithValue("@categoryId", categoryId);
                insert.Parameters.AddWithValue("@particularName", particularName.Trim());
                insert.ExecuteNonQuery();
            }

            using var lookup = connection.CreateCommand();
            lookup.Transaction = transaction;
            lookup.CommandText = "SELECT Id FROM ParticularMaster WHERE CategoryId = @categoryId AND ParticularName = @particularName";
            lookup.Parameters.AddWithValue("@categoryId", categoryId);
            lookup.Parameters.AddWithValue("@particularName", particularName.Trim());
            return Convert.ToInt64(lookup.ExecuteScalar());
        }

        private static string? ResolveWorkbookPath(string workbookFileName)
        {
            var candidates = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "Documents", workbookFileName),
                Path.Combine(Environment.CurrentDirectory, "Documents", workbookFileName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Documents", workbookFileName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Documents", workbookFileName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "Documents", workbookFileName),
                Path.Combine(AppContext.BaseDirectory, "..", "Documents", workbookFileName)
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            var searchRoot = new DirectoryInfo(AppContext.BaseDirectory);
            for (var i = 0; i < 6; i++)
            {
                var matches = searchRoot.GetFiles(workbookFileName, SearchOption.AllDirectories);
                if (matches.Length > 0)
                {
                    return matches[0].FullName;
                }

                if (searchRoot.Parent == null)
                {
                    break;
                }

                searchRoot = searchRoot.Parent;
            }

            return null;
        }

        private static IEnumerable<string> GetWorkbookPaths()
        {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var documentDirectories = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Documents"),
                Path.Combine(Environment.CurrentDirectory, "Documents"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Documents"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Documents"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "Documents"),
                Path.Combine(AppContext.BaseDirectory, "..", "Documents")
            };

            foreach (var directory in documentDirectories)
            {
                var full = Path.GetFullPath(directory);
                if (Directory.Exists(full))
                {
                    foreach (var file in Directory.EnumerateFiles(full, "*.xlsx", SearchOption.AllDirectories))
                    {
                        files.Add(file);
                    }
                }
            }

            foreach (var fileName in new[] { "June 2026.xlsx", "July 2026.xlsx", "August 2026.xlsx", "September 2026.xlsx" })
            {
                var path = ResolveWorkbookPath(fileName);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    files.Add(path);
                }
            }

            return files.OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
        }

        private static void ImportWorkbookValuesIfNeeded(string workbookPath)
        {
            if (!File.Exists(workbookPath))
            {
                return;
            }

            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM AccountingWorkbookCellValues WHERE SourceFile = @sourceFile";
            checkCommand.Parameters.AddWithValue("@sourceFile", workbookPath);
            var existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
            if (existingCount > 0)
            {
                return;
            }

            using var archive = ZipFile.OpenRead(workbookPath);
            var sharedStrings = new Dictionary<int, string>();
            var sharedEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (sharedEntry != null)
            {
                using var sharedReader = new StreamReader(sharedEntry.Open());
                var sharedXml = new XmlDocument();
                sharedXml.LoadXml(sharedReader.ReadToEnd());
                var index = 0;
                var sharedNodes = sharedXml.SelectNodes("//*[local-name()='si']");
                if (sharedNodes != null)
                {
                    foreach (XmlNode node in sharedNodes)
                    {
                        var textNodes = node.SelectNodes(".//*[local-name()='t']");
                        var text = string.Empty;
                        if (textNodes != null)
                        {
                            text = string.Join(string.Empty, textNodes.Cast<XmlNode>().Select(n => n.InnerText));
                        }

                        sharedStrings[index++] = text;
                    }
                }
            }

            var workbookXml = new XmlDocument();
            using (var workbookStream = archive.GetEntry("xl/workbook.xml")?.Open())
            {
                if (workbookStream == null)
                {
                    return;
                }

                using var workbookReader = new StreamReader(workbookStream);
                workbookXml.LoadXml(workbookReader.ReadToEnd());
            }

            var workbookRelsXml = new XmlDocument();
            using (var relsStream = archive.GetEntry("xl/_rels/workbook.xml.rels")?.Open())
            {
                if (relsStream == null)
                {
                    return;
                }

                using var relsReader = new StreamReader(relsStream);
                workbookRelsXml.LoadXml(relsReader.ReadToEnd());
            }

            var relMap = new Dictionary<string, string>();
            var relNodes = workbookRelsXml.SelectNodes("//*[local-name()='Relationship']");
            if (relNodes != null)
            {
                foreach (XmlNode rel in relNodes)
                {
                    var id = rel.Attributes?["Id"]?.Value;
                    var target = rel.Attributes?["Target"]?.Value;
                    if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(target))
                    {
                        relMap[id] = target;
                    }
                }
            }

            var namespaceManager = new XmlNamespaceManager(workbookXml.NameTable);
            namespaceManager.AddNamespace("a", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            namespaceManager.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

            var sheetNodes = workbookXml.SelectNodes("//a:sheet", namespaceManager);
            if (sheetNodes != null)
            {
                foreach (XmlNode sheetNode in sheetNodes)
                {
                    var sheetName = sheetNode.Attributes?["name"]?.Value ?? string.Empty;
                    var relId = sheetNode.Attributes?["id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"]?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(sheetName) || string.IsNullOrWhiteSpace(relId) || !relMap.TryGetValue(relId, out var sheetTarget))
                    {
                        continue;
                    }

                    var normalizedTarget = sheetTarget.TrimStart('/');
                    var sheetEntry = archive.GetEntry(Path.Combine("xl", normalizedTarget).Replace('\\', '/').Replace("//", "/"));
                    if (sheetEntry == null)
                    {
                        continue;
                    }

                    using var sheetStream = sheetEntry.Open();
                    using var sheetReader = new StreamReader(sheetStream);
                    var sheetXml = new XmlDocument();
                    sheetXml.LoadXml(sheetReader.ReadToEnd());

                    using var insertSheet = connection.CreateCommand();
                    insertSheet.CommandText = @"
                        INSERT OR IGNORE INTO AccountingWorkbookSheets (SourceFile, SheetName) VALUES (@sourceFile, @sheetName);
                        INSERT OR IGNORE INTO WorkbookSheets (SheetName, SourceFile) VALUES (@sheetName, @sourceFile);
                    ";
                    insertSheet.Parameters.AddWithValue("@sheetName", sheetName);
                    insertSheet.Parameters.AddWithValue("@sourceFile", workbookPath);
                    insertSheet.ExecuteNonQuery();

                    var sheetNs = new XmlNamespaceManager(sheetXml.NameTable);
                    sheetNs.AddNamespace("a", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
                    var rowNodes = sheetXml.SelectNodes("//a:row", sheetNs);
                    if (rowNodes != null)
                    {
                        foreach (XmlNode rowNode in rowNodes)
                        {
                            var rowNumber = 0;
                            if (int.TryParse(rowNode.Attributes?["r"]?.Value, out var parsedRow))
                            {
                                rowNumber = parsedRow;
                            }

                            var cellNodes = rowNode.SelectNodes("a:c", sheetNs);
                            if (cellNodes != null)
                            {
                                foreach (XmlNode cellNode in cellNodes)
                                {
                                    var cellRef = cellNode.Attributes?["r"]?.Value ?? $"{sheetName}:{rowNumber}";
                                    var cellType = cellNode.Attributes?["t"]?.Value ?? "n";
                                    var rawValue = cellNode.SelectSingleNode("a:v", sheetNs)?.InnerText;
                                    var cellValue = rawValue;

                                    if (cellType == "s" && int.TryParse(rawValue, out var sharedIndex) && sharedStrings.TryGetValue(sharedIndex, out var sharedText))
                                    {
                                        cellValue = sharedText;
                                    }

                                    if (cellType == "inlineStr")
                                    {
                                        cellValue = cellNode.SelectSingleNode("a:is/a:t", sheetNs)?.InnerText ?? rawValue;
                                    }

                                    if (string.IsNullOrWhiteSpace(cellValue) && cellNode.SelectSingleNode("a:is", sheetNs) != null)
                                    {
                                        cellValue = cellNode.SelectSingleNode("a:is", sheetNs)?.InnerText ?? string.Empty;
                                    }

                                    if (string.IsNullOrWhiteSpace(cellValue) && string.IsNullOrWhiteSpace(rawValue))
                                    {
                                        continue;
                                    }

                                    using var insertCell = connection.CreateCommand();
                                    insertCell.CommandText = @"
                                        INSERT OR IGNORE INTO AccountingWorkbookCellValues (SourceFile, SheetName, RowNumber, ColumnReference, CellReference, CellValue, CellType)
                                        VALUES (@sourceFile, @sheetName, @rowNumber, @columnReference, @cellReference, @cellValue, @cellType);
                                        INSERT OR IGNORE INTO WorkbookCellValues (SheetName, RowNumber, ColumnReference, CellReference, CellValue, CellType)
                                        VALUES (@sheetName, @rowNumber, @columnReference, @cellReference, @cellValue, @cellType);
                                    ";
                                    insertCell.Parameters.AddWithValue("@sourceFile", workbookPath);
                                    insertCell.Parameters.AddWithValue("@sheetName", sheetName);
                                    insertCell.Parameters.AddWithValue("@rowNumber", rowNumber);
                                    insertCell.Parameters.AddWithValue("@columnReference", cellRef.Length >= 1 ? cellRef.Substring(0, 1) : cellRef);
                                    insertCell.Parameters.AddWithValue("@cellReference", cellRef);
                                    insertCell.Parameters.AddWithValue("@cellValue", (object?)cellValue ?? DBNull.Value);
                                    insertCell.Parameters.AddWithValue("@cellType", cellType);
                                    insertCell.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ImportSemanticWorkbookDataIfNeeded(string workbookPath)
        {
            if (!File.Exists(workbookPath))
            {
                return;
            }

            var lastWriteUtc = File.GetLastWriteTimeUtc(workbookPath).ToString("O", CultureInfo.InvariantCulture);
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT LastWriteUtc FROM AccountingWorkbookImports WHERE SourceFile = @sourceFile";
            checkCommand.Parameters.AddWithValue("@sourceFile", workbookPath);
            var importedLastWrite = checkCommand.ExecuteScalar()?.ToString();

            using var semanticCheckCommand = connection.CreateCommand();
            semanticCheckCommand.CommandText = @"
                SELECT COUNT(*) FROM (
                    SELECT Id FROM IncomeLedgerEntries WHERE SourceFile = @sourceFile
                    UNION ALL
                    SELECT Id FROM ExpenseLedgerEntries WHERE SourceFile = @sourceFile
                    UNION ALL
                    SELECT Id FROM JournalTransactions WHERE SourceFile = @sourceFile
                    UNION ALL
                    SELECT Id FROM BalanceSheetItems WHERE SourceFile = @sourceFile
                    UNION ALL
                    SELECT Id FROM BankReconciliationEntries WHERE SourceFile = @sourceFile
                )
            ";
            semanticCheckCommand.Parameters.AddWithValue("@sourceFile", workbookPath);
            var semanticRowsForFile = Convert.ToInt32(semanticCheckCommand.ExecuteScalar());

            if (string.Equals(importedLastWrite, lastWriteUtc, StringComparison.Ordinal) && semanticRowsForFile > 0)
            {
                return;
            }

            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var cellsCommand = connection.CreateCommand())
            {
                cellsCommand.CommandText = "SELECT SheetName, CellReference, CellValue FROM AccountingWorkbookCellValues WHERE SourceFile = @sourceFile";
                cellsCommand.Parameters.AddWithValue("@sourceFile", workbookPath);
                using var reader = cellsCommand.ExecuteReader();
                while (reader.Read())
                {
                    var key = $"{reader.GetString(0)}|{reader.GetString(1)}";
                    cells[key] = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                }
            }

            if (cells.Count == 0)
            {
                return;
            }

            string Cell(string sheet, string reference) => cells.TryGetValue($"{sheet}|{reference}", out var value) ? value.Trim() : string.Empty;
            double Amount(string sheet, string reference) =>
                double.TryParse(Cell(sheet, reference), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;

            using var transaction = connection.BeginTransaction();
            foreach (var tableName in new[] { "JournalTransactions", "ExpenseLedgerEntries", "IncomeLedgerEntries", "BalanceSheetItems", "BankReconciliationEntries" })
            {
                using var deleteCommand = connection.CreateCommand();
                deleteCommand.Transaction = transaction;
                deleteCommand.CommandText = $"DELETE FROM {tableName} WHERE SourceFile = @sourceFile";
                deleteCommand.Parameters.AddWithValue("@sourceFile", workbookPath);
                deleteCommand.ExecuteNonQuery();
            }

            using (var deleteImport = connection.CreateCommand())
            {
                deleteImport.Transaction = transaction;
                deleteImport.CommandText = "DELETE FROM AccountingWorkbookImports WHERE SourceFile = @sourceFile";
                deleteImport.Parameters.AddWithValue("@sourceFile", workbookPath);
                deleteImport.ExecuteNonQuery();
            }

            for (var row = 5; row <= 1000; row++)
            {
                var particulars = Cell("Journal", $"C{row}");
                if (string.IsNullOrWhiteSpace(particulars))
                {
                    continue;
                }

                if (particulars.Contains("Total", StringComparison.OrdinalIgnoreCase) || particulars.Contains("Closing Balance", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var debit = Amount("Journal", $"D{row}");
                var credit = Amount("Journal", $"E{row}");
                var entryDate = ConvertWorkbookDate(Cell("Journal", $"B{row}"));
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT INTO JournalTransactions (EntryDate, CategoryId, ParticularId, Debit, Credit, SourceFile, SourceRow)
                    VALUES (
                        @entryDate,
                        (SELECT CASE WHEN COUNT(*) = 1 THEN MIN(CategoryId) END
                         FROM ParticularMaster
                         WHERE lower(trim(ParticularName)) = lower(trim(@particulars))),
                        (SELECT CASE WHEN COUNT(*) = 1 THEN MIN(Id) END
                         FROM ParticularMaster
                         WHERE lower(trim(ParticularName)) = lower(trim(@particulars))),
                        @debit,
                        @credit,
                        @sourceFile,
                        @sourceRow
                    )
                ";
                insert.Parameters.AddWithValue("@entryDate", (object?)entryDate ?? DBNull.Value);
                insert.Parameters.AddWithValue("@particulars", particulars);
                insert.Parameters.AddWithValue("@debit", debit);
                insert.Parameters.AddWithValue("@credit", credit);
                insert.Parameters.AddWithValue("@sourceFile", workbookPath);
                insert.Parameters.AddWithValue("@sourceRow", row);
                insert.ExecuteNonQuery();
            }

            foreach (var block in new[]
            {
                (CategoryColumn: "C", DateColumn: "C", ParticularsColumn: "D", AmountColumn: "E"),
                (CategoryColumn: "G", DateColumn: "G", ParticularsColumn: "H", AmountColumn: "I")
            })
            {
                for (var headerRow = 1; headerRow <= 1000; headerRow++)
                {
                    var category = Cell("Ledger(Expenses)", $"{block.CategoryColumn}{headerRow}");
                    if (string.IsNullOrWhiteSpace(category) || !Cell("Ledger(Expenses)", $"{block.DateColumn}{headerRow + 1}").StartsWith("Date", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    for (var row = headerRow + 2; row <= 1000; row++)
                    {
                        var particulars = Cell("Ledger(Expenses)", $"{block.ParticularsColumn}{row}");
                        if (particulars.Contains("Grand Total", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        var amount = Amount("Ledger(Expenses)", $"{block.AmountColumn}{row}");
                        if (string.IsNullOrWhiteSpace(particulars) || amount == 0)
                        {
                            continue;
                        }

                        var categoryId = GetOrCreateCategoryId(connection, transaction, category, "Expense");
                        var particularId = GetOrCreateParticularId(connection, transaction, categoryId, particulars);
                        using var insert = connection.CreateCommand();
                        insert.Transaction = transaction;
                        insert.CommandText = @"
                            INSERT INTO ExpenseLedgerEntries (EntryDate, CategoryId, ParticularId, Amount, SourceFile, SourceCell)
                            VALUES (@entryDate, @categoryId, @particularId, @amount, @sourceFile, @sourceCell)
                        ";
                        insert.Parameters.AddWithValue("@entryDate", (object?)ConvertWorkbookDate(Cell("Ledger(Expenses)", $"{block.DateColumn}{row}")) ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@categoryId", categoryId);
                        insert.Parameters.AddWithValue("@particularId", particularId);
                        insert.Parameters.AddWithValue("@amount", amount);
                        insert.Parameters.AddWithValue("@sourceFile", workbookPath);
                        insert.Parameters.AddWithValue("@sourceCell", $"{block.DateColumn}{row}");
                        insert.ExecuteNonQuery();
                    }
                }
            }

            for (var headerRow = 1; headerRow <= 1000; headerRow++)
            {
                var category = Cell("Ledger (Income)", $"D{headerRow}");
                var nextDateCell = Cell("Ledger (Income)", $"D{headerRow + 1}");
                var nextParticularsCell = Cell("Ledger (Income)", $"E{headerRow + 1}");
                var nextAmountCell = Cell("Ledger (Income)", $"F{headerRow + 1}");
                var isIncomeHeader = !string.IsNullOrWhiteSpace(category)
                    && (nextDateCell.StartsWith("Date", StringComparison.OrdinalIgnoreCase)
                        || (nextParticularsCell.StartsWith("Particulars", StringComparison.OrdinalIgnoreCase)
                            && nextAmountCell.StartsWith("Amount", StringComparison.OrdinalIgnoreCase)));

                if (!isIncomeHeader)
                {
                    continue;
                }

                for (var row = headerRow + 2; row <= 1000; row++)
                {
                    var particulars = Cell("Ledger (Income)", $"E{row}");
                    if (particulars.Contains("Grand Total", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    var amount = Amount("Ledger (Income)", $"F{row}");
                    if (string.IsNullOrWhiteSpace(particulars) || amount == 0)
                    {
                        continue;
                    }

                    var categoryId = GetOrCreateCategoryId(connection, transaction, category, "Income");
                    var particularId = GetOrCreateParticularId(connection, transaction, categoryId, particulars);
                    using var insert = connection.CreateCommand();
                    insert.Transaction = transaction;
                    insert.CommandText = @"
                        INSERT INTO IncomeLedgerEntries (EntryDate, CategoryId, ParticularId, Amount, SourceFile, SourceCell)
                        VALUES (@entryDate, @categoryId, @particularId, @amount, @sourceFile, @sourceCell)
                    ";
                    insert.Parameters.AddWithValue("@entryDate", (object?)ConvertWorkbookDate(Cell("Ledger (Income)", $"D{row}")) ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@categoryId", categoryId);
                    insert.Parameters.AddWithValue("@particularId", particularId);
                    insert.Parameters.AddWithValue("@amount", amount);
                    insert.Parameters.AddWithValue("@sourceFile", workbookPath);
                    insert.Parameters.AddWithValue("@sourceCell", $"D{row}");
                    insert.ExecuteNonQuery();
                }
            }

            var balanceSheetTitle = Cell("BS", "A1");
            for (var row = 3; row <= 1000; row++)
            {
                var particulars = Cell("BS", $"A{row}");
                if (string.IsNullOrWhiteSpace(particulars))
                {
                    continue;
                }

                var lineType = GetStatementLineType(particulars);
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT INTO BalanceSheetItems (StatementTitle, EntryDate, ParticularId, Debit, Credit, LineType, SourceFile, SourceRow)
                    VALUES (
                        @statementTitle,
                        @entryDate,
                        (SELECT CASE WHEN COUNT(*) = 1 THEN MIN(Id) END
                         FROM ParticularMaster
                         WHERE lower(trim(ParticularName)) = lower(trim(@particulars))),
                        @debit,
                        @credit,
                        @lineType,
                        @sourceFile,
                        @sourceRow
                    )
                ";
                insert.Parameters.AddWithValue("@statementTitle", balanceSheetTitle);
                insert.Parameters.AddWithValue("@entryDate", (object?)ParseStatementDate(balanceSheetTitle)?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? DBNull.Value);
                insert.Parameters.AddWithValue("@particulars", particulars);
                insert.Parameters.AddWithValue("@debit", Amount("BS", $"B{row}"));
                insert.Parameters.AddWithValue("@credit", Amount("BS", $"C{row}"));
                insert.Parameters.AddWithValue("@lineType", lineType);
                insert.Parameters.AddWithValue("@sourceFile", workbookPath);
                insert.Parameters.AddWithValue("@sourceRow", row);
                insert.ExecuteNonQuery();
            }

            var bankStatementTitle = Cell("Bank Reconciliation Statement", "E3");
            for (var row = 5; row <= 1000; row++)
            {
                var particulars = Cell("Bank Reconciliation Statement", $"F{row}");
                if (string.IsNullOrWhiteSpace(particulars))
                {
                    continue;
                }

                var lineType = GetStatementLineType(particulars);
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT INTO BankReconciliationEntries (StatementTitle, EntryDate, Particulars, Debit, Credit, LineType, SourceFile, SourceRow)
                    VALUES (@statementTitle, @entryDate, @particulars, @debit, @credit, @lineType, @sourceFile, @sourceRow)
                ";
                insert.Parameters.AddWithValue("@statementTitle", bankStatementTitle);
                insert.Parameters.AddWithValue("@entryDate", (object?)ConvertWorkbookDate(Cell("Bank Reconciliation Statement", $"E{row}")) ?? DBNull.Value);
                insert.Parameters.AddWithValue("@particulars", particulars);
                insert.Parameters.AddWithValue("@debit", Amount("Bank Reconciliation Statement", $"G{row}"));
                insert.Parameters.AddWithValue("@credit", Amount("Bank Reconciliation Statement", $"H{row}"));
                insert.Parameters.AddWithValue("@lineType", lineType);
                insert.Parameters.AddWithValue("@sourceFile", workbookPath);
                insert.Parameters.AddWithValue("@sourceRow", row);
                insert.ExecuteNonQuery();
            }

            using (var markImported = connection.CreateCommand())
            {
                markImported.Transaction = transaction;
                markImported.CommandText = "INSERT INTO AccountingWorkbookImports (SourceFile, LastWriteUtc) VALUES (@sourceFile, @lastWriteUtc)";
                markImported.Parameters.AddWithValue("@sourceFile", workbookPath);
                markImported.Parameters.AddWithValue("@lastWriteUtc", lastWriteUtc);
                markImported.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static string GetStatementLineType(string particulars)
        {
            if (particulars.Contains("Grand Total", StringComparison.OrdinalIgnoreCase))
            {
                return "GrandTotal";
            }

            if (particulars.Contains("Closing Balance", StringComparison.OrdinalIgnoreCase))
            {
                return "ClosingBalance";
            }

            return particulars.Trim().Equals("Total", StringComparison.OrdinalIgnoreCase) ? "Total" : "Detail";
        }

        private static string? ConvertWorkbookDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var serialDate))
            {
                try
                {
                    return DateTime.FromOADate(serialDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            if (DateTime.TryParse(value, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return null;
        }

        public static void SeedDefaultUserIfNeeded()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            command.Parameters.AddWithValue("@username", "admin");

            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                return;
            }

            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO Users (Username, Password, FullName) VALUES (@username, @password, @fullName)";
            insert.Parameters.AddWithValue("@username", "admin");
            insert.Parameters.AddWithValue("@password", "admin123");
            insert.Parameters.AddWithValue("@fullName", "System Administrator");
            insert.ExecuteNonQuery();
        }

        public static bool ValidateUser(string username, string password)
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            var result = Convert.ToInt32(command.ExecuteScalar());
            return result > 0;
        }

        public static void SeedDefaultCategoriesIfNeeded()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM CategoryMaster";
            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                return;
            }

            using var insert = connection.CreateCommand();
            insert.CommandText = @"
                INSERT INTO CategoryMaster (CategoryName, CategoryType) VALUES
                    ('Service Revenue', 'Income'),
                    ('Contract Revenue', 'Income'),
                    ('Consulting', 'Income'),
                    ('Utilities', 'Expense'),
                    ('Office', 'Expense'),
                    ('Maintenance', 'Expense'),
                    ('Insurance', 'Expense'),
                    ('Travel', 'Expense');";
            insert.ExecuteNonQuery();
        }

        public static DataTable ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            var table = new DataTable();

            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            using var reader = command.ExecuteReader();
            table.Load(reader);
            return table;
        }

        public static List<string> GetCategoryNames(string categoryType)
        {
            var categories = new List<string>();
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT CategoryName
                FROM CategoryMaster
                WHERE IsActive = 1 AND CategoryType = @categoryType
                ORDER BY CategoryName
            ";
            command.Parameters.AddWithValue("@categoryType", categoryType);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString(0));
            }

            return categories;
        }

        public static int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            return command.ExecuteNonQuery();
        }
    }
}
