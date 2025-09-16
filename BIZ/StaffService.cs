using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DAL.Models;

namespace BIZ
{
    public class StaffService
    {
        private UserDAL userDAL = new UserDAL();

        public int CreateStaffUser(string username, string password, string role, string firstName, string lastName, StaffUser creatingAdmin)
        {
            if (creatingAdmin == null || creatingAdmin.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only Admins can create staff users.");
            }
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("Username, password, role, and first name are required.");
            }
            if (role != "Teller" && role != "Manager" && role != "Admin") // Ensure it is a valid role
            {
                throw new ArgumentException("Invalid role specified.");
            }


            // Check if username exists
            if (userDAL.GetUserByUsername(username) != null)
            {
                throw new ArgumentException($"Username '{username}' already exists.");
            }


            try
            {
                var (hash, salt) = PasswordHelper.HashPassword(password);
                return userDAL.CreateStaffUser(username, hash, salt, role, firstName, lastName);
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Database error creating staff: {dbEx.Message}");
                throw new ApplicationException("Database error creating staff user.", dbEx);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Generic error creating staff: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred creating staff user.", ex);
            }
        }

        public bool ToggleStaffSuspension(int staffID, bool suspend, StaffUser actingAdmin)
        {
            if (actingAdmin == null || actingAdmin.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only Admins can manage staff suspension.");
            }

            try
            {
                return userDAL.UpdateStaffUserSuspension(staffID, suspend);
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Database error updating suspension: {dbEx.Message}");
                throw new ApplicationException("Database error updating staff suspension.", dbEx);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Generic error updating suspension: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred updating staff suspension.", ex);
            }
        }

        public List<StaffUser> GetAllStaff(StaffUser requestingUser)
        {
            if (requestingUser == null || requestingUser.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only Admins can view all staff.");
            }
            try
            {
                // DAL returns all staff
                return userDAL.GetAllStaffUsers();
            }
            catch (System.Data.SqlClient.SqlException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error getting staff list: {dbEx.Message}");
                throw new ApplicationException("Database error retrieving staff list.", dbEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error getting staff list: {ex.Message}");
                throw new ApplicationException("An unexpected error occurred retrieving staff list.", ex);
            }
        }
    }
}

