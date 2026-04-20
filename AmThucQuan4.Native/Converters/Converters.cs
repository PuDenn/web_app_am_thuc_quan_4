using System.Globalization;

namespace AmThucQuan4.Native.Converters;

/// <summary>null → false, non-null → true</summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is not null;
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

/// <summary>true → "⏹ Dừng", false → "▶ Phát Audio" (localized via parameter)</summary>
public class BoolToAudioTextConverter : IValueConverter
{
    // parameter = "play|stop" strings từ ViewModel
    public object Convert(object? value, Type t, object? p, CultureInfo c)
    {
        // Nếu có parameter dạng "play_text|stop_text" thì dùng
        if (p is string param && param.Contains('|'))
        {
            var parts = param.Split('|');
            return value is true ? parts[1] : parts[0];
        }
        return value is true ? "⏹ Dừng" : "▶ Phát Audio";
    }
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

/// <summary>true → false, false → true</summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is bool b && !b;
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) =>
        value is bool b && !b;
}
