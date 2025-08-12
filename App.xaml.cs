using System.Configuration;
using System.Data;
using System.Windows;
using WpfApp = System.Windows.Application;

namespace floating_clock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WpfApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 全局异常处理
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 检查命令行参数
            bool startMinimized = e.Args.Contains("--minimized") || e.Args.Contains("-m");
            
            base.OnStartup(e);
            
            // 如果指定了最小化启动，则在MainWindow加载后处理
            if (startMinimized)
            {
                // 将启动参数保存到应用程序属性中，供MainWindow使用
                Current.Properties["StartMinimized"] = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowException(e.ExceptionObject as Exception, "Unhandled Exception");
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowException(e.Exception, "Dispatcher Unhandled Exception");
            e.Handled = true;
        }

        private void ShowException(Exception? ex, string title)
        {
            if (ex != null)
            {
                System.Windows.MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
