using Dapper;
using Microsoft.Data.Sqlite;
using pet_pm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


namespace pet_pm.Storage
{
    public class DBPasswordStorage : IPasswordStorage
    {
        string dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "p_vault", "vault.db");
        SqliteConnection connection;

        public DBPasswordStorage() 
        {
            string dirName = Path.GetDirectoryName(dbFile)!;

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
        }
        public void AddPassword(PasswordEntry entry)
        {
            string insertSql = @"
            INSERT INTO Passwords (ServiceName, Login, Password)
            VALUES (@ServiceName, @Login, @Password);";

            connection.Execute(insertSql, entry);

        }

        public IReadOnlyList<PasswordEntry> GetAllPasswords()
        {
            string selectSql = "SELECT * FROM Passwords;";
            var passwords = connection.Query<PasswordEntry>(selectSql).ToList();
            return passwords.AsReadOnly();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
