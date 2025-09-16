using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class AuditData : ObservableObject // Represents an audit in the AuditLog table
    {
        private int _auditID;
        private DateTime _timestamp;
        private int _performingStaffID;
        private string _actionDescription;
        private string _targetEntity;
        private string _targetEntityID;

        // Populated from staff table
        private string _performingUsername;

        // Public Properties
        public int AuditID
        {
            get => _auditID;
            set { _auditID = value; OnPropertyChanged(); }
        }
        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimestampDisplay)); }
        }
        public int PerformingStaffID
        {
            get => _performingStaffID;
            set { _performingStaffID = value; OnPropertyChanged(); OnPropertyChanged(nameof(StaffInfo)); }
        }
        public string ActionDescription
        {
            get => _actionDescription;
            set { _actionDescription = value; OnPropertyChanged(); }
        }
        public string TargetEntity
        {
            get => _targetEntity;
            set { _targetEntity = value; OnPropertyChanged(); }
        }
        public string TargetEntityID
        { // Stored as string
            get => _targetEntityID;
            set { _targetEntityID = value; OnPropertyChanged(); }
        }

        public string PerformingUsername
        {
            get => _performingUsername;
            set { _performingUsername = value; OnPropertyChanged(); OnPropertyChanged(nameof(StaffInfo)); }
        }

        // Read only Timepstamp for the UI
        public string TimestampDisplay => Timestamp.ToString("g"); // General short date/time
        public string StaffInfo => $"{PerformingUsername ?? "N/A"} (ID: {PerformingStaffID})";
    }
}