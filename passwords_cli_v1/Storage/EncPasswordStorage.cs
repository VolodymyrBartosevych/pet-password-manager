using pet_pm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pet_pm.Storage
{
    internal class EncPasswordStorage : IPasswordStorage, IDisposable
    {
        string masterPassword;
        string saltPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "p_vault", "salt.bin");
        byte[] salt;
        byte[] key;
        Aes aes;
        DBPasswordStorage db;

        public EncPasswordStorage()
        { 
            masterPassword = GetMasterPassword();
            salt = GetSalt(saltPath);
            key = GenerateKey(masterPassword, salt);

            aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            db = new DBPasswordStorage();
        }
        public void AddPassword(PasswordEntry entry)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(entry.Password);
            using var encryptor = aes.CreateEncryptor();
            byte[] cipher = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
            string cipherB64 = Convert.ToBase64String(cipher);

            PasswordEntry encPasswordEntry = new PasswordEntry(entry.ServiceName, entry.Login, cipherB64);

            db.AddPassword(encPasswordEntry);

        }

        public IReadOnlyList<PasswordEntry> GetAllPasswords()
        {
            IReadOnlyList<PasswordEntry> encPasswords = db.GetAllPasswords();
            List<PasswordEntry> passwords = new List<PasswordEntry>();
            foreach (var encPassword in encPasswords)
            {
                byte[] cipher = Convert.FromBase64String(encPassword.Password);
                using var decryptor = aes.CreateDecryptor();
                byte[] plainText = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                string password = Encoding.UTF8.GetString(plainText);
                passwords.Add(new PasswordEntry(encPassword.ServiceName, encPassword.Login, password));
            }
            return passwords.AsReadOnly();
        }

        string GetMasterPassword()
        {
            Console.WriteLine("Enter your master password");
            StringBuilder password = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }

            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password.ToString();
        }

        byte[] GenerateKey(string masterPassword, byte[] salt) 
        {
            int iterations = 100000;
            int keySize = 32;

            using var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keySize);
        }

        byte[] GetSalt(string path) 
        {
            if (File.Exists(path)) 
            { 
                return File.ReadAllBytes(path);    
            }

            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            File.WriteAllBytes(path, salt);
            return salt;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
