using Dapper;
using Microsoft.Data.Sqlite;
using pet_pm.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


namespace pet_pm.Storage
{
    public class DBPasswordStorage : IPasswordStorage, IDisposable
    {
        string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "p_vault", "vault.db");
        SqliteConnection connection;

        string bakDir;
        int wrCount;

        public DBPasswordStorage() 
        {
            string dirName = Path.GetDirectoryName(dbFile)!;
            bakDir = Path.Combine(Path.GetDirectoryName(dbFile)!, "backups");

            Directory.CreateDirectory(bakDir);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);



            connection = new SqliteConnection($"Data Source={dbFile}");
            connection.Open();

            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Passwords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ServiceName TEXT NOT NULL,
                Login TEXT NOT NULL,
                Password TEXT NOT NULL
                );";

            connection.Execute(createTableSql);
            Backup("Startup");

        }
        public void AddPassword(PasswordEntry entry)
        {
            string insertSql = @"
            INSERT INTO Passwords (ServiceName, Login, Password)
            VALUES (@ServiceName, @Login, @Password);";

            int maxAttempts = 3;
            int attemptCount = 0;
            int waitMs = 150;


            while (true) 
            {
                attemptCount++;
                try
                {
                    using var transaction = connection.BeginTransaction();
                    connection.Execute(insertSql, entry, transaction);
                    transaction.Commit();

                    wrCount++;
                    if (wrCount >= 15)
                    {
                        Backup("autosave");
                        wrCount = 0;
                    }
                    break;
                }
                catch (SqliteException sqlEx) when (sqlEx.SqliteErrorCode == 5 ||
                                            sqlEx.SqliteErrorCode == 6)
                {
                    if (attemptCount == maxAttempts)
                    {
                        Console.WriteLine($"DB busy after {attemptCount} attempts: {sqlEx.Message}");
                        throw new InvalidOperationException("Database is busy. Please close other applications or try again later.");
                    }

                    Thread.Sleep(waitMs);
                    waitMs *= 2;
                    continue;
                }
                catch (SqliteException sqlEx)
                {
                    Console.WriteLine("SQLite error writing DB: " + sqlEx.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error writing DB: " + ex.Message);
                    throw;
                }


            }
        }

        public IReadOnlyList<PasswordEntry> GetAllPasswords()
        {
            try 
            {
                string selectSql = "SELECT * FROM Passwords;";
                var passwords = connection.Query<PasswordEntry>(selectSql).ToList();
                return passwords.AsReadOnly();
            }
            catch (SqliteException sqlEx) when (sqlEx.SqliteErrorCode == 5 ||
                                         sqlEx.SqliteErrorCode == 6)
            {
                Console.WriteLine("Database is busy or locked. Try again in a moment. " + sqlEx.Message);
                return new List<PasswordEntry>().AsReadOnly();
            }
            catch (SqliteException sqlEx)
            {
                Console.WriteLine("SQLite error reading DB: " + sqlEx.Message);
                return new List<PasswordEntry>().AsReadOnly();
            }
        }

        public void Dispose()
        {
            Backup("Shutdown");
            connection?.Dispose();
        }

        private void Backup(string label) 
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
                string safeLabel = String.IsNullOrEmpty(label) ? "" : $"_{label}";
                string bakFile = Path.Combine(bakDir, $"vault_{timestamp}{safeLabel}.db");

                connection.Execute($"VACUUM INTO '{bakFile.Replace("'", "''")}';");

                BackupCleanup(5);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to backup: {ex.Message}");
            }
        }

        private void BackupCleanup(int keepQty) 
        {
            var files = Directory.GetFiles(bakDir, "vault_*.db").OrderByDescending(f => File.GetCreationTime(f)).Skip(keepQty);

            foreach (var file in files)
            {
                try {
                    File.Delete(file);
                } catch { 
                    
                }
            }
        }
    }
}
