using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class BugReport : ObservableObject
    {
        // Private fields
        private int _reportID;
        private string _reportDescription;
        private DateTime _reportTimestamp;
        private int? _reportedByStaffID;
        private string _status; // Keep as string for  matching CHECK constraint

        // Populated from the DAO sql join data
        private string _reporterName;

        // Public Properties
        public int ReportID
        {
            get => _reportID;
            set { _reportID = value; OnPropertyChanged(); }
        }
        public string ReportDescription
        {
            get => _reportDescription;
            set { _reportDescription = value; OnPropertyChanged(); }
        }
        public DateTime ReportTimestamp
        {
            get => _reportTimestamp;
            set { _reportTimestamp = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReportTimestampDisplay)); }
        }
        public int? ReportedByStaffID
        { 
            get => _reportedByStaffID;
            set { _reportedByStaffID = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string ReporterName
        {
            get => _reporterName;
            set { _reporterName = value; OnPropertyChanged(); }
        }

        // Timestamp to be dislayed on the UI
        public string ReportTimestampDisplay => ReportTimestamp.ToString("g"); // General short date/time format
    }
}
