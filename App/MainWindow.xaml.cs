using App.Entities;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Account> _accounts = new List<Account>();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            _accounts.AddRange(new List<Account>
            {
                new Account
                {
                    AppName = "Riot",
                    Username = "user1",
                    Password = "password1"
                },
                new Account
                {
                    AppName = "Steam",
                    Username = "user2",
                    Password = "password2"
                },
                new Account
                {
                    AppName = "Epic",
                    Username = "user3",
                    Password = "password3"
                }
            });

            if(File.Exists("accounts.json"))
            {
                var accounts = File.ReadAllText("accounts.json");
                _accounts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Account>>(accounts);
            }

            AccountComboBox.ItemsSource = _accounts;
        }

        private void AccountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoginButton.IsEnabled = AccountComboBox.SelectedItem != null;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountComboBox.SelectedItem is Account selectedAccount)
            {
                AutoFillLogin(selectedAccount.AppName, selectedAccount.Username, selectedAccount.Password);
            }
        }

        private async void AutoFillLogin(string appName, string username, string password)
        {
            try
            {
                string processName = GetProcessName(appName);
                ForcusApplication(processName);

                await WindowsInput.Simulate.Events()
                    .Click(username)
                    .Click("{TAB}")
                    .Click(password)
                    .Click("{ENTER}")
                    .Invoke();

            }
            catch (Exception ex)
            {

                MessageBox.Show($"Lỗi : {ex.Message}");
            }
        }

        private string GetProcessName(string appName)
        {
            return appName.ToLower() switch
            {
                "riot" => "RiotClientServices",
                "steam" => "Steam",
                _ => throw new Exception("Ứng dụng không được hỗ trợ!")
            };
        }

        private void ForcusApplication(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                SetForegroundWindow(processes[0].MainWindowHandle);
                System.Threading.Thread.Sleep(500);
            }
            else
            {
                MessageBox.Show($"{processName} chưa chạy");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(_accounts);
            File.WriteAllText("accounts.json", json);
        }
    }
}