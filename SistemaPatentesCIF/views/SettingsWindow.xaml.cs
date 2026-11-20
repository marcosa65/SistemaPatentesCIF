using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SistemaPatentesCIF.themes;

namespace SistemaPatentesCIF.views
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string themeName)
            {
                // Aplica el tema seleccionado
                ThemeManager.ApplyTheme(themeName);
                // Actualiza y guarda el tema inmediatamente en el JSON
                SettingsManager.Current.ThemeName = themeName;
                SettingsManager.Save(SettingsManager.Current);
            }
        }
    }
}
