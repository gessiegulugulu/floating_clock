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

namespace floating_clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public DataModel data =  new DataModel();
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // 初始化计时器
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            layout.DataContext = data;

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                MessageBox.Show($"功能未实现： {menuItem.Header}");
            }
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                MessageBox.Show($"功能未实现： {menuItem.Header}");
            }
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_Help(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Floating Clock / v0.0.1");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            data.Update();

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: 记录窗口停靠位置
            var workingArea = SystemParameters.WorkArea;
            this.Left = workingArea.Right - this.Width;
            this.Top = (workingArea.Bottom/2) - this.Height;
        }
    }

    public static class Setting
    {
        // TODO: 
        public static Dictionary<string, string> visibility_state = new Dictionary<string, string>();
        
        public static void Load()
        {
            var _ = ConfigurationManager.AppSettings;
            visibility_state["clock"] = _["clock"] ?? "Collapsed";
            visibility_state["CPU"] = _["CPU"] ?? "Visible";
            visibility_state["RAM"] = _["RAM"] ?? "Visible";
            visibility_state["upload"] = _["upload"] ?? "Visible";
            visibility_state["download"] = _["download"] ?? "Visible";


        }
        public static void Save()
        {

        }
    }


    public class DataModel : INotifyPropertyChanged
    {

        SystemMonitor systemMonitor = new SystemMonitor();
        long last_up = 0;
        long last_down = 0;

        public string Time { get; set; } = "";
        public string CPU { get; set; } = "";
        public string RAM { get; set; } = "";
        public string Upload { get; set; } = "";
        public string Download { get; set; } = "";


        public DataModel()
        {
            Update();
        }

        public DataModel Update()
        {
            Time = DateTime.Now.ToString("HH : mm : ss");


            var cpu = systemMonitor.GetCpuUsage();
            var ram = systemMonitor.GetAvailableRam();
            long up = 0;
            long down = 0;

            // 计算网速
            var net = systemMonitor.GetNetworkStatistics();
            foreach (var item in net)
            {
                up += item.BytesSent;
                down += item.BytesReceived;
            }


            CPU= $"{cpu:F2}%";
            RAM = Util.ConvertBytesToReadableSize(ram);

            if (last_down + last_up > 0) // 第一次获取流量，跳过计算
            {
                Upload = Util.ConvertBytesToReadableSize((up - last_up) / 1000.0, "kmg") + "/s";
                Download = Util.ConvertBytesToReadableSize((down - last_down) / 1000.0, "kmg") + "/s";
            }

            last_up = up;
            last_down = down;



            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(CPU));
            OnPropertyChanged(nameof(RAM));
            OnPropertyChanged(nameof(Upload));
            OnPropertyChanged(nameof(Download));

            return this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}