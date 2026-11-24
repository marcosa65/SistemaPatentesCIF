using System.Windows;
using SistemaPatentesCIF.models;

namespace SistemaPatentesCIF.views
{
    /// <summary>
    /// Lógica de interacción para VehicleHistoryDetailWindow.xaml
    /// </summary>
    public partial class VehicleHistoryDetailWindow : Window
    {

        public VehicleHistoryDetailWindow( VehicleInfoRecord vehicleInfo)
        {
            InitializeComponent();
            this.DataContext = vehicleInfo;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
