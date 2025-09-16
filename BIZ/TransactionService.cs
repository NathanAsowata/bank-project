using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;

namespace BIZ
{
    public class TransactionService
    {
        private TransactionDAL _transactionDAL = new TransactionDAL();
        private AccountDAL _accountDAL = new AccountDAL();
        private const decimal ApprovalThreshold = 10000;

        public bool Deposit(int accountId, decimal amount, string description, StaffUser performingStaff)
        {
            if (performingStaff == null || (performingStaff.Role != "Teller" && performingStaff.Role != "Manager" && performingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions for deposit.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be positive.");
            }

            Account account = _accountDAL.GetAccountById(accountId); // Using _accountDAL instance
            if (account == null)
            {
                throw new KeyNotFoundException($"Account {accountId} not found.");
            }

            bool requiresApproval = (performingStaff.Role == "Teller" && amount >= ApprovalThreshold);
            string status = requiresApproval ? "Pending" : "Approved";

            var transaction = new TransactionLog
            {
                DestinationAccountID = accountId,
                SourceAccountID = null,
                TransactionType = "Deposit",
                Amount = amount,
                Description = description,
                RequiresApproval = requiresApproval,
                ApprovalStatus = status,
                ReferenceNumber = Guid.NewGuid().ToString("N").Substring(0, 16),
                TransactionDate = DateTime.Now
            };

            try
            {
                int transactionId = _transactionDAL.CreateTransaction(transaction); 

                if (!requiresApproval)
                {
                    bool balanceUpdated = _accountDAL.UpdateAccountBalance(accountId, amount);
                    if (!balanceUpdated)
                    {
                        System.Diagnostics.Debug.WriteLine($"CRITICAL: Deposit transaction {transactionId} logged but failed to update balance for account {accountId}. Manual correction needed.");
                        throw new ApplicationException("Deposit logged, but failed to update account balance. Please contact support.");
                    }
                }
                return true;
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error during deposit: {dbEx.Message}");
                throw new ApplicationException("Database error during deposit.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error during deposit: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred during deposit.", ex);
            }
        }

        public bool Withdraw(int accountId, decimal amount, string description, StaffUser performingStaff)
        {
            if (performingStaff == null || (performingStaff.Role != "Teller" && performingStaff.Role != "Manager" && performingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions for withdrawal.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be positive.");
            }

            Account account = _accountDAL.GetAccountById(accountId); // Use _accountDAL instance
            if (account == null)
            {
                throw new KeyNotFoundException($"Account {accountId} not found.");
            }

            bool requiresApproval = (performingStaff.Role == "Teller" && amount >= ApprovalThreshold);
            string status = requiresApproval ? "Pending" : "Approved";

            if (!requiresApproval)
            {
                if (account.AvailableBalance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds for withdrawal.");
                }
            }

            var transaction = new TransactionLog
            {
                SourceAccountID = accountId,
                DestinationAccountID = null,
                TransactionType = "Withdrawal",
                Amount = amount,
                Description = description,
                RequiresApproval = requiresApproval,
                ApprovalStatus = status,
                ReferenceNumber = Guid.NewGuid().ToString("N").Substring(0, 16), // Unique Reference Number
                TransactionDate = DateTime.Now
            };

            try
            {
                int transactionId = _transactionDAL.CreateTransaction(transaction); // Using _transactionDAL instance

                if (!requiresApproval)
                {
                    bool balanceUpdated = _accountDAL.UpdateAccountBalance(accountId, -amount);
                    if (!balanceUpdated)
                    {
                        System.Diagnostics.Debug.WriteLine($"CRITICAL: Withdrawal transaction {transactionId} logged but failed to update balance for account {accountId}. Manual correction needed.");
                        throw new ApplicationException("Withdrawal logged, but failed to update account balance. Please contact support.");
                    }
                }
                return true;
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error during withdrawal: {dbEx.Message}");
                throw new ApplicationException("Database error during withdrawal.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error during withdrawal: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred during withdrawal.", ex);
            }
        }

        public bool Transfer(int sourceAccountId, int? destinationAccountId, int? destinationSortCode, decimal amount, string description, StaffUser performingStaff)
        {
            if (performingStaff == null || (performingStaff.Role != "Teller" && performingStaff.Role != "Manager" && performingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions for transfer.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Transfer amount must be positive.");
            }
            if (!destinationAccountId.HasValue && !destinationSortCode.HasValue)
            {
                throw new ArgumentException("Destination account or sort code must be provided for transfer.");
            }
            if (sourceAccountId == destinationAccountId)
            {
                throw new ArgumentException("Cannot transfer funds to the same account.");
            }

            Account sourceAccount = _accountDAL.GetAccountById(sourceAccountId); // Using _accountDAL instance
            if (sourceAccount == null)
            {
                throw new KeyNotFoundException($"Source account {sourceAccountId} not found.");
            }

            bool isInternalTransfer = destinationAccountId.HasValue;
            Account destinationAccount = null;

            if (isInternalTransfer)
            {
                destinationAccount = _accountDAL.GetAccountById(destinationAccountId.Value); // Usin _accountDAL instance
                if (destinationAccount == null)
                {
                    throw new KeyNotFoundException($"Destination account {destinationAccountId.Value} not found.");
                }
                destinationSortCode = destinationAccount.SortCode;
            }
            else
            {
                if (sourceAccount.AccountType == "Savings")
                {
                    throw new InvalidOperationException("Savings accounts can only transfer funds internally.");
                }
                if (!destinationSortCode.HasValue)
                {
                    throw new ArgumentException("Destination sort code is required for external transfers.");
                }
            }

            bool requiresApproval = (performingStaff.Role == "Teller" && amount >= ApprovalThreshold);
            string status = requiresApproval ? "Pending" : "Approved";
            string transferRef = Guid.NewGuid().ToString("N").Substring(0, 16); // Reference already generated here

            if (!requiresApproval)
            {
                if (sourceAccount.AvailableBalance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds for transfer.");
                }
            }

            var transaction = new TransactionLog
            {
                SourceAccountID = sourceAccountId,
                DestinationAccountID = destinationAccountId,
                TransactionType = "Transfer",
                Amount = amount,
                Description = description + (isInternalTransfer ? $" to Acc {destinationAccountId}" : $" to Sort Code {destinationSortCode}"),
                RequiresApproval = requiresApproval,
                ApprovalStatus = status,
                ReferenceNumber = transferRef,
                TransactionDate = DateTime.Now
            };

            try
            {
                int transactionId = _transactionDAL.CreateTransaction(transaction); // Use _transactionDAL instance

                if (!requiresApproval)
                {
                    bool sourceDebited = _accountDAL.UpdateAccountBalance(sourceAccountId, -amount); // Use _accountDAL instance
                    bool destinationCredited = true;

                    if (isInternalTransfer)
                    {
                        destinationCredited = _accountDAL.UpdateAccountBalance(destinationAccountId.Value, amount); // Use _accountDAL instance
                    }

                    if (!sourceDebited || !destinationCredited)
                    {
                        System.Diagnostics.Debug.WriteLine($"CRITICAL: Transfer transaction {transactionId} logged but failed balance update (SourceOK:{sourceDebited}, DestOK:{destinationCredited}). Manual correction needed.");
                        throw new ApplicationException("Transfer logged, but failed to update account balances correctly. Please contact support.");
                    }
                }
                return true;
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error during transfer: {dbEx.Message}");
                throw new ApplicationException("Database error during transfer.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error during transfer: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred during transfer.", ex);
            }
        }

        public List<TransactionLog> GetTransactionsForAccount(int accountId, StaffUser requestingStaff)
        {
            if (requestingStaff == null || (requestingStaff.Role != "Teller" && requestingStaff.Role != "Manager" && requestingStaff.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to view transactions.");
            }
            try
            {
                return _transactionDAL.GetTransactionsByAccountID(accountId); // Use _transactionDAL instance
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error getting transactions: {dbEx.Message}");
                throw new ApplicationException("Database error retrieving transactions.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error getting transactions: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred retrieving transactions.", ex);
            }
        }

        public List<TransactionLog> GetPendingApprovalTransactions(StaffUser requestingManager)
        {
            if (requestingManager == null || (requestingManager.Role != "Manager" && requestingManager.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Only Managers or Admins can view pending transactions.");
            }
            try
            {
                return _transactionDAL.GetPendingTransactions(); // Use _transactionDAL instance
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error getting pending tx: {dbEx.Message}");
                throw new ApplicationException("Database error retrieving pending transactions.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error getting pending tx: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred retrieving pending transactions.", ex);
            }
        }

        public bool ApproveTransaction(int transactionId, StaffUser approvingManager)
        {
            return ProcessApproval(transactionId, "Approved", approvingManager);
        }

        public bool RejectTransaction(int transactionId, StaffUser rejectingManager)
        {
            return ProcessApproval(transactionId, "Rejected", rejectingManager, false);
        }

        private bool ProcessApproval(int transactionId, string status, StaffUser manager, bool applyBalanceChanges = true)
        {
            if (manager == null || (manager.Role != "Manager" && manager.Role != "Admin"))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to approve/reject transactions.");
            }

            try
            {
                TransactionLog transaction = _transactionDAL.GetTransactionByID(transactionId); // Use _transactionDAL instance
                if (transaction == null || transaction.ApprovalStatus != "Pending")
                {
                    throw new InvalidOperationException("Transaction not found or not pending approval.");
                }

                bool statusUpdated = _transactionDAL.UpdateTransactionApprovalStatus(transactionId, status, manager.StaffID); // Use _transactionDAL instance

                if (!statusUpdated)
                {
                    throw new InvalidOperationException("Failed to update transaction status. It might have been processed already.");
                }

                if (status == "Approved" && applyBalanceChanges)
                {
                    bool sourceUpdated = true;
                    bool destUpdated = true;

                    if (transaction.SourceAccountID.HasValue)
                    {
                        Account sourceAccount = _accountDAL.GetAccountById(transaction.SourceAccountID.Value); // Use _accountDAL instance
                        if (sourceAccount == null || sourceAccount.AvailableBalance < transaction.Amount)
                        {
                            System.Diagnostics.Debug.WriteLine($"APPROVAL FAILED (Funds): Transaction {transactionId} approved, but source account {transaction.SourceAccountID} has insufficient funds ({sourceAccount?.AvailableBalance}) for amount {transaction.Amount}. Manual correction needed.");
                            // Decide how to handle this - maybe auto-reject? Or just throw?
                            // For now, let the UpdateAccountBalance potentially fail or rely on DB constraints
                            // throw new InvalidOperationException($"Insufficient funds in source account {transaction.SourceAccountID} at time of approval.");
                        }

                        sourceUpdated = _accountDAL.UpdateAccountBalance(transaction.SourceAccountID.Value, -transaction.Amount); // Use _accountDAL instance
                    }

                    if (transaction.DestinationAccountID.HasValue)
                    {
                        destUpdated = _accountDAL.UpdateAccountBalance(transaction.DestinationAccountID.Value, transaction.Amount); // Use _accountDAL instance
                    }

                    if (!sourceUpdated || !destUpdated)
                    {
                        System.Diagnostics.Debug.WriteLine($"CRITICAL: Approved transaction {transactionId} failed balance update (SourceOK:{sourceUpdated}, DestOK:{destUpdated}). Manual correction needed.");
                        throw new ApplicationException("Transaction approved, but failed to update account balances correctly. Please contact support immediately.");
                    }
                }
                return true;
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error processing approval: {dbEx.Message}");
                throw new ApplicationException("Database error processing transaction approval.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error processing approval: {ex.Message}");
                if (ex is KeyNotFoundException || ex is InvalidOperationException || ex is UnauthorizedAccessException) throw;
                throw new ApplicationException("An unexpected error occurred processing transaction approval.", ex);
            }
        }
    }
}