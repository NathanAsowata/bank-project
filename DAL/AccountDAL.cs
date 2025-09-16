using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class AccountDAL
    {
        private DAO dao = new DAO();

        public int CreateAccount(Account account)
        {
            int newAccountID = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_CreateAccount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FirstName", account.FirstName);
                    cmd.Parameters.AddWithValue("@Surname", account.Surname);
                    cmd.Parameters.AddWithValue("@Email", (object)account.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object)account.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine1", (object)account.AddressLine1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine2", (object)account.AddressLine2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", (object)account.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@County", account.County);
                    cmd.Parameters.AddWithValue("@AccountType", account.AccountType);
                    cmd.Parameters.AddWithValue("@SortCode", account.SortCode);
                    cmd.Parameters.AddWithValue("@Balance", account.Balance);
                    cmd.Parameters.AddWithValue("@OverdraftLimit", account.OverdraftLimit);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        newAccountID = Convert.ToInt32(result);
                    }
                }
            }
            dao.CloseCon();
            return newAccountID;
        }

        public bool UpdateAccountDetails(Account account)
        {
            int rowsAffected = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateAccountDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    cmd.Parameters.AddWithValue("@Email", (object)account.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object)account.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine1", (object)account.AddressLine1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine2", (object)account.AddressLine2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", (object)account.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@County", account.County);
                    cmd.Parameters.AddWithValue("@OverdraftLimit", account.OverdraftLimit);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            dao.CloseCon();
            return rowsAffected > 0;
        }

        public Account GetAccountById(int accountId)
        {
            Account account = null;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAccountByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountID", accountId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            account = MapReaderToAccount(reader);
                        }
                    }
                }
            }
            dao.CloseCon();
            return account;
        }

        public List<Account> GetAllAccounts()
        {
            List<Account> accounts = new List<Account>();
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(MapReaderToAccount(reader));
                        }
                    }
                }
            }
            dao.CloseCon();
            return accounts;
        }

        public bool UpdateAccountBalance(int accountId, decimal amountChange)
        {
            int rowsAffected = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateAccountBalance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountID", accountId);
                    cmd.Parameters.AddWithValue("@AmountChange", amountChange);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            dao.CloseCon();

            return true; 
        }


        private Account MapReaderToAccount(SqlDataReader reader)
        {
            return new Account
            {
                AccountID = Convert.ToInt32(reader["AccountID"]),
                FirstName = reader["FirstName"].ToString(),
                Surname = reader["Surname"].ToString(),
                Email = reader["Email"]?.ToString(),
                Phone = reader["Phone"]?.ToString(),
                AddressLine1 = reader["AddressLine1"]?.ToString(),
                AddressLine2 = reader["AddressLine2"]?.ToString(),
                City = reader["City"]?.ToString(),
                County = reader["County"].ToString(),
                AccountType = reader["AccountType"].ToString(),
                SortCode = Convert.ToInt32(reader["SortCode"]),
                Balance = Convert.ToDecimal(reader["Balance"]),
                OverdraftLimit = Convert.ToDecimal(reader["OverdraftLimit"]),
                DateCreated = Convert.ToDateTime(reader["DateCreated"])
            };
        }
    }
}
