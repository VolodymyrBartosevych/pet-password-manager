# Pet Password Manager

A simple, console-based **password manager** written in **C# (.NET)**.
It securely stores and encrypts your passwords locally using modern cryptographic standards.

---

## Features Implemented

* **Master password protection**

  * User sets a master password on first run
  * Password verification on each startup (hashed and salted verification, no plain-text storage)
* **AES-256 encryption**

  * All stored passwords are encrypted using AES in CBC mode
  * Keys are derived via PBKDF2 with SHA-256 and 310,000 iterations
* **Local database storage**

  * Passwords are stored in a local SQLite database through a data-access layer
* **Basic console UI**

  * Simple menu for adding and viewing saved passwords
  * Ability to reveal selected password entries

---

## Upcoming Features

* **Multi-user accounts**
  Each user will have a separate encrypted vault and master password.
* **Built-in password generator**
  Generate strong passwords with custom rules (length, symbols, etc.).
* **Export / Import vault**
  Allow exporting the encrypted database for backup and restoring it later.
* **UI improvements**
  More readable console menus, potential migration to a minimal GUI in the future.

---

## Tech Stack

* **Language:** C#
* **Framework:** .NET 8 (or your current version)
* **Database:** SQLite (via `Microsoft.Data.Sqlite`)
* **Encryption:** AES (CBC mode), PBKDF2 (Rfc2898DeriveBytes)

---

## Notes

* The app currently supports one local master password.
* All encryption keys are derived from the user’s master password — if forgotten, the vault cannot be recovered.
* Sensitive data such as salts, hashes, and encrypted entries are stored under:

  ```
  %AppData%\p_vault\
  ```

---

## Getting Started

1. Clone the repository

   ```bash
   git clone https://github.com/VolodymyrBartosevych/pet-password-manager.git
   ```
2. Open the project in Visual Studio or VS Code
3. Run the app

   ```bash
   dotnet run
   ```
