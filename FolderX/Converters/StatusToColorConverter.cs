using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderX.Converters
{
    /// <summary>
    /// 将状态字符串转换为颜色：
    ///   "已存在" / "已写入" → 绿色
    ///   其他              → 红色
    /// </summary>
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StatusToColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(0, 160, 80));
        private static readonly SolidColorBrush RedBrush   = new(Color.FromRgb(200, 0, 0));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string ?? string.Empty;
            return (status == "已存在" || status == "已写入") ? GreenBrush : RedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
