# Mini Database Project - Complete Beginner's Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Project Overview](#project-overview)
3. [Technologies Used](#technologies-used)
4. [Folder Structure Explained](#folder-structure-explained)
5. [How the Application Works](#how-the-application-works)
6. [Step-by-Step Architecture](#step-by-step-architecture)
7. [Authentication Flow](#authentication-flow)
8. [MFA Flow](#mfa-flow)
9. [Database Wizard Flow](#database-wizard-flow)
10. [Logging System](#logging-system)
11. [Configuration System](#configuration-system)
12. [Exception Handling](#exception-handling)
13. [Validation System](#validation-system)
14. [Console UI System](#console-ui-system)
15. [Coding Standards](#coding-standards)
16. [How to Set Up and Run](#how-to-set-up-and-run)
17. [Common Errors and Solutions](#common-errors-and-solutions)
18. [Future Improvements](#future-improvements)

---

## Introduction

Welcome to the **Mini Database Project**! This is a professional console application built with C# and .NET. The application allows users to log in securely, manage their profile, and create databases from SQL scripts.

This documentation is written for **beginners**. Every folder, file, and concept is explained in simple language so you can understand, modify, and extend the project.

---

## Project Overview

The Mini Database Project is a **Console Application** that:

- Starts with a **secure login screen** (supports MFA)
- Shows a **professional dashboard** with menu options
- Lets you **manage your profile** (update info, change password, enable MFA)
- Provides a **Database Creation Wizard** to create databases from SQL scripts
- Shows **real-time progress** when creating databases
- Has a **complete logging system** with a log viewer
- Includes **Help** and **About** sections

---

## Technologies Used

| Technology | Purpose |
|------------|---------|
| **C# 13** | The programming language used |
| **.NET 10.0** | The framework the application runs on |
| **Microsoft.Extensions.DependencyInjection** | For dependency injection (managing objects) |
| **Microsoft.Extensions.Configuration.Json** | For reading appsettings.json configuration |
| **Microsoft.Extensions.Configuration.Binder** | For binding configuration to C# objects |
| **PBKDF2 (RFC 2898)** | For secure password hashing |
| **ANSI Escape Codes** | For colored console output |
| **Unicode Characters** | For borders, icons, and progress bars |

### Why These Packages?

- **Dependency Injection**: Makes the code organized and testable. Instead of creating objects manually, DI creates them for you and manages their dependencies.
- **Configuration**: Allows you to change app settings (like server names, log paths) without changing the code.
- **PBKDF2 Password Hashing**: Industry-standard secure way to store passwords. Even if someone gets the database, they cannot read the passwords.

---

## Folder Structure Explained

```
MiniDbProject/
|
+-- Program.cs                    # Entry point of the application
+-- appsettings.json              # Configuration file (settings you can change)
+-- PROJECT_DOCUMENTATION.md      # This file
|
+-- Authentication/               # Login and security logic
|   +-- IAuthenticationService.cs # Interface (contract) for authentication
|   +-- AuthenticationService.cs  # Implementation of authentication
|
+-- Configuration/               # Configuration classes
|   +-- AppConfiguration.cs       # Maps appsettings.json to C# objects
|
+-- Constants/                   # Fixed values used everywhere
|   +-- AppConstants.cs           # Colors, icons, menu keys, log levels
|
+-- Database/                    # Database creation logic
|   +-- IDatabaseService.cs       # Interface for database operations
|   +-- DatabaseService.cs        # Implementation (demo/stub version)
|   +-- IProgressTracker.cs       # Interface for tracking progress
|
+-- DTOs/                        # Data Transfer Objects
|   +-- LoginRequest.cs           # Data needed for login
|   +-- LoginResponse.cs          # Data returned after login
|   +-- ChangePasswordRequest.cs  # Data for password change
|   +-- ProfileUpdateRequest.cs   # Data for profile update
|   +-- DatabaseSelectionDto.cs   # Data for database selection
|
+-- Exceptions/                  # Custom error types
|   +-- AppException.cs           # Base exception for the application
|   +-- AuthenticationException.cs # Login-related errors
|
+-- Helpers/                     # Helper/utility classes
|   +-- ConsoleHelper.cs          # All console output methods
|   +-- InputHelper.cs            # Getting user input safely
|   +-- PasswordHelper.cs         # Password hashing and validation
|   +-- ValidationHelper.cs       # Input validation functions
|
+-- Logging/                     # Logging system
|   +-- ILoggerService.cs         # Interface for logging
|   +-- LoggerService.cs          # Implementation (in-memory + file)
|
+-- Models/                      # Data models/classes
|   +-- User.cs                   # User account information
|   +-- LogEntry.cs               # A single log entry
|   +-- MenuItem.cs               # A menu item
|   +-- ServerPortal.cs           # A server portal with databases
|   +-- DatabaseInfo.cs           # Information about a database
|   +-- ProgressInfo.cs           # Progress tracking information
|   +-- WizardStep.cs             # A step in the database wizard
|
+-- Resources/                   # Text messages used in the app
|   +-- Messages.cs               # All user-facing messages
|
+-- Services/                    # Business logic services
|   +-- IProfileService.cs        # Interface for profile management
|   +-- ProfileService.cs         # Implementation of profile management
|
+-- Utilities/                   # Small utility classes
|   +-- ConsoleSpinner.cs         # Animated loading spinner
|   +-- SecureStringHelper.cs     # Secure string handling
|   +-- TimeHelper.cs             # Time formatting functions
|
+-- Validation/                  # Validation classes
|   +-- (included in Helpers/)
|
+-- Views/                       # The UI layer (what the user sees)
|   +-- IView.cs                  # Interface for all views
|   +-- ConsoleRenderer.cs        # Drawing functions (headers, boxes)
|   +-- MenuSystem.cs             # Interactive menu system
|   +-- LoginView.cs              # Login screen
|   +-- DashboardView.cs          # Main dashboard after login
|   +-- ProfileView.cs            # Profile management screen
|   +-- DatabaseWizardView.cs     # Database creation wizard
|   +-- ProgressView.cs           # Real-time progress display
|   +-- LogViewerView.cs          # Log browsing screen
|   +-- HelpView.cs               # Help and documentation screen
|   +-- AboutView.cs              # About the application screen
```

### What Each Folder Does

- **Authentication**: Handles user login, logout, password verification, and MFA.
- **Configuration**: Reads appsettings.json and creates easy-to-use C# objects.
- **Constants**: Stores values that don't change, like color codes and icon characters.
- **Database**: Contains all logic related to creating databases from SQL scripts.
- **DTOs**: Small classes that carry data between different parts of the application.
- **Exceptions**: Custom error types for different error scenarios.
- **Helpers**: Utility functions that make common tasks easier (console output, input, validation).
- **Logging**: Records what the application does for debugging and monitoring.
- **Models**: Core data structures that represent real-world things (User, Database, etc.).
- **Resources**: All text messages in one place so they are easy to find and change.
- **Services**: Business logic for profile management.
- **Utilities**: Small, reusable tools (spinner animation, time formatting).
- **Views**: The user interface - what users see and interact with.

---

## How the Application Works

### Flow of Execution

1. **Program.cs** starts the application
2. **Dependency Injection** sets up all the services
3. **Application.RunAsync()** runs the main loop
4. **LoginView** asks for username and password
5. After login, **DashboardView** shows the main menu
6. User selects an option (Profile, Wizard, Logs, Help, About)
7. The selected view runs
8. User can go back to Dashboard or logout

### The Main Loop

```csharp
// In Program.cs -> Application.RunAsync()

while (true)          // Keep running until user exits
{
    await _loginView.ShowAsync();      // Show login screen
    await _dashboardView.ShowAsync();  // Show dashboard
    // After dashboard returns, ask if user wants to login again
}
```

---

## Authentication Flow

### Step-by-Step Login Process

1. User enters **username** and **password**
2. `AuthenticationService.LoginAsync()` is called
3. The service checks the credentials against demo accounts:
   - `admin` / `Admin@123`
   - `user` / `User@123`
   - `demo` / `Demo@123`
4. If the user has MFA enabled, they are asked for an **MFA code**
5. If valid, the user is logged in and a `User` object is created
6. If invalid, an error is shown

### Why Passwords Are Not Stored in Plain Text

In a real application, passwords are **hashed** (scrambled) using PBKDF2. The `PasswordHelper` class shows how this works:

- **Salting**: A random salt is generated for each user
- **Hashing**: The password + salt is hashed 100,000 times
- **Verification**: The same process is repeated and compared

This means:
- Even if someone steals the database, they cannot read passwords
- Each user has a unique salt, so two users with the same password have different hashes

### Lockout Mechanism

- After 5 failed attempts, the account is locked for 15 minutes
- This prevents brute-force attacks

---

## MFA Flow

Multi-Factor Authentication adds an extra layer of security.

### How MFA Works in This Application

1. User enables MFA in their **Profile**
2. A **secret key** is generated (in real app, this is a QR code)
3. User adds this secret to an authenticator app (Google Authenticator, Microsoft Authenticator)
4. On login, the app asks for a **6-digit code** from the authenticator app
5. The code changes every 30 seconds
6. Without the correct code, login fails

### Demo MFA

In demo mode, you can enable/disable MFA but the actual code validation is simulated.

---

## Database Wizard Flow

### The 4-Step Wizard

#### Step 1: Source Selection
- Application loads available **server portals** (Production, Development, Staging, Analytics)
- Each portal has multiple **databases**
- User selects a portal, then a database

#### Step 2: Destination Configuration
- User enters the **local SQL Server instance** (e.g., `(localdb)\MSSQLLocalDB`)
- **IMPORTANT**: Only local instances are allowed - this is enforced by validation
- User enters the **destination database name**

#### Step 3: Validation
- Source database is validated
- Destination instance is checked (must be local)
- SQL script file is checked (if exists)
- Database size is estimated

#### Step 4: Summary & Confirmation
- All selections are displayed in a summary
- Dependencies are listed
- User confirms to start the process

### Progress Screen

When database creation starts:
- A **real-time progress bar** shows completion percentage
- **Current stage** and **task** are displayed
- **Elapsed time** and **ETA** are calculated
- **Records processed** count is updated
- User can cancel with **ESC** or **Ctrl+C**
- Cancellation asks for **confirmation**

---

## Logging System

### Types of Logs

| Level | Color | When Used |
|-------|-------|-----------|
| INFORMATION | Blue | Normal operations (user logged in, navigation) |
| SUCCESS | Green | Successful operations (login, password change) |
| WARNING | Yellow | Suspicious or unusual events (failed login) |
| ERROR | Red | Failures and exceptions |
| DEBUG | Dim | Detailed information for debugging |

### How Logging Works

1. **Logs in Memory**: All logs are stored in a `List<LogEntry>` for quick access
2. **Logs to File**: Logs are also written to files in the `Logs/` folder
3. **File Pattern**: Each day gets its own file like `app_20260710.log`
4. **Automatic Cleanup**: Only the last 30 log files are kept

### Log Viewer

The **Log Viewer** screen lets you:
- Browse all logs
- **Filter by level** (Info, Warning, Error, Success)
- **Filter by date** (enter a specific date)
- **Search by keyword**
- **Export logs** to a JSON file
- **Clear filters**

---

## Configuration System

### appsettings.json

This file contains all settings that can be changed without modifying the code:

```json
{
  "ApplicationSettings": {
    "ApplicationName": "Mini Database Project",
    "Version": "1.0.0",
    "Author": "Your Name"
  },
  "SqlServer": {
    "LocalInstance": "(localdb)\\MSSQLLocalDB",
    "ConnectionTimeoutSeconds": 30
  },
  "Logging": {
    "LogDirectory": "Logs",
    "MaxLogFiles": 30
  },
  "Authentication": {
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 15
  }
}
```

### How to Change Settings

1. Open `appsettings.json`
2. Find the setting you want to change
3. Modify the value
4. Save the file
5. Restart the application

Example: To change the console width, modify `ConsoleWidth` value.

The `AppConfiguration` class reads this file using `ConfigurationBuilder` and creates typed objects that are easy to use in code.

---

## Exception Handling

### Custom Exceptions

| Exception | When Thrown |
|-----------|-------------|
| `AppException` | Base class for all application exceptions |
| `AuthenticationException` | Login failures, invalid credentials |
| `ValidationException` | Invalid user input |
| `DatabaseException` | Database-related errors |
| `ConfigurationException` | Configuration file issues |

### Where Errors Are Caught

1. **At the View level**: Each view catches errors from services
2. **At the Application level**: `Application.RunAsync()` catches unexpected errors
3. **At the Program level**: `Program.Main()` catches startup errors

### Error Display

Errors are shown in **red** with a clear message explaining what went wrong and how to fix it.

---

## Validation System

### What Gets Validated

| Field | Validation Rule |
|-------|-----------------|
| Username | 3-50 characters, letters/numbers/underscores only |
| Email | Must be a valid email format (e.g., user@example.com) |
| Password | Minimum 8 characters, must include uppercase, lowercase, digit, special character |
| Database Name | Start with letter or underscore, max 128 characters |
| Server Name | Cannot be empty, max 256 characters |
| SQL Script Path | Must exist and end with `.sql` |
| Destination Instance | Must be a local instance (LocalDB, localhost, etc.) |

### Local vs Remote Destination

This is a **critical business rule**: The destination must always be a local SQL Server instance. The `ValidationHelper.IsLocalSqlInstance()` method enforces this:

```csharp
public static bool IsLocalSqlInstance(string instance)
{
    // Checks if instance name contains:
    // - (localdb) - LocalDB
    // - localhost - Local machine
    // - . or 127.* or (local) - Local machine
    // - Simple server\instance (no dots = local network)
    if (lower.Contains("(localdb)") || lower.Contains("localhost") || ...)
        return true;
    return false;
}
```

If a user tries to use a remote server (like `sqlserver.company.com`), it is **rejected** both by validation and by the UI warning.

---

## Console UI System

### Colors

The application uses a consistent color scheme:

| Element | Color |
|---------|-------|
| Headers and Titles | Magenta/Purple |
| Success Messages | Green |
| Error Messages | Red |
| Warning Messages | Yellow |
| Information | Blue |
| Normal Text | White |
| Dim/Secondary Text | Gray |
| Highlights | Bright Yellow |

### Icons

Special characters are used as icons to make the interface more visual:

- `[+]` = Success
- `[!]` = Error
- `[*]` = Warning
- `[i]` = Information
- `[?]` = Question/Prompt
- `->` = Arrow

### Panels and Borders

Information is displayed in bordered panels using Unicode box-drawing characters:

```
╔══════════════════════╗
║ Title                ║
╠══════════════════════╣
║ Content here...      ║
╚══════════════════════╝
```

### Progress Bar

Real-time progress is shown with a filled bar:

```
████████████░░░░░░░░░░  50%
```

### Menu System

The `MenuSystem` class creates interactive menus that:
- Can be navigated with **arrow keys** (up/down)
- Are selected with **Enter**
- Can be exited with **Escape**
- Show the currently selected item highlighted
- Support number keys for quick selection

---

## Coding Standards

### SOLID Principles Used

| Principle | How It's Applied |
|-----------|-----------------|
| **S**ingle Responsibility | Each class has one job (e.g., `LoginView` only handles login) |
| **O**pen/Closed | Classes are open for extension, closed for modification (interfaces) |
| **L**iskov Substitution | Derived classes can replace base classes (interface implementations) |
| **I**nterface Segregation | Small, focused interfaces (`IAuthenticationService`, `ILoggerService`) |
| **D**ependency Inversion | High-level modules depend on abstractions (interfaces), not concrete classes |

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `LoginView`, `AuthenticationService` |
| Methods | PascalCase | `ShowAsync()`, `LoginAsync()` |
| Variables | camelCase | `username`, `currentUser` |
| Interfaces | I + PascalCase | `ILoggerService`, `IAuthenticationService` |
| Constants | PascalCase | `AppName`, `Success` |
| Private fields | _camelCase | `_logger`, `_authService` |

### Async Programming

- All I/O operations use `async/await`
- Long-running tasks are asynchronous to keep the UI responsive
- Method names end with `Async` suffix

### Dependency Injection

Services are registered in `Program.cs`:
```csharp
services.AddSingleton<ILoggerService, LoggerService>();
services.AddTransient<LoginView>();
```

- **Singleton**: One instance for the whole application (e.g., Logger)
- **Transient**: New instance every time it's requested (e.g., Views)

---

## How to Set Up and Run

### Prerequisites

1. **Windows 10 or later** (the application uses Windows-specific console features)
2. **.NET 10.0 SDK** or later
   - Download from: https://dotnet.microsoft.com/download
   - Verify with: `dotnet --version`

### Setup Steps

1. **Open a terminal** (Command Prompt, PowerShell, or Terminal)

2. **Navigate to the project folder**:
   ```bash
   cd C:\Users\YourName\Desktop\Mini_db_project\MiniDbProject
   ```

3. **Restore NuGet packages** (download dependencies):
   ```bash
   dotnet restore
   ```

4. **Build the project** (compile the code):
   ```bash
   dotnet build
   ```

5. **Run the application**:
   ```bash
   dotnet run
   ```

### Demo Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Administrator |
| user | User@123 | Standard User |
| demo | Demo@123 | Demo User |

---

## Common Errors and Solutions

### Build Errors

| Error | Solution |
|-------|----------|
| `error CS0246: The type or namespace name '...' could not be found` | Run `dotnet restore` to download packages |
| `error MSB3030: Could not copy the file` | Close any programs using the file and rebuild |
| `NU1101: Unable to find package` | Check internet connection, run `dotnet restore` |

### Runtime Errors

| Error | Solution |
|-------|----------|
| `An unexpected error occurred` | Check the logs in the `Logs/` folder |
| `Login failed` | Make sure you're using the correct credentials |
| `Database creation failed` | Ensure SQL Server LocalDB is installed |
| `Configuration file not found` | Make sure `appsettings.json` is in the project folder |

### Console Display Issues

| Problem | Solution |
|---------|----------|
| Unicode characters show as boxes | Run in Windows Terminal or enable UTF-8 |
| Colors not showing | The console must support ANSI escape codes (Windows 10+ Terminal) |
| Layout looks wrong | Try different console window size |

---

## Future Improvements

### Easy to Add Next

1. **Real Database Connection**: Replace `DatabaseService.cs` with actual SQL Server connection using `Microsoft.Data.SqlClient`
2. **MFA with Real Codes**: Integrate a library like `Otp.NET` for real TOTP codes
3. **Pause/Resume**: The progress tracker already has the structure, just need to implement it
4. **Multiple SQL Providers**: Add support for PostgreSQL, MySQL by creating new implementations of `IDatabaseService`
5. **Themes**: Allow users to switch between different color themes
6. **Internationalization**: Move all strings in Messages.cs to resource files for multiple languages

### Architecture is Ready For

- **Real authentication** against a database
- **Multiple database providers** (just implement the interfaces)
- **Unit testing** (all services use interfaces that can be mocked)
- **Web API** (the services layer can be reused in an ASP.NET Core project)

---

## Every Class Explained

### Authentication Folder

**IAuthenticationService** - Interface that defines what authentication can do:
- Login, Logout, ChangePassword, ValidateMfaCode, GetCurrentUser

**AuthenticationService** - The actual implementation:
- Checks against demo accounts
- Handles MFA flow
- Logs all authentication events

### Configuration Folder

**AppConfiguration** - Reads appsettings.json and creates objects:
- ApplicationSettings, SqlServer, Logging, Authentication, DatabaseCreation, Security

### Constants Folder

**AppConstants** - Fixed values used everywhere:
- Colors (ANSI codes), Icons (Unicode characters), MenuKeys, LogLevels

### Database Folder

**IDatabaseService** - Interface for database operations:
- Get portals, get databases, validate, estimate size, create database

**DatabaseService** - Demo implementation:
- Returns mock data for portals and databases
- Simulates database creation with delays

**IProgressTracker** - Interface for tracking progress:
- Events that fire when progress changes

### DTOs Folder

**LoginRequest** - Data needed to log in (username, password, optional MFA code)

**LoginResponse** - Data returned after login (success/failure, user object, messages)

**ChangePasswordRequest** - Data for changing password (current, new, confirm)

**ProfileUpdateRequest** - Data for updating profile (name, email, phone, department)

**DatabaseSelectionDto** - Data for database selection (portal, server, databases)

### Exceptions Folder

**AppException** - Base exception with error code and user-friendly flag

**AuthenticationException** - For login errors

**ValidationException** - For input validation errors

**DatabaseException** - For database errors

**ConfigurationException** - For configuration errors

### Helpers Folder

**ConsoleHelper** - All console output methods:
- WriteColored, WriteSuccess, WriteError, DisplayPanel, DisplayBanner, etc.

**InputHelper** - All user input methods:
- ReadString, ReadInt, ReadPassword, ReadBool

**PasswordHelper** - Password utilities:
- GenerateSalt, HashPassword, VerifyPassword, IsStrongPassword

**ValidationHelper** - Input validation:
- IsValidUsername, IsValidEmail, IsValidDatabaseName, IsLocalSqlInstance

### Logging Folder

**ILoggerService** - Interface for logging:
- LogInfo, LogSuccess, LogWarning, LogError, GetLogs, ExportLogs, etc.

**LoggerService** - Implementation:
- Stores logs in memory (list) and writes to files
- Automatically cleans up old log files

### Models Folder

**User** - Represents a user account:
- Username, Email, FullName, PasswordHash, MFA settings, Login dates

**LogEntry** - Represents a single log entry:
- Timestamp, Level, Category, Message, Exception details

**MenuItem** - Represents a menu item:
- Id, Label, Description, Shortcut key

**ServerPortal** - Represents a server portal:
- PortalName, ServerName, Databases list

**DatabaseInfo** - Represents a database:
- DatabaseName, Size, TableCount, Script file path

**ProgressInfo** - Tracks progress of database creation:
- Steps, Percentage, Current task, Elapsed time, ETA

**WizardStep** - Represents a wizard step:
- StepNumber, Title, IsCompleted, Status

### Resources Folder

**Messages** - All user-facing text messages organized by category:
- Login, Dashboard, Profile, DatabaseWizard, LogViewer, Help, Errors, Validation, Common

### Services Folder

**IProfileService** - Interface for profile management:
- GetProfile, UpdateProfile, ChangePassword, ToggleMfa

**ProfileService** - Implementation:
- Handles profile updates, password changes, MFA toggle

### Utilities Folder

**ConsoleSpinner** - Animated loading spinner:
- Shows a spinning block animation while waiting
- Can be started and stopped

**SecureStringHelper** - Secure string handling:
- Creates SecureString, converts between plain and secure strings

**TimeHelper** - Time formatting:
- FormatTimeSpan, FormatDateTime, GetTimeAgo

### Views Folder

**IView** - Interface for all views:
- Name property, ShowAsync method

**ConsoleRenderer** - Drawing functions:
- DrawHeader, DrawFooter, DrawMenuTitle, DrawInfoPanel, DrawBox

**MenuSystem** - Interactive menu:
- Add items, navigate with arrows, select with Enter

**LoginView** - The login screen:
- Shows banner, asks for username/password, handles MFA

**DashboardView** - Main menu after login:
- Shows welcome message, menu options, navigates to other views

**ProfileView** - Profile management:
- Shows user info, update profile, change password, toggle MFA

**DatabaseWizardView** - Database creation wizard:
- 4-step wizard (source, destination, validation, summary)

**ProgressView** - Real-time progress:
- Shows progress bar, current task, elapsed time, ETA, cancel option

**LogViewerView** - Log browser:
- Shows logs, filter by level/date, search, export

**HelpView** - Help documentation:
- Features, shortcuts, troubleshooting

**AboutView** - Application information:
- Version, author, technology stack

---

## Conclusion

This project is designed to be a **learning tool** as much as a functional application. Each part is separated so you can understand how it fits together. The use of interfaces, dependency injection, and separation of concerns means you can change one part without affecting others.

**Happy coding!**
