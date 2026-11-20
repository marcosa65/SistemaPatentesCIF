using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SistemaPatentesCIF.themes
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName)) return;

            string fileName = themeName.EndsWith("Theme", StringComparison.OrdinalIgnoreCase)
                ? themeName
                : $"{themeName}Theme";

            var themeUri = new Uri($"/SistemaPatentesCIF;component/themes/{fileName}.xaml", UriKind.Relative);
            var newThemeDict = new ResourceDictionary { Source = themeUri };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var existingTheme = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase));

            if (existingTheme != null)
            {
                dictionaries.Remove(existingTheme);
            }

            dictionaries.Insert(0, newThemeDict);
        }

    }

}
