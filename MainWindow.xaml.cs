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
using System.Configuration;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Win32.SafeHandles;
using System.Data;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace floating_clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// color palette
    public partial class MainWindow : Window
    {
        // Windows API 调用，用于控制窗口在 Alt+Tab 中的显示
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        public DataModel data = new DataModel();
        private DispatcherTimer timer = new DispatcherTimer();
        private WinForms.NotifyIcon? notifyIcon;
        private WinForms.ToolStripMenuItem? showHideMenuItem;
        private WinForms.ToolStripMenuItem? autoStartMenuItem;
        private bool allowClose = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;

            // 初始化系统托盘
            InitializeNotifyIcon();

            // 初始化计时器
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            this.DataContext = data;

        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new WinForms.NotifyIcon();
            try
            {
                // 尝试从资源中加载图标
                var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/icon.ico"));
                if (iconStream != null)
                {
                    notifyIcon.Icon = new System.Drawing.Icon(iconStream.Stream);
                }
                else
                {
                    // 备选方案：从文件系统加载
                    string iconPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "icon.ico");
                    if (System.IO.File.Exists(iconPath))
                    {
                        notifyIcon.Icon = new System.Drawing.Icon(iconPath);
                    }
                    else
                    {
                        notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
            }
            catch
            {
                notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            notifyIcon.Text = "Floating Clock - 悬浮时钟";
            notifyIcon.Visible = true;

            // 双击托盘图标显示/隐藏窗口
            notifyIcon.DoubleClick += (s, e) => ToggleWindowVisibility();

            // 创建托盘右键菜单
            var contextMenu = new WinForms.ContextMenuStrip();
            showHideMenuItem = new WinForms.ToolStripMenuItem("显示", null, (s, e) => ToggleWindowVisibility());
            autoStartMenuItem = new WinForms.ToolStripMenuItem("开机自启", null, (s, e) => ToggleAutoStart());
            
            contextMenu.Items.Add(showHideMenuItem);
            contextMenu.Items.Add(autoStartMenuItem);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("退出", null, (s, e) => ExitApplication());
            
            // 设置菜单打开时更新状态
            contextMenu.Opening += (s, e) => UpdateMenuItemsStatus();
            
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ToggleWindowVisibility()
        {
            if (Visibility == Visibility.Visible && WindowState != WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
            
            // 更新菜单状态
            UpdateMenuItemsStatus();
        }

        private void ToggleAutoStart()
        {
            try
            {
                RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk != null)
                {
                    string appName = "FloatingClock";
                    string exePath = System.AppContext.BaseDirectory + "floating_clock.exe";
                    
                    if (rk.GetValue(appName) == null)
                    {
                        // 添加自启动，带最小化参数
                        rk.SetValue(appName, $"\"{exePath}\" --minimized");
                    }
                    else
                    {
                        // 移除自启动
                        rk.DeleteValue(appName, false);
                    }
                    rk.Close();
                    
                    // 立即更新菜单状态
                    UpdateMenuItemsStatus();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"设置自启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsAutoStartEnabled()
        {
            try
            {
                RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                if (rk != null)
                {
                    string appName = "FloatingClock";
                    bool isEnabled = rk.GetValue(appName) != null;
                    rk.Close();
                    return isEnabled;
                }
            }
            catch
            {
                // 如果出现异常，返回false
            }
            return false;
        }

        private void UpdateMenuItemsStatus()
        {
            if (showHideMenuItem != null)
            {
                // 根据窗口当前显示状态设置勾选
                showHideMenuItem.Checked = (Visibility == Visibility.Visible && WindowState != WindowState.Minimized);
            }
            
            if (autoStartMenuItem != null)
            {
                // 根据自启动状态设置勾选
                autoStartMenuItem.Checked = IsAutoStartEnabled();
            }
        }

        private void ExitApplication()
        {
            allowClose = true;
            data.SaveSetting();
            notifyIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {

                var x = Properties.Setting.Default.PosLeft;
                var y = Properties.Setting.Default.PosTop;
                if (x != this.Left || y != this.Top)
                {
                    Properties.Setting.Default.PosLeft = this.Left;
                    Properties.Setting.Default.PosTop = this.Top;
                    Properties.Setting.Default.Save();
                }
                 
            }

        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void MenuItem_Click_Minimize(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MenuItem_Click_Help(object sender, RoutedEventArgs e)
        {

            var helpWindow = new HelpWindow(this);

            var text = "";
            text += "- 窗口打开来源: 右键菜单\n";

            text += "- 配置文件保存在: \n";
            text += ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            
            // 需要检查HelpWindow的实际实现来设置文本
            // helpWindow.info.Text = text;

            helpWindow.Show();
            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            data.Update();

            var x = Properties.Setting.Default.PosLeft;
            var y = Properties.Setting.Default.PosTop;
            if(x != this.Left || y != this.Top){
                Properties.Setting.Default.PosLeft = this.Left;
                Properties.Setting.Default.PosTop = this.Top;
                data.SaveSetting();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置窗口样式，使其不在 Alt+Tab 中显示
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
           
            var x = Properties.Setting.Default.PosLeft;
            var y = Properties.Setting.Default.PosTop;

            if(x+y < 0)
            {
                var workingArea = SystemParameters.WorkArea;
                this.Left = workingArea.Right - this.Width-10;
                this.Top = workingArea.Top  + this.Height;
            }
            else
            {
                this.Left = x;
                this.Top = y;
            }

            // 检查是否需要最小化启动
            if (System.Windows.Application.Current.Properties.Contains("StartMinimized"))
            {
                WindowState = WindowState.Minimized;
                Hide();
            }

            //MessageBox.Show($"{x},{y}");
     
        }
    }


    /// <summary>
    /// 主数据模型
    /// </summary>
    public class DataModel : INotifyPropertyChanged
    {
        public bool mark_setting_changed = false;

        SystemMonitor systemMonitor = new SystemMonitor();
        long last_up = 0;
        long last_down = 0;
        DateTime last_save_setting_at = DateTime.MinValue;

        public string Time { get; set; } = "";
        public string CPU { get; set; } = "";
        public string RAM { get; set; } = "";
        public string Upload { get; set; } = "";
        public string Download { get; set; } = "";

        

        //private ColorOptions _color = ColorOptions.blue;
        //public ColorOptions Color
        //{
        //    get => _color;
        //    set
        //    {
        //        if (_color != value)
        //        {
        //            _color = value;
        //            OnPropertyChanged(nameof(Color));
        //        }
        //    }
        //}
        // 不想用枚举类型了，底层直接用字符串
       private string _color_name = "blue";
        public string ColorName
        {
            get => _color_name;
            set
            {
                if (_color_name != value)
                {
                    _color_name = value;
                    ChangeSetting();
                    OnPropertyChanged(nameof(ColorName));
                }
            }
        } 

        //public ObservableProperty<bool> Topmost { get; set; } = new();
        //public ObservableProperty<string> Topmost { get; set; } = new("True");

        public string VisibilityClock { get; set; } = "Visible";
        public string VisibilityCPU { get; set; } = "Visible";
        public string VisibilityRAM { get; set; } = "Visible";
        public string VisibilityUpload { get; set; } = "Visible";
        public string VisibilityDownload { get; set; } = "Visible";




        bool topmost = true;
        bool showClock = true;
        bool showCPU = true;
        bool showRAM = true;
        bool showUpload = true;
        bool showDownload = true;

        public bool Topmost
        {
            get { return topmost; }
            set
            {
                if (!Equals(this.topmost, value))
                {
                    this.topmost = value;
                    ChangeSetting();
                    OnPropertyChanged(nameof(Topmost));
                }
            }
        }

        public bool ShowClock
        {
            get { return showClock; }
            set
            {
                if (!Equals(this.showClock, value))
                {
                    this.showClock = value;
                    VisibilityClock = value ? "Visible" : "Collapsed";
                    ChangeSetting();
                    OnPropertyChanged(nameof(VisibilityClock));
                    OnPropertyChanged(nameof(ShowClock));
                }
            }
        }

        public bool ShowCPU
        {
            get { return showCPU; }
            set
            {
                if (!Equals(this.showCPU, value))
                {
                    this.showCPU = value;
                    VisibilityCPU = value ? "Visible" : "Collapsed";
                    ChangeSetting();
                    OnPropertyChanged(nameof(VisibilityCPU));
                    OnPropertyChanged(nameof(ShowCPU));
                }
            }
        }

        public bool ShowRAM
        {
            get { return showRAM; }
            set
            {
                if (!Equals(this.showRAM, value))
                {
                    this.showRAM = value;
                    VisibilityRAM = value ? "Visible" : "Collapsed";
                    ChangeSetting();
                    OnPropertyChanged(nameof(VisibilityRAM));
                    OnPropertyChanged(nameof(ShowRAM));
                }
            }
        }

        public bool ShowUpload
        {
            get { return showUpload; }
            set
            {
                if (!Equals(this.showUpload, value))
                {
                    this.showUpload = value;
                    VisibilityUpload = value ? "Visible" : "Collapsed";
                    ChangeSetting();
                    OnPropertyChanged(nameof(VisibilityUpload));
                    OnPropertyChanged(nameof(ShowUpload));
                }
            }
        }

        public bool ShowDownload
        {
            get { return showDownload; }
            set
            {
                if (!Equals(this.showDownload, value))
                {
                    this.showDownload = value;
                    VisibilityDownload = value ? "Visible" : "Collapsed";
                    ChangeSetting();
                    OnPropertyChanged(nameof(VisibilityDownload));
                    OnPropertyChanged(nameof(ShowDownload));
                }
            }
        }


        public DataModel()
        {
            Update();
            LoadSetting();
            // ChangeSetting();

        }

        public DataModel Update()
        {
           var now = DateTime.Now;
            Time = now.ToString("HH : mm : ss");

            var cpu = systemMonitor.GetCpuUsage();
            var ram = systemMonitor.GetAvailableRam();

            CPU = $"{cpu:F2}%";
            RAM = Util.ConvertBytesToReadableSize(ram);

            // 计算网速
            long up = 0;
            long down = 0;

            // 获取网卡列表，和流量计数
            var net = systemMonitor.GetNetworkStatistics();
            foreach (var item in net)
            {
                up += item.BytesSent;
                down += item.BytesReceived;
            }

            if (last_down + last_up > 0) // 第一次获取流量，跳过计算
            {
                Upload = Util.ConvertBytesToReadableSize((up - last_up) / 1000.0, "kmg") + "/s";
                Download = Util.ConvertBytesToReadableSize((down - last_down) / 1000.0, "kmg") + "/s";
            }
            else
            {
                Upload = "0/ks";
                Download = "0/ks";
            }

            last_up = up;
            last_down = down;


            // 更新视图
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(CPU));
            OnPropertyChanged(nameof(RAM));
            OnPropertyChanged(nameof(Upload));
            OnPropertyChanged(nameof(Download));


            if((now - last_save_setting_at).TotalSeconds > 5)
            {
                last_save_setting_at = now;
                SaveSetting();
            }


            return this;
        }


        public void LoadSetting()
        {
            //var _ = ConfigurationManager.AppSettings;
            //Topmost = bool.Parse(_["Topmost"] ?? "true");
            //ShowClock = bool.Parse(_["ShowClock"] ?? "true");
            //ShowCPU = bool.Parse(_["ShowCPU"] ?? "true");
            //ShowRAM = bool.Parse(_["ShowRAM"] ?? "true");
            //ShowUpload = bool.Parse(_["ShowUpload"] ?? "true");
            //ShowDownload = bool.Parse(_["ShowDownload"] ?? "true");

            Topmost = Properties.Setting.Default.Topmost;
            ShowClock = Properties.Setting.Default.ShowClock;
            ShowCPU = Properties.Setting.Default.ShowCPU;
            ShowRAM = Properties.Setting.Default.ShowRAM;
            ShowUpload = Properties.Setting.Default.ShowUpload;
            ShowDownload = Properties.Setting.Default.ShowDownload;
            ColorName = Properties.Setting.Default.ColorName;
        }



        public void ChangeSetting()
        {
            mark_setting_changed = true;
        }



        public void SaveSetting()
        {
            if (!mark_setting_changed) return;

            // Assign the values to the settings properties
            Properties.Setting.Default.Topmost = Topmost;
            Properties.Setting.Default.ShowClock = ShowClock;
            Properties.Setting.Default.ShowCPU = ShowCPU;
            Properties.Setting.Default.ShowRAM = ShowRAM;
            Properties.Setting.Default.ShowUpload = ShowUpload;
            Properties.Setting.Default.ShowDownload = ShowDownload;
            Properties.Setting.Default.ColorName = ColorName;
            // Save the settings
            Properties.Setting.Default.Save();

            mark_setting_changed = false;


            //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var settings = config.AppSettings.Settings;

            //if(settings["Topmost"] == null)
            //    settings.Add("Topmost", Topmost.ToString());
            //else
            //    settings["Topmost"].Value = Topmost.ToString();

            //if (settings["ShowClock"] == null)
            //    settings.Add("ShowClock", showClock.ToString());
            //else
            //    settings["ShowClock"].Value = showClock.ToString();

            //if (settings["ShowCPU"] == null)
            //    settings.Add("ShowCPU", showCPU.ToString());
            //else
            //    settings["ShowCPU"].Value = showCPU.ToString();

            //if (settings["ShowRAM"] == null)
            //    settings.Add("ShowRAM", showRAM.ToString());
            //else
            //    settings["ShowRAM"].Value = showRAM.ToString();

            //if (settings["ShowUpload"] == null)
            //    settings.Add("ShowUpload", showUpload.ToString());
            //else
            //    settings["ShowUpload"].Value = showUpload.ToString();

            //if (settings["ShowDownload"] == null)
            //    settings.Add("ShowDownload", showDownload.ToString());
            //else
            //    settings["ShowDownload"].Value = showDownload.ToString();

            //config.Save(ConfigurationSaveMode.Modified);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T? value;

        public T? Value
        {
            get { return value; }
            set
            {
                if (!Equals(this.value, value))
                {
                    this.value = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableProperty()
        {

        }

        public ObservableProperty(T? value)
        {
            Value = value;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}