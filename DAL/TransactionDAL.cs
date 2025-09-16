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
    public class TransactionDAL
    {
        private DAO dao = new DAO();

        public int CreateTransaction(TransactionLog transaction)
        {
            int newTransactionID = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_CreateTransaction", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SourceAccountID", (object)transaction.SourceAccountID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DestinationAccountID", (object)transaction.DestinationAccountID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                    cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    cmd.Parameters.AddWithValue("@Description", (object)transaction.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequiresApproval", transaction.RequiresApproval);
                    cmd.Parameters.AddWithValue("@ApprovalStatus", transaction.ApprovalStatus);
                    cmd.Parameters.AddWithValue("@ReferenceNumber", (object)transaction.ReferenceNumber ?? DBNull.Value);


                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        newTransactionID = Convert.ToInt32(result);
                    }
                }
            }
            dao.CloseCon();
            return newTransactionID;
        }

        public List<TransactionLog> GetTransactionsByAccountID(int accountId)
        {
            List<TransactionLog> transactions = new List<TransactionLog>();
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetTransactionsByAccountID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AccountID", accountId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(MapReaderToTransaction(reader));
                        }
                    }
                }
            }
            dao.CloseCon();
            return transactions;
        }

        public List<TransactionLog> GetPendingTransactions()
        {
            List<TransactionLog> transactions = new List<TransactionLog>();
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPendingTransactions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new TransactionLog
                            {
                                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                                SourceAccountID = reader["SourceAccountID"] != DBNull.Value ? Convert.ToInt32(reader["SourceAccountID"]) : (int?)null,
                                SourceAccountName = reader["SourceAccountName"]?.ToString(), // Added field
                                DestinationAccountID = reader["DestinationAccountID"] != DBNull.Value ? Convert.ToInt32(reader["DestinationAccountID"]) : (int?)null,
                                DestinationAccountName = reader["DestinationAccountName"]?.ToString(), // Added field
                                TransactionType = reader["TransactionType"].ToString(),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                                Description = reader["Description"]?.ToString(),
                                ReferenceNumber = reader["ReferenceNumber"]?.ToString(),
                                RequiresApproval = true,
                                ApprovalStatus = "Pending"
                            });
                        }
                    }
                }
            }
            dao.CloseCon();
            return transactions;
        }

        public bool UpdateTransactionApprovalStatus(int transactionId, string status, int managerId)
        {
            int result = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateTransactionApprovalStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                    cmd.Parameters.AddWithValue("@ApprovalStatus", status);
                    cmd.Parameters.AddWithValue("@ApprovingManagerID", managerId);

                    object scalarResult = cmd.ExecuteScalar();
                    if (scalarResult != null && scalarResult != DBNull.Value)
                    {
                        result = Convert.ToInt32(scalarResult);
                    }
                }
            }
            dao.CloseCon();
            return result == 1;
        }

        public TransactionLog GetTransactionByID(int transactionId)
        {
            TransactionLog transaction = null;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetTransactionByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TransactionID", transactionId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            transaction = new TransactionLog
                            {
                                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                                SourceAccountID = reader["SourceAccountID"] != DBNull.Value ? Convert.ToInt32(reader["SourceAccountID"]) : (int?)null,
                                DestinationAccountID = reader["DestinationAccountID"] != DBNull.Value ? Convert.ToInt32(reader["DestinationAccountID"]) : (int?)null,
                                TransactionType = reader["TransactionType"].ToString(),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                ApprovalStatus = reader["ApprovalStatus"].ToString()
                               
                            };
                        }
                    }
                }
            }
            dao.CloseCon();
            return transaction;
        }


        private TransactionLog MapReaderToTransaction(SqlDataReader reader)
        {
            return new TransactionLog
            {
                TransactionID = Convert.ToInt32(reader["TransactionID"]),
                SourceAccountID = reader["SourceAccountID"] != DBNull.Value ? Convert.ToInt32(reader["SourceAccountID"]) : (int?)null,
                DestinationAccountID = reader["DestinationAccountID"] != DBNull.Value ? Convert.ToInt32(reader["DestinationAccountID"]) : (int?)null,
                TransactionType = reader["TransactionType"].ToString(),
                Amount = Convert.ToDecimal(reader["Amount"]),
                TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                Description = reader["Description"]?.ToString(),
                RequiresApproval = Convert.ToBoolean(reader["RequiresApproval"]),
                ApprovalStatus = reader["ApprovalStatus"].ToString(),
                ReferenceNumber = reader["ReferenceNumber"]?.ToString()
            };
        }
    }
}

