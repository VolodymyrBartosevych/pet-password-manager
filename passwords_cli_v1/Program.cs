using System.Text.Json;
using System.IO;

class Program {
    static void Main(string[] args)
    {
        IPasswordStorage storage = new FilePasswordStorage();
        PasswordManager manager = new PasswordManager(storage);
        Menu menu = new Menu(manager);

        menu.DisplayMainMenu();
    }

}

class PasswordManager {

    private readonly IPasswordStorage storage;

    public PasswordManager(IPasswordStorage storage)
    {
        this.storage = storage;
    }

    public void AddNewPassword() {
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



    public void ViewAllPasswords() {
        IReadOnlyList<PasswordEntry> passwords = storage.GetAllPasswords();
        foreach (var entry in passwords) { 
            Console.WriteLine(entry.ServiceName);
            Console.WriteLine("login:" + entry.Login);
            Console.WriteLine("password: ************"); //add revealing later
        }
    }
   


}

class Menu { 
    PasswordManager manager;
    bool showMenu = true;


    public Menu(PasswordManager manager) { 
        this.manager = manager;
    }

    public void DisplayMainMenu() {
        while (showMenu) {
            PauseMenu();
            MainMenu();
        }
    }

    void MainMenu() {
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

    void PauseMenu() {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}


class PasswordEntry {
    public string ServiceName { get; }
    public string Login { get; }
    public string Password { get; }
    public PasswordEntry(string serviceName, string login, string password) {
        this.ServiceName = serviceName;
        this.Login = login;
        this.Password = password;
    }
}

interface IPasswordStorage {
    void AddPassword(PasswordEntry entry);
    IReadOnlyList<PasswordEntry> GetAllPasswords();
}

class PasswordStorage : IPasswordStorage {
    List<PasswordEntry> passwords = new List<PasswordEntry>();
    
    public void AddPassword(PasswordEntry entry) {
        passwords.Add(entry);
    }

    public IReadOnlyList<PasswordEntry> GetAllPasswords() {
        return passwords.AsReadOnly();
    }
}

class FilePasswordStorage : IPasswordStorage
{
    string dirName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "p_vault");
    string fileName;
    List<PasswordEntry> passwords = new List<PasswordEntry>();

    public FilePasswordStorage() {
        fileName = Path.Combine(dirName, "Vault.json");


        if (!Directory.Exists(dirName)) {
            Directory.CreateDirectory(dirName);
        }

        if (File.Exists(fileName)) {
            LoadFromFile(fileName);
        } 
    }

    void LoadFromFile(string fileName) {
        try
        {
            Console.WriteLine("Loading file...");
            string jsonString = File.ReadAllText(fileName);
            var deserialized = JsonSerializer.Deserialize<List<PasswordEntry>>(jsonString);
            if (deserialized == null) {
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