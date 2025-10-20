using Microsoft.Data.Sqlite;
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
        DBPasswordStorage db;

        public EncPasswordStorage()
        {
            try
            {
                masterPassword = GetMasterPassword();
                salt = GetSalt(saltPath);
                key = GenerateKey(masterPassword, salt);

                db = new DBPasswordStorage();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File system error while setting up encryption: {ex.Message}");
                throw;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Encryption initialization failed: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during setup: {ex.Message}");
                throw;
            }
        }
        public void AddPassword(PasswordEntry entry)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                byte[] plainText = Encoding.UTF8.GetBytes(entry.Password);
                using var encryptor = aes.CreateEncryptor();
                byte[] cipher = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);

                byte[] combined = new byte[16 + cipher.Length];
                Buffer.BlockCopy(aes.IV, 0, combined, 0, 16);
                Buffer.BlockCopy(cipher, 0, combined, 16, cipher.Length);

                string cipherB64 = Convert.ToBase64String(combined);

                PasswordEntry encPasswordEntry = new PasswordEntry(entry.ServiceName, entry.Login, cipherB64);

                db.AddPassword(encPasswordEntry);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");
                throw;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }

        }

        public IReadOnlyList<PasswordEntry> GetAllPasswords()
        {
            try
            {
                IReadOnlyList<PasswordEntry> encPasswords = db.GetAllPasswords();
                List<PasswordEntry> passwords = new List<PasswordEntry>();
                foreach (var encPassword in encPasswords)
                {
                    byte[] combined = Convert.FromBase64String(encPassword.Password);

                    byte[] cipher = new byte[combined.Length - 16];
                    byte[] iv = new byte[16];

                    Buffer.BlockCopy(combined, 0, iv, 0, 16);
                    Buffer.BlockCopy(combined, 16, cipher, 0, cipher.Length);

                    using var aes = Aes.Create();
                    aes.Key = key;
                    aes.IV = iv;

                    using var decryptor = aes.CreateDecryptor();
                    byte[] plainText = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    string password = Encoding.UTF8.GetString(plainText);
                    passwords.Add(new PasswordEntry(encPassword.ServiceName, encPassword.Login, password));
                }
                return passwords.AsReadOnly();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Database read error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
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
            int iterations = 310_000;
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
            Array.Clear(key, 0, key.Length);
            masterPassword = string.Empty;
        }
    }
}
