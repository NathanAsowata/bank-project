using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BIZ;
using DAL.Models;

namespace BankApp.Pages
{
    public partial class LoginPage : Page
    {
        private AuthService _authService = new AuthService();
        private MainWindow _mainWindow;
        private bool _isPasswordVisible = false; // Track Password visibility state

        public LoginPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            TxtUsername.Focus();
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Show Password
                TxtPasswordVisible.Text = TxtPassword.Password; // Copy password to TextBox
                TxtPassword.Visibility = Visibility.Collapsed;
                TxtPasswordVisible.Visibility = Visibility.Visible;
                BtnTogglePasswordVisibility.Content = "\uE7C1"; // Hide icon (Blocked2 or similar)
                TxtPasswordVisible.Focus(); // Set focus to visible control
                TxtPasswordVisible.CaretIndex = TxtPasswordVisible.Text.Length; // Move cursor to end
            }
            else
            {
                // Hide Password
                TxtPassword.Password = TxtPasswordVisible.Text; // Copy password back to PasswordBox
                TxtPasswordVisible.Visibility = Visibility.Collapsed;
                TxtPassword.Visibility = Visibility.Visible;
                BtnTogglePasswordVisibility.Content = "\uE7B3"; // Show icon (RedEye)
                TxtPassword.Focus(); // Set focus to visible control
                // PasswordBox doesn't expose CaretIndex easily, focus is usually sufficient
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
            string username = TxtUsername.Text;
            // Get password from the currently visible control
            string password = _isPasswordVisible ? TxtPasswordVisible.Text : TxtPassword.Password;

            try
            {
                StaffUser user = _authService.Login(username, password);

                if (user != null)
                {
                    _mainWindow.UserLoggedIn(user);
                }
                else
                {
                    ErrorMessage.Text = "Invalid username or password.";
                    ErrorMessage.Visibility = Visibility.Visible;
                    _mainWindow.ShowStatusMessage("Login failed.", true);
                }
            }
            catch (ApplicationException appEx)
            {
                ErrorMessage.Text = $"Login Error: {appEx.Message}";
                ErrorMessage.Visibility = Visibility.Visible;
                _mainWindow.ShowStatusMessage($"Login Error: {appEx.Message}", true);
               
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"An unexpected error occurred: {ex.Message}";
                ErrorMessage.Visibility = Visibility.Visible;
                _mainWindow.ShowStatusMessage($"Unexpected login error: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"Unexpected Login Error: {ex}");
            }
            finally
            {
                // Clear fields after attempt for security
                TxtUsername.Clear();
                TxtPassword.Clear();
                TxtPasswordVisible.Clear();
                // Ensure password is hidden again if it was visible
                if (_isPasswordVisible)
                {
                    // Reset the password state without calling the toggle handler again
                    _isPasswordVisible = false;
                    TxtPasswordVisible.Visibility = Visibility.Collapsed;
                    TxtPassword.Visibility = Visibility.Visible;
                    BtnTogglePasswordVisibility.Content = "\uE7B3";
                }
            }
        }
    }
}