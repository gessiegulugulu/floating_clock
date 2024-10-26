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

namespace floating_clock
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            // 剧中显示
            var workingArea = SystemParameters.WorkArea;
            this.Left = workingArea.Right/2 - this.Width/2;
            this.Top = workingArea.Bottom/2 - this.Height/2;
        }

        /// <summary>
        /// 以指定父窗口创建窗口，使窗口能被父窗口一起关闭
        /// </summary>
        /// <param name="owner">父窗口的引用</param>
        public HelpWindow(Window owner): this() 
        {
            Owner = owner;
        }

    }
}
