using pet_pm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pet_pm.Storage
{
    public class FilePasswordStorage : IPasswordStorage
    {
        string dirName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "p_vault");
        string fileName;
        List<PasswordEntry> passwords = new List<PasswordEntry>();

        public FilePasswordStorage()
        {
            fileName = Path.Combine(dirName, "Vault.json");


            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            if (File.Exists(fileName))
            {
                LoadFromFile(fileName);
            }
        }

        void LoadFromFile(string fileName)
        {
            try
            {
                Console.WriteLine("Loading file...");
                string jsonString = File.ReadAllText(fileName);
                var deserialized = JsonSerializer.Deserialize<List<PasswordEntry>>(jsonString);
                if (deserialized == null)
                {
                    throw new JsonException("Vault file is empty or invalid");
                }
                passwords = deserialized;
                Console.WriteLine("File loaded successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: Could not read or parse the password file. Reason: {e.Message}");
                string corruptFileName = $"{fileName}.corrupt.{DateTime.Now:yyyyMMddHHmmss}";
                try
                {
                    File.Move(fileName, corruptFileName);
                    Console.WriteLine($"Corrupted file renamed to {corruptFileName}");
                    Console.WriteLine("Starting with an empty password vault.");
                    passwords = new List<PasswordEntry>();
                }
                catch (Exception moveEx)
                {
                    Console.WriteLine($"Could not rename corrupted file. Reason: {moveEx.Message}");
                    Console.WriteLine("Do you want to start with an empty passsword vault? [y/n]");
                    string? choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "y":
                            Console.WriteLine("Starting with an empty password vault.");
                            passwords = new List<PasswordEntry>();
                            break;
                        case "n":
                            break;

                    }
                }

            }
        }

        void IPasswordStorage.AddPassword(PasswordEntry entry)
        {
            passwords.Add(entry);
            string jsonString = JsonSerializer.Serialize(passwords);
            File.WriteAllText(fileName, jsonString);
        }

        IReadOnlyList<PasswordEntry> IPasswordStorage.GetAllPasswords()
        {
            return passwords.AsReadOnly();
        }
    }
}
