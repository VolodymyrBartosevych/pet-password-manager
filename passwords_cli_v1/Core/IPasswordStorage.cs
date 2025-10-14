using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pet_pm.Core
{
    public interface IPasswordStorage
    {
        void AddPassword(PasswordEntry entry);
        IReadOnlyList<PasswordEntry> GetAllPasswords();
    }

}
