using System.Windows;
using SistemaPatentesCIF.data;
using SistemaPatentesCIF.themes;
using SistemaPatentesCIF.views;

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

        private void Window_ContentRendered(object? sender, EventArgs e)
        {
            // Código que se ejecuta cuando la ventana se ha renderizado completamente
            LastDetectionTime.Text =
                DataBaseManager.GetLastDetection()?.DetectionDate.ToString("g")
                ?? "No hay detecciones";
            
        }

        private void Window_Loaded(object? sender, EventArgs e)
        {
            // Codigo que se ejecuta cuando la ventana se carga, previamente a que se renderice completamente
            // Todos los controles (TextBox, DataGrid, etc.) ya están cargados
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
