using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DAL.Models;

namespace BIZ
{
    public class AuthService
    {
        private UserDAL userDAL = new UserDAL();

        public StaffUser Login(string username, string password)
        {
            try
            {
                // Input Validation: Ensure the username and password fields are not empty
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return null;
                }

                StaffUser user = userDAL.GetUserByUsername(username);

                if (user != null && !user.IsSuspended)
                {
                    bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash, user.Salt);
                    if (isPasswordValid)
                    {
                        // Clear sensitive data before returning to UI layer
                        user.PasswordHash = null;
                        user.Salt = null;
                        return user;
                    }
                }
                return null; // Return null, if login failed (user not found, suspended, or wrong password)
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                // Log error (using system diagnostics)
                System.Diagnostics.Debug.WriteLine($"Database error during login: {dbEx.Message}");
                throw new ApplicationException("Database error during login. Please try again later.", dbEx);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Generic error during login: {ex.Message}");
                throw new ApplicationException($"An unexpected error occurred during login. {ex.Message}", ex);
            }
        }
    }
}
