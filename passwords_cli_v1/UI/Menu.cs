using pet_pm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pet_pm.UI
{
    public class Menu
    {
        PasswordManager manager;
        bool showMenu = true;


        public Menu(PasswordManager manager)
        {
            this.manager = manager;
        }

        public void DisplayMainMenu()
        {
            while (showMenu)
            {
                PauseMenu();
                MainMenu();
            }
        }

        void MainMenu()
        {
            Console.Clear();
            Console.WriteLine("1. Add New Password");
            Console.WriteLine("2. View All Passwords");
            Console.WriteLine("3. Exit");
            Console.WriteLine("Enter your choice:");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    manager.AddNewPassword();
                    PauseMenu();
                    showMenu = true;
                    break;
                case "2":
                    manager.ViewAllPasswords();
                    PauseMenu();
                    break;
                case "3":
                    showMenu = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice");
                    PauseMenu();
                    showMenu = true;
                    break;
            }
        }

        void PauseMenu()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
