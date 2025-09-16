using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models
{
    public class Staff : ObservableObject
    {
        // Private backing fields
        private int _staffID;
        private string _username;
        private string _passwordHash;
        private string _passwordSalt; 
        private string _firstName;
        private string _lastName;
        private StaffRole _staffRole;
        private bool _isActive;

        // Property used by DAL to load string role before conversion
        public string RoleString { get; set; }

        // Public Properties
        public int StaffID
        {
            get => _staffID;
            set
            {
                _staffID = value;
                OnPropertyChanged();
            }
        }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName)); // Update computed property
            }
        }
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName)); // Update computed property
            }
        }
        public StaffRole StaffRole
        {
            get => _staffRole;
            set
            {
                _staffRole = value;
                OnPropertyChanged();
            }
        }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayStatus)); // Update computed property
            }
        }

        public string PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = value; // No OnPropertyChanged needed
        }
        public string PasswordSalt
        {
            get => _passwordSalt;
            set => _passwordSalt = value; // No OnPropertyChanged needed
        }


        // Read only values for the UI
        public string FullName => $"{FirstName} {LastName}";
        public string DisplayStatus => IsActive ? "Active" : "Suspended";
    }
}
