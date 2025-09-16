using BankApp.Pages; // The namespace for the Pages
using BIZ;
using DAL.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BankApp
{
    public partial class MainWindow : Window
    {
        private AuthService _authService = new AuthService();
        private AccountService _accountService = new AccountService();

        public MainWindow()
        {
            InitializeComponent();
            NavigateToLogin();
            UpdateMenuVisibility();
        }

        private void NavigateToLogin()
        {
            MainFrame.Navigate(new LoginPage(this));
        }

        public void UserLoggedIn(StaffUser user)
        {
            App.CurrentUser = user;
            UpdateMenuVisibility();
            StatusBarUserGreeting.Text = $"Welcome, {user.FirstName ?? user.Username} ({user.Role})";
            StatusBarMessage.Text = "Login Successful.";
            MainFrame.Navigate(new DashboardPage(this));
        }

        public void UserLoggedOut()
        {
            App.CurrentUser = null;
            UpdateMenuVisibility();
            StatusBarUserGreeting.Text = "Not logged in";
            StatusBarMessage.Text = "Logout Successful.";
            NavigateToLogin();
        }

        private void UpdateMenuVisibility()
        {
            bool isLoggedIn = App.CurrentUser != null;
            string role = App.CurrentUser?.Role;

            MenuItemLogin.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            MenuItemLogout.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            MenuAccount.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            MenuManage.Visibility = isLoggedIn && (role == "Manager" || role == "Admin") ? Visibility.Visible : Visibility.Collapsed;

            MenuItemApproveTransactions.Visibility = isLoggedIn && (role == "Manager" || role == "Admin") ? Visibility.Visible : Visibility.Collapsed;
            MenuItemManageStaff.Visibility = isLoggedIn && role == "Admin" ? Visibility.Visible : Visibility.Collapsed;

            MenuItemEditAccount.IsEnabled = isLoggedIn;
            MenuItemDeposit.IsEnabled = isLoggedIn;
            MenuItemWithdraw.IsEnabled = isLoggedIn;
            MenuItemTransfer.IsEnabled = isLoggedIn;
            MenuItemViewTransactions.IsEnabled = isLoggedIn;
            MenuItemExportXml.IsEnabled = isLoggedIn;
        }

        public void ShowStatusMessage(string message, bool isError = false)
        {
            StatusBarMessage.Text = message;
            StatusBarMessage.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black;
        }


        // --- Menu Click Handlers ---
        private void MenuItemLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLogin();
        }

        private void MenuItemLogout_Click(object sender, RoutedEventArgs e)
        {
            UserLoggedOut();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemNewAccount_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
                MainFrame.Navigate(new NewAccountPage(this));
        }

        private void MenuItemEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                MainFrame.Navigate(new EditAccountPage(this, selectedAccount.AccountID));
            }
            else
            {
                MessageBox.Show("Please select an account from the dashboard first.",
                                "Account Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void MenuItemDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                MainFrame.Navigate(new DepositPage(this, selectedAccount.AccountID));
            }
            else
            {
                MessageBox.Show("Please select an account from the dashboard first.",
                                "Account Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void MenuItemWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                MainFrame.Navigate(new WithdrawPage(this, selectedAccount.AccountID));
            }
            else
            {
                MessageBox.Show("Please select an account from the dashboard first.",
                               "Account Selection Required",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void MenuItemTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                MainFrame.Navigate(new TransferPage(this, selectedAccount.AccountID));
            }
            else
            {
                MessageBox.Show("Please select a source account from the dashboard first.",
                               "Account Selection Required",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void MenuItemViewTransactions_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                MainFrame.Navigate(new ViewTransactionsPage(this, selectedAccount.AccountID));
            }
            else
            {
                MessageBox.Show("Please select an account from the dashboard first.",
                               "Account Selection Required",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void MenuItemExportXml_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DashboardPage dashboard && dashboard.AccountsGrid.SelectedItem is Account selectedAccount)
            {
                try
                {
                    string xmlData = _accountService.SerializeAccountToXml(selectedAccount.AccountID, App.CurrentUser);

                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                    saveFileDialog.FileName = $"Account_{selectedAccount.AccountID}.xml";
                    saveFileDialog.Filter = "XML Files (*.xml)|*.xml";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, xmlData);
                        ShowStatusMessage($"Account {selectedAccount.AccountID} exported successfully.");
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Error exporting account: {ex.Message}", true);
                    MessageBox.Show($"Error exporting account: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an account from the dashboard first.",
                               "Account Selection Required",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void MenuItemApproveTransactions_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null && (App.CurrentUser.Role == "Manager" || App.CurrentUser.Role == "Admin"))
                MainFrame.Navigate(new ApproveTransactionsPage(this));
        }

        private void MenuItemManageStaff_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null && App.CurrentUser.Role == "Admin")
                MainFrame.Navigate(new ManageStaffPage(this));
        }

        public void NavigateToDashboard()
        {
            MainFrame.Navigate(new DashboardPage(this));
        }
    }
}