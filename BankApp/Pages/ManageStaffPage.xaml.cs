using BIZ;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BankApp.Pages
{
    public partial class ManageStaffPage : Page, INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private StaffService _staffService = new StaffService();

        private ObservableCollection<StaffUser> _staffUsers;
        public ObservableCollection<StaffUser> StaffUsers
        {
            get { return _staffUsers; }
            set { _staffUsers = value; OnPropertyChanged(nameof(StaffUsers)); }
        }

        public ManageStaffPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            DataContext = this;
            CmbNewRole.SelectedIndex = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser?.Role != "Admin")
            {
                MessageBox.Show("Access denied. Only Administrators can manage staff.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                _mainWindow.NavigateToDashboard();
                return;
            }
            LoadStaffData();
        }

        private void LoadStaffData()
        {
            try
            {
                List<StaffUser> staffList = _staffService.GetAllStaff(App.CurrentUser);
                StaffUsers = new ObservableCollection<StaffUser>(staffList);
                _mainWindow.ShowStatusMessage($"Loaded {StaffUsers.Count} staff users.");
            }
            catch (Exception ex)
            {
                HandleError("Error loading staff data", ex);
                StaffUsers = new ObservableCollection<StaffUser>();
            }
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtNewUsername.Text.Trim();
            string password = TxtNewPassword.Password;
            string firstName = TxtNewFirstName.Text.Trim();
            string lastName = TxtNewLastName.Text.Trim();
            string role = (CmbNewRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Username, Password, First Name, and Role are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int newStaffId = _staffService.CreateStaffUser(username, password, role, firstName, lastName, App.CurrentUser);
                MessageBox.Show($"Staff user '{username}' created successfully with ID {newStaffId}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainWindow.ShowStatusMessage($"User {username} created.");
                LoadStaffData();
                TxtNewUsername.Clear();
                TxtNewPassword.Clear();
                TxtNewFirstName.Clear();
                TxtNewLastName.Clear();
                CmbNewRole.SelectedIndex = 0;
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(argEx.Message, "Creation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                _mainWindow.ShowStatusMessage(argEx.Message, true);
            }
            catch (Exception ex)
            {
                HandleError("Error creating staff user", ex);
            }
        }

        private void Suspend_Click(object sender, RoutedEventArgs e)
        {
            ToggleSuspension(true);
        }

        private void Reactivate_Click(object sender, RoutedEventArgs e)
        {
            ToggleSuspension(false);
        }

        private void ToggleSuspension(bool suspend)
        {
            if (StaffGrid.SelectedItem is StaffUser selectedUser)
            {
                if (selectedUser.StaffID == App.CurrentUser.StaffID)
                {
                    MessageBox.Show("You cannot suspend your own account.", "Action Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string action = suspend ? "suspend" : "reactivate";
                if (MessageBox.Show($"Are you sure you want to {action} user '{selectedUser.Username}'?", $"Confirm {action}", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _staffService.ToggleStaffSuspension(selectedUser.StaffID, suspend, App.CurrentUser);
                        if (success)
                        {
                            MessageBox.Show($"User '{selectedUser.Username}' has been {action}d.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            _mainWindow.ShowStatusMessage($"User {selectedUser.Username} {action}d.");
                            LoadStaffData();
                        }
                        else
                        {
                            MessageBox.Show($"Failed to {action} user. An error occurred or the user was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError($"Error {action}ing user", ex);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a staff user from the grid first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDashboard();
            }
        }

        private void HandleError(string context, Exception ex)
        {
            string message = $"{context}: {ex.Message}";
            _mainWindow.ShowStatusMessage(message, true);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"{context}: {ex}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}