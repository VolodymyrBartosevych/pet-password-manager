using pet_pm.Core;
using System;
using System.Collections.Generic;

namespace pet_pm.UI
{
    public class Menu
    {
        private readonly PasswordManager manager;
        private bool showMenu = true;

        public Menu(PasswordManager manager)
        {
            this.manager = manager;
        }

        public void DisplayMainMenu()
        {
            while (showMenu)
            {
                Console.Clear();
                Console.WriteLine("==== Password Vault ====");
                Console.WriteLine("1. Add New Password");
                Console.WriteLine("2. View All Passwords");
                Console.WriteLine("3. Exit");
                Console.WriteLine("========================");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        manager.AddNewPassword();
                        PauseMenu();
                        break;

                    case "2":
                        ShowPasswordList();
                        break;

                    case "3":
                        showMenu = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        PauseMenu();
                        break;
                }
            }
        }

        private void ShowPasswordList()
        {
            var passwords = manager.GetAllPasswords();

            if (passwords.Count == 0)
            {
                Console.WriteLine("No passwords stored yet.");
                PauseMenu();
                return;
            }

            Console.WriteLine("=== Stored Accounts ===");
            for (int i = 0; i < passwords.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {passwords[i].ServiceName}");
            }
            Console.WriteLine("=======================");
            Console.Write("Enter the number of the account to reveal, or press Enter to return: ");

            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return;

            if (int.TryParse(input, out int index) && index > 0 && index <= passwords.Count)
            {
                var entry = passwords[index - 1];
                Console.Clear();
                Console.WriteLine($"Service: {entry.ServiceName}");
                Console.WriteLine($"Login:   {entry.Login}");
                Console.WriteLine($"Password: {entry.Password}");
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }

            PauseMenu();
        }

        private void PauseMenu()
        {
            Console.WriteLine("\nPress any key to return to the menu...");
            Console.ReadKey(true);
        }
    }
}
