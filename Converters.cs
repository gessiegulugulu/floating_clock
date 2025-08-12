// 严重性 代码	说明	项目	文件	行	禁止显示状态	详细信息
// 错误	XDG0008	命名空间“clr-namespace:floating_clock”中不存在“StringToBooleanConverter”名称。	floating_clock C:\Users\kebu\source\repos\floating_clock\App.xaml	8		


using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace floating_clock
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value != null && value.Equals(parameter);
            Debug.WriteLine($"Convert: value={value}, parameter={parameter}, result={result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = value.Equals(true) ? parameter : System.Windows.Data.Binding.DoNothing;
            Debug.WriteLine($"ConvertBack: value={value}, parameter={parameter}, result={result}");
            return result;
        }
    }


    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // 直接比较字符串值
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果绑定值是 true，则返回参数值
            if (value.Equals(true))
            {
                return parameter.ToString();
            }

            return System.Windows.Data.Binding.DoNothing;
        }
    }

    public class ColorNameToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return System.Windows.Application.Current.FindResource(stringValue.ToLower());
            }
            return System.Windows.Application.Current.FindResource("gray");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorNameToOpaqueColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return System.Windows.Application.Current.FindResource("opaque_" + stringValue.ToLower());
            }
            return System.Windows.Application.Current.FindResource("opaque_gray");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

