using SistemaPatentesCIF.data;
using SistemaPatentesCIF.services;
using SistemaPatentesCIF.views;
using System.Windows;
using System.Windows.Media;

namespace SistemaPatentesCIF
{
    /// <summary>
    /// Lógica de interacción para InitialWindow.xaml
    /// </summary>
    public partial class InitialWindow : Window
    {
        public InitialWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            this.ContentRendered += Window_ContentRendered;
        }

        private async void Window_Loaded(object? sender, EventArgs e)
        {
            // Codigo que se ejecuta cuando la ventana se carga, previamente a que se renderice completamente
            // Todos los controles (TextBox, DataGrid, etc.) ya están cargados
            GetAPIStatus();

        }

        private void Window_ContentRendered(object? sender, EventArgs e)
        {
            // Código que se ejecuta cuando la ventana se ha renderizado completamente
            LastDetectionTime.Text =
                DataBaseManager.GetLastDetection()?.DetectionDate.ToString("g")
                ?? "No hay detecciones";
        }

        private async void GetAPIStatus()
        {
            ApiDomainStatus.Text = "● Comprobando..";
            ApiDomainStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107")); // Color amarillo
            await Task.Delay(1000); // Simular retraso en la comprobación
            bool online = await PlateLookupApi.IsSiteAvailable();
            if (online)
            {
                ApiDomainStatus.Text = "● Disponible";
                ApiDomainStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // Color verde
            }
            else
            {
                ApiDomainStatus.Text = "● No disponible";
                ApiDomainStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // Color rojo
            }
        }



        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow historyWindow = new HistoryWindow();
            historyWindow.Show();
        }

        private void DetectionButton_Click(object sender, RoutedEventArgs e)
        {
            DetectionWindow DetectionWindow = new DetectionWindow();
            DetectionWindow.Show();
        }

        private void IntelligenceButton_Click(object sender, RoutedEventArgs e)
        {
            IntelligenceWindow intelligenceWindow = new IntelligenceWindow();
            intelligenceWindow.Show();
        }

        private void VehicleInfo_Click(object sender, RoutedEventArgs e)
        {
            VehicleInfoWindow vehicleInfoWindow = new VehicleInfoWindow();
            vehicleInfoWindow.Show();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }
    }
}
