using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class StaffUser
    {
        public int StaffID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string Role { get; set; } // Teller, Manager, Admin
        public bool IsSuspended { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

