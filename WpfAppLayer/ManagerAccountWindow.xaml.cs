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
    /// Interaction logic for ManagerAccountWindow.xaml
    /// </summary>
    public partial class ManagerAccountWindow : Window
    {
        private List<Account> _accounts;

        public ManagerAccountWindow(List<Account> accounts)
        {
            InitializeComponent();
            _accounts = accounts;
            AccountList.ItemsSource = _accounts;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditAccountWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                _accounts.Add(editWindow.Account);
                AccountList.Items.Refresh();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountList.SelectedItem is Account selectedAccount)
            {
                var editWindow = new EditAccountWindow(selectedAccount);
                if (editWindow.ShowDialog() == true)
                {
                    AccountList.Items.Refresh();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountList.SelectedItem is Account selectedAccount)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa tài khoản này?",
                    "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _accounts.Remove(selectedAccount);
                    AccountList.Items.Refresh();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
