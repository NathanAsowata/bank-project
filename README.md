

# **README: BankApp (Academic Project) - A WPF Desktop Banking Application**

This document provides a technical overview of the BankApp project, a multi-user desktop banking application built with C# and WPF.

### **Core Features Implemented**

*   **Role-Based Access Control (RBAC):** Functionality is strictly segregated between "Teller," "Manager," and "Admin" roles.
*   **Secure User Authentication:** Staff credentials are securely stored and verified.
*   **Full Account Management:** Create and edit customer bank accounts (Current & Savings).
*   **Complete Transaction Lifecycle:** Full support for Deposits, Withdrawals, and Internal/External Transfers.
*   **Managerial Oversight:** A transaction approval workflow for large sums handled by Tellers.
*   **Administrative Functions:** Management of staff user accounts, including creation and suspension.

---

### **Technical Implementation**

#### **1. Architecture: 3-Tier Separation of Concerns**

The application is built on a classic 3-Tier Architecture (`BankApp` UI, `BIZ` Logic, `DAL` Data) to enforce separation of concerns and maintainability.

*   **Direct Impact:** This structure ensures that UI changes in the **BankApp** project (e.g., modifying a XAML page) have no impact on the core business rules.
*   **Direct Impact:** All business logic (e.g., permission checks, transaction validation) is centralized in the **BIZ** layer, making it independent of the UI and the database. Swapping the SQL Server database for another would only require changes in the **DAL**, with no modifications to the business logic.

#### **2. Security: Robust Password Hashing and Role Enforcement**

Security was a primary concern, addressed through secure password storage and strict, server-side permission checks.

*   **Direct Impact:** Implemented robust password security using the `PasswordHelper` class in the BIZ layer, which leverages the .NET `Rfc2898DeriveBytes` (PBKDF2) algorithm. For each new staff user, a unique, cryptographically secure 16-byte salt is generated and stored alongside the hashed password. This prevents rainbow table attacks and ensures that identical passwords do not result in the same hash.
*   **Direct Impact:** Enforced Role-Based Access Control (RBAC) at the **BIZ layer**, not the UI. For example, the `StaffService.CreateStaffUser` method explicitly checks if the `creatingAdmin.Role` is "Admin" before proceeding. This secures the business logic at its source, preventing unauthorized actions even if UI controls were somehow bypassed.

#### **3. Presentation Layer (UI): WPF and MVVM Principles**

The user interface was built using WPF, following principles of the Model-View-ViewModel (MVVM) pattern for a responsive and maintainable UI.

*   **Direct Impact:** Ensured a responsive UI by implementing the `INotifyPropertyChanged` interface (via the `ObservableObject` base class in a `Shared` project, though the files show it implemented directly in the UI layer's pages) and using `ObservableCollection` for data grids (e.g., on `DashboardPage.xaml.cs`). This allows the WPF data binding engine to automatically update the account list in real-time when a new account is created or a transaction is approved, without requiring manual UI refresh code.
*   **Direct Impact:** Centralized all navigation logic within `MainWindow.xaml.cs`. This class acts as a controller, managing the `Frame` that hosts the various pages (`LoginPage`, `DashboardPage`, etc.). This approach decouples individual pages from each other, as they only need to communicate back to the `MainWindow`.

#### **4. Business Logic Layer (BIZ): Encapsulating Business Rules**

The BIZ layer translates project requirements into testable C# code, handling all validation and logical operations.

*   **Direct Impact:** Enforced the business rule for transaction approvals by defining a `const decimal ApprovalThreshold` of 10,000. The `Deposit`, `Withdraw`, and `Transfer` methods in the `TransactionService` explicitly check if the `performingStaff.Role` is "Teller" and if the transaction `amount` exceeds this threshold. If so, the transaction's `ApprovalStatus` is set to "Pending" and saved to the database, directly implementing the dual-control requirement.
*   **Direct Impact:** Managed account-specific rules, such as preventing overdrafts on Savings accounts. In the `AccountService.CreateNewAccount` method, the code explicitly checks if `AccountType` is "Savings" and forces the `OverdraftLimit` to 0, ensuring data integrity at the business logic level.

#### **5. Data Access Layer (DAL): Isolating Database Interaction**

All database communication is handled exclusively by the DAL using ADO.NET and Stored Procedures.

*   **Direct Impact:** The `DAO.cs` class centralizes all `SqlConnection` management. By using `using` blocks for `SqlConnection` and `SqlCommand` objects, it guarantees that database connections are properly opened and closed, preventing resource leaks.
*   **Direct Impact:** Each DAL class (e.g., `AccountDAL`, `TransactionDAL`) is responsible for mapping C# model properties to `SqlParameter` objects for stored procedure calls (e.g., `sp_CreateAccount`). This isolates all SQL-specific code, making the application easier to debug and maintain. The mapping from a `SqlDataReader` back to a C# model is handled by private `MapReaderTo...` methods, ensuring consistency.

---

### **Database Schema **

The application relies on three primary tables:

*   **Accounts:** Stores customer and bank account details (Name, Address, Type, Balance, Overdraft).
*   **Staff:** Stores employee login details (Username, Hashed Password, Salt, Role, Status).
*   **TransactionLog:** Records every financial transaction (Deposit, Withdrawal, Transfer) with details on source/destination, amount, status (Pending/Approved), and the staff member who initiated it.

---

### **How to Run**

1.  **Database Setup:**
    *   Execute the SQL script on a SQL Server instance to create the `BankAppDB` database, tables, and stored procedures.
    *   Update the `DBCon` connection string in `BankApp/App.config` to point to your SQL Server instance.

2.  **Open in Visual Studio:**
    *   Open the `BankApp.sln` solution file in Visual Studio.
    *   Ensure the `BankApp` project is set as the startup project.

3.  **Run the Application:**
    *   Press F5 or click the "Start" button to build and run the application.
    *   Default login credentials can be found in the database setup script (or use the "Admin" features to create new users).
