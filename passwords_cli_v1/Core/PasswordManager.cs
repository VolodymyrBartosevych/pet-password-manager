using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pet_pm.Core
{
    public class PasswordManager
    {

        private readonly IPasswordStorage storage;

        public PasswordManager(IPasswordStorage storage)
        {
            this.storage = storage;
        }

        public void AddNewPassword()
        {
            string serviceName;
            string login;
            String password;
            Console.WriteLine("Enter Service Name:");
            serviceName = Console.ReadLine() ?? string.Empty;
            Console.WriteLine("Enter Login:");
            login = Console.ReadLine() ?? string.Empty;
            Console.WriteLine("Enter Password:");
            password = Console.ReadLine() ?? string.Empty;

            PasswordEntry entry = new PasswordEntry(serviceName, login, password);
            storage.AddPassword(entry);
            Console.WriteLine("Password added successfully");

        }



        public void ViewAllPasswords()
        {
            IReadOnlyList<PasswordEntry> passwords = storage.GetAllPasswords();
            foreach (var entry in passwords)
            {
                Console.WriteLine(entry.ServiceName);
                Console.WriteLine("login:" + entry.Login);
                Console.WriteLine("password: ************"); //add revealing later
            }
        }



    }

}
