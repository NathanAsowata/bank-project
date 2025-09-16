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
    public class UserDAL
    {
        private DAO dao = new DAO();

        public StaffUser GetUserByUsername(string username)
        {
            StaffUser user = null;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetStaffUserByUsername", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new StaffUser
                            {
                                StaffID = Convert.ToInt32(reader["StaffID"]),
                                Username = reader["Username"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Salt = reader["Salt"].ToString(),
                                Role = reader["Role"].ToString(),
                                IsSuspended = Convert.ToBoolean(reader["IsSuspended"]),
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString()
                            };
                        }
                    }
                }
            }
            dao.CloseCon();
            return user;
        }

        public int CreateStaffUser(string username, string passwordHash, string salt, string role, string firstName, string lastName)
        {
            int newStaffID = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_CreateStaffUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Salt", salt);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        newStaffID = Convert.ToInt32(result);
                    }
                }
            }
            dao.CloseCon();
            return newStaffID;
        }

        public bool UpdateStaffUserSuspension(int staffID, bool isSuspended)
        {
            int rowsAffected = 0;
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateStaffUserSuspension", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StaffID", staffID);
                    cmd.Parameters.AddWithValue("@IsSuspended", isSuspended);
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            dao.CloseCon();
            return rowsAffected > 0;
        }

        public List<StaffUser> GetAllStaffUsers()
        {
            List<StaffUser> users = new List<StaffUser>();
            using (SqlConnection conn = dao.OpenCon())
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllStaffUsers", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new StaffUser
                            {
                                StaffID = Convert.ToInt32(reader["StaffID"]),
                                Username = reader["Username"].ToString(),
                                Role = reader["Role"].ToString(),
                                IsSuspended = Convert.ToBoolean(reader["IsSuspended"]),
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString()
                            });
                        }
                    }
                }
            }
            dao.CloseCon();
            return users;
        }
    }
}
