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

namespace floating_clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// color palette
    public partial class MainWindow : Window
    {
        public DataModel data = new DataModel();
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // 初始化计时器
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            this.DataContext = data;

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

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                MessageBox.Show($"功能未实现： {menuItem.Header}  ");
            }
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                MessageBox.Show($"功能未实现： {menuItem.Header}  ");
            }
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            data.SaveSetting();
            //data.SaveSetting();
            Close();
        }

        private void MenuItem_Click_Help(object sender, RoutedEventArgs e)
        {
            var text = "Floating Clock / v0.0.1\n";
            // text += ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            text += $"data.Color : {data.ColorName}\n";
            text += "配置文件保存在\n";
            text += ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            MessageBox.Show(text, "帮助");
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
       private string _color_name = "gray";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T value;

        public T Value
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

        public ObservableProperty(T value)
        {
            Value = value;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ColorOptions
    {
        red,
        blue,
        green,
    }
}