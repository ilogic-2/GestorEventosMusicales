using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GestorEventosMusicales.Converters
{
    public class ManagerIdToVisibilityConverter : IValueConverter
    {
        public int ActualManagerId { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int id)
            {
                return id != ActualManagerId;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
