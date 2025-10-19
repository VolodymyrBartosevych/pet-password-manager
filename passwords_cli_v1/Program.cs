using System.Text.Json;
using System.IO;

using pet_pm.Core;
using pet_pm.Storage;
using pet_pm.UI;


class Program {
    static void Main(string[] args)
    {
        using (EncPasswordStorage storage = new EncPasswordStorage())
        {
            PasswordManager manager = new PasswordManager(storage);
            Menu menu = new Menu(manager);

            menu.DisplayMainMenu();
        }
    }

}








