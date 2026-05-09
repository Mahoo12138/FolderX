using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderX.Converters
{
    /// <summary>
    /// ColumnSetting.IsVisible → 行前景色（隐藏列显示为灰色）
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToGrayConverter : IValueConverter
    {
        private static readonly SolidColorBrush BlackBrush = new(Colors.Black);
        private static readonly SolidColorBrush GrayBrush  = new(Color.FromRgb(160, 160, 160));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? BlackBrush : GrayBrush;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
