using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DAL.Models;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;


namespace BIZ
{
    public class AccountService
    {
        private AccountDAL accountDAL = new AccountDAL();
        private int defaultSortCode;

        public AccountService()
        {
            // Read default sort code from config
            if (!int.TryParse(ConfigurationManager.AppSettings["DefaultSortCode"], out defaultSortCode))
            {
                defaultSortCode = 101010; // Fallback default
                System.Diagnostics.Debug.WriteLine("Warning: DefaultSortCode not found or invalid in App.config. Using fallback 101010.");
            }
        }

        public int GetDefaultSortCode()
        {
            return defaultSortCode;
        }


        public int CreateNewAccount(Account account, StaffUser creatingStaff)
        {
            if (creatingStaff == null || (creatingStaff.Role != "Teller" && creatingStaff.Role != "Manager" && creatingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to create an account.");
            }

            // Validation
            if (string.IsNullOrWhiteSpace(account.FirstName) || string.IsNullOrWhiteSpace(account.Surname) || string.IsNullOrWhiteSpace(account.County) || string.IsNullOrWhiteSpace(account.AccountType))
            {
                throw new ArgumentException("First Name, Surname, County, and Account Type are required.");
            }
            if (account.AccountType != "Current" && account.AccountType != "Savings")
            {
                throw new ArgumentException("Invalid Account Type.");
            }
            if (account.Balance < 0)
            {
                throw new ArgumentException("Initial balance cannot be negative.");
            }
            if (account.AccountType == "Savings" && account.OverdraftLimit != 0)
            {
                // Enforce Overdraft requiremnt for Savings accounts
                account.OverdraftLimit = 0;
            }
            if (account.AccountType == "Current" && account.OverdraftLimit < 0)
            {
                throw new ArgumentException("Overdraft limit cannot be negative.");
            }

            // Use the fallback sort code if the default sort code is not found
            if (account.SortCode == 0) account.SortCode = defaultSortCode;


            try
            {
                // The AccountID returned by DAL is also the Account Number
                return accountDAL.CreateAccount(account);
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error creating account: {dbEx.Message}");
                throw new ApplicationException("Database error occurred while creating the account.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error creating account: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred while creating the account.", ex);
            }
        }

        public bool UpdateAccountDetails(Account account, StaffUser updatingStaff)
        {
            if (updatingStaff == null || (updatingStaff.Role != "Teller" && updatingStaff.Role != "Manager" && updatingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to update account details.");
            }

            // Ensure required fields for update are present
            if (account.AccountID <= 0 || string.IsNullOrWhiteSpace(account.County))
            {
                throw new ArgumentException("Account ID and County are required for update.");
            }

            // Fetch original account to enforce the project requirements
            Account originalAccount = accountDAL.GetAccountById(account.AccountID);
            if (originalAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {account.AccountID} not found.");
            }

            // Ensure overdraft is 0 if somehow trying to update a Savings account's overdraft
            if (originalAccount.AccountType == "Savings" && account.OverdraftLimit != 0)
            {
                account.OverdraftLimit = 0;
            }
            if (account.OverdraftLimit < 0)
            {
                throw new ArgumentException("Overdraft limit cannot be negative.");
            }


            try
            {
                // Only specific fields are updated by the DAL procedure
                return accountDAL.UpdateAccountDetails(account);
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error updating account: {dbEx.Message}");
                throw new ApplicationException("Database error occurred while updating the account.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error updating account: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred while updating the account.", ex);
            }
        }

        public Account GetAccountById(int accountId, StaffUser requestingStaff)
        {
            if (requestingStaff == null || (requestingStaff.Role != "Teller" && requestingStaff.Role != "Manager" && requestingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to view account details.");
            }

            try
            {
                return accountDAL.GetAccountById(accountId);
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error getting account: {dbEx.Message}");
                throw new ApplicationException("Database error occurred while retrieving the account.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error getting account: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred while retrieving the account.", ex);
            }
        }

        public List<Account> GetAllAccounts(StaffUser requestingStaff)
        {
            if (requestingStaff == null || (requestingStaff.Role != "Teller" && requestingStaff.Role != "Manager" && requestingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to view all accounts.");
            }
            try
            {
                return accountDAL.GetAllAccounts();
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error getting all accounts: {dbEx.Message}");
                throw new ApplicationException("Database error occurred while retrieving accounts.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error getting all accounts: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred while retrieving accounts.", ex);
            }
        }

        public string SerializeAccountToXml(int accountId, StaffUser requestingStaff)
        {
            if (requestingStaff == null || (requestingStaff.Role != "Teller" && requestingStaff.Role != "Manager" && requestingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to serialize account data.");
            }

            try
            {
                Account account = accountDAL.GetAccountById(accountId);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with ID {accountId} not found.");
                }

                XmlSerializer serializer = new XmlSerializer(typeof(Account));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, account);
                    return writer.ToString();
                }
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error during XML serialization: {dbEx.Message}");
                throw new ApplicationException("Database error occurred while preparing data for serialization.", dbEx);
            }
            catch (KeyNotFoundException knfex)
            {
                throw knfex; // Throw specific exception again
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error during XML serialization: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred during XML serialization.", ex);
            }
        }
    }
}
