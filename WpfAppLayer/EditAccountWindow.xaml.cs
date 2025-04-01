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
using System.Windows.Shapes;
using WpfAppLayer.Entities;

namespace WpfAppLayer
{
    /// <summary>
    /// Interaction logic for EditAccountWindow.xaml
    /// </summary>
    public partial class EditAccountWindow : Window
    {
        public Account Account { get; private set; }

        public EditAccountWindow(Account account)
        {
            InitializeComponent();
            Account = account ?? new Account();

            AppNameTextBox.Text = Account.AppName ?? string.Empty;
            UsernameTextBox.Text = Account.Username ?? string.Empty;
            PasswordBox.Password = Account.Password ?? string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Account.AppName = AppNameTextBox.Text;
            Account.Username = UsernameTextBox.Text;
            Account.Password = PasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
