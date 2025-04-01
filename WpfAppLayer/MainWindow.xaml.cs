using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput.Events;
using WpfAppLayer.Entities;

namespace WpfAppLayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Account> _accounts = new List<Account>();
        private Dictionary<string, Point> _positions = new Dictionary<string, Point>();
        private Point? _tempPosition;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private struct POINT
        {
            public int X;
            public int Y;
        }

        private const int SW_RESTORE = 9;

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            LoadPositions();
        }

        private void LoadAccounts()
        {
            if (File.Exists("accounts.json"))
            {
                var accounts = File.ReadAllText("accounts.json");
                _accounts = JsonConvert.DeserializeObject<List<Account>>(accounts);
            }

            AccountListBox.ItemsSource = _accounts;
        }

        private void LoadPositions()
        {
            if (File.Exists("positions.json"))
            {
                var positionsJson = File.ReadAllText("positions.json");
                _positions = JsonConvert.DeserializeObject<Dictionary<string, Point>>(positionsJson);
            }
        }

        private void AccountListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoginButton.IsEnabled = AccountListBox.SelectedItem != null;
            SetPositionButton.IsEnabled = AccountListBox.SelectedItem != null;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListBox.SelectedItem is Account selectedAccount)
            {
                await LoginToRiot(selectedAccount.Username, selectedAccount.Password, selectedAccount.AppName);
            }
        }

        private void ManageAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            var managerWindow = new ManagerAccountWindow(_accounts);
            managerWindow.ShowDialog();
            AccountListBox.ItemsSource = null;
            AccountListBox.ItemsSource = _accounts;
        }

        private void SetPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListBox.SelectedItem is Account selectedAccount)
            {
                Process riotProcess = StartOrFocusRiotClient();
                if (riotProcess == null)
                {
                    MessageBox.Show("Không tìm thấy Riot Client!");
                    return;
                }

                MessageBox.Show("Hãy chuyển sang Riot Client và nhấp chuột trái vào vị trí trường username.");
                _tempPosition = null;

                MouseHook.Start();
                MouseHook.MouseAction += MouseHook_MouseAction;
            }
        }

        private void MouseHook_MouseAction(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GetCursorPos(out POINT point);
                _tempPosition = new Point(point.X, point.Y);
                MouseHook.Stop();
                MouseHook.MouseAction -= MouseHook_MouseAction;

                Dispatcher.Invoke(() =>
                {
                    if (MessageBox.Show($"Xác nhận lưu vị trí X: {_tempPosition.Value.X}, Y: {_tempPosition.Value.Y}?",
                        "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (AccountListBox.SelectedItem is Account selectedAccount)
                        {
                            _positions[selectedAccount.AppName] = _tempPosition.Value;
                            SavePositions();
                            MessageBox.Show("Đã lưu vị trí thành công!");
                        }
                    }
                    _tempPosition = null;
                });
            }
        }

        private async Task LoginToRiot(string username, string password, string appName)
        {
            try
            {
                Process riotProcess = StartOrFocusRiotClient();
                if (riotProcess == null)
                {
                    MessageBox.Show("Không tìm thấy Riot Client. Vui lòng mở ứng dụng trước!");
                    return;
                }

                if (_positions.ContainsKey(appName))
                {
                    await Task.Delay(1000);
                    var position = _positions[appName];
                    await WindowsInput.Simulate.Events()
                        .MoveTo((int)position.X, (int)position.Y)
                        .Click(ButtonCode.Left)
                        .Wait(200)
                        .Click(username)
                        .Wait(200)
                        .Click(KeyCode.Tab)
                        .Wait(200)
                        .Click(password)
                        .Wait(200)
                        .Click(KeyCode.Enter)
                        .Invoke();
                }
                else
                {
                    await Task.Delay(2000);
                    await WindowsInput.Simulate.Events()
                        .Wait(500)
                        .Click(username)
                        .Wait(500)
                        .Click(KeyCode.Tab)
                        .Wait(500)
                        .Click(password)
                        .Wait(500)
                        .Click(KeyCode.Enter)
                        .Invoke();
                }

                MessageBox.Show("Đăng nhập thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng nhập: {ex.Message}");
            }
        }

        private Process StartOrFocusRiotClient()
        {
            Process[] processes = Process.GetProcessesByName("Riot Client");
            if (processes.Length > 0)
            {
                Process riotProcess = processes[0];
                IntPtr hWnd = riotProcess.MainWindowHandle;

                if (hWnd != IntPtr.Zero)
                {
                    SetForegroundWindow(hWnd);
                    ShowWindow(hWnd, SW_RESTORE);
                    Thread.Sleep(500);
                    return riotProcess;
                }
            }
            try
            {
                string riotPath = @"E:\Riot Games\Riot Client\RiotClientServices.exe";
                if (File.Exists(riotPath))
                {

                    Process.Start(riotPath);
                    Thread.Sleep(5000);
                    return Process.GetProcessesByName("RiotClientServices")[0];
                }
                else
                {
                    MessageBox.Show("Không tìm thấy file RiotClientServices.exe tại đường dẫn mặc định!");
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void SavePositions()
        {
            string json = JsonConvert.SerializeObject(_positions);
            File.WriteAllText("positions.json", json);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string json = JsonConvert.SerializeObject(_accounts);
            File.WriteAllText("accounts.json", json);
            SavePositions();
        }
    }

    public static class MouseHook
    {
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static event EventHandler<MouseEventArgs> MouseAction;

        public static void Start()
        {
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                MouseAction?.Invoke(null, new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private struct POINT
        {
            public int x;
            public int y;
        }

        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    // Cập nhật MouseEventArgs nếu cần
    public class MouseEventArgs : EventArgs
    {
        public MouseButtons Button { get; }
        public int Clicks { get; }
        public int X { get; }
        public int Y { get; }
        public int Delta { get; }

        public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
        {
            Button = button;
            Clicks = clicks;
            X = x;
            Y = y;
            Delta = delta;
        }
    }

    public enum MouseButtons
    {
        Left,
        Right,
        Middle
    }
}