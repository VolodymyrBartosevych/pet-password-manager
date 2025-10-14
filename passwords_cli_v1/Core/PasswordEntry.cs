using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pet_pm.Core
{
    public class PasswordEntry
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } =  string.Empty;

        public PasswordEntry() { }
        public PasswordEntry(string serviceName, string login, string password)
        {
            this.ServiceName = serviceName;
            this.Login = login;
            this.Password = password;
        }
    }

}
