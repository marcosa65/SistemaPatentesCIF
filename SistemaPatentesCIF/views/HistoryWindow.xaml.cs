using SistemaPatentesCIF.data;
using SistemaPatentesCIF.models;
using System.Windows;
using System.Windows.Controls;

namespace SistemaPatentesCIF.views
{
    /// <summary>
    /// Lógica de interacción para HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private readonly List<DetectionRecord> recordList;

        public HistoryWindow()
        {
            InitializeComponent();
            recordList = DataBaseManager.GetAllDetections();
            InjectMockVehicleDetails(recordList); // Inyecta detalles de vehículos simulados
            UpdateDataGridSource(recordList);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateDataGridSource(List<DetectionRecord> detections)
        {
            dgDetections.ItemsSource = detections;
        }

        private void TxtFilterPlate_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e
        )
        {
            List<DetectionRecord> filteredRecords;

            // Limpiamos espacios en blanco del término ingresado
            string cleanedString = TxtFilterPlate.Text.Trim();

            // Filtramos ignorando mayúsculas y minúsculas
            filteredRecords = recordList
                .Where(record =>
                    !string.IsNullOrEmpty(record.Plate)
                    && record.Plate.Contains(cleanedString, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            UpdateDataGridSource(filteredRecords);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtFilterPlate.Text = string.Empty;
        }

        private void BtnViewDetail_Click(object sender, RoutedEventArgs e) 
        {
            // 1. Obtienes el botón que disparó el clic y extraes el objeto de esa fila (DetectionRecord)
            if (sender is Button btn && btn.DataContext is DetectionRecord selectedRecord)
            {
                // 2. Verificas que el registro realmente tenga datos vehiculares cargados
                if (selectedRecord.VehicleInfo.HasValue)
                {
                    // 3. Instancias la ventana pasándole el struct VehicleInfoModel por el constructor
                    var detailWindow = new VehicleHistoryDetailWindow(selectedRecord.VehicleInfo.Value)
                    {
                        Owner = this // Centra la ventana emergente sobre HistoryWindow
                    };

                    // 4. La muestras como ventana modal (bloquea la de atrás hasta que se cierre)
                    detailWindow.ShowDialog();
                }
            }
        }

        private void InjectMockVehicleDetails(List<DetectionRecord> recordList)
        {
            if (recordList == null || recordList.Count == 0)
                return;

            var mockVehicles = new List<VehicleInfoModel>
            {
                new VehicleInfoModel
                {
                    Make = "Toyota",
                    Model = "Hilux 4x4 SRX",
                    Year = 2022,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Pick-Up",
                    CurrentLocation = new VehicleLocationInfoModel
                    {
                        Province = "Formosa",
                        City = "Formosa Capital",
                    },
                },
                new VehicleInfoModel
                {
                    Make = "Volkswagen",
                    Model = "Amarok V6 Extreme",
                    Year = 2021,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Pick-Up",
                    CurrentLocation = new VehicleLocationInfoModel
                    {
                        Province = "Chaco",
                        City = "Resistencia",
                    },
                },
                new VehicleInfoModel
                {
                    Make = "Ford",
                    Model = "Focus III Titanium",
                    Year = 2018,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Sedán 5 Puertas",
                    CurrentLocation = new VehicleLocationInfoModel
                    {
                        Province = "Formosa",
                        City = "Clorinda",
                    },
                },
            };

            var random = new Random();
            int itemsToPopulate = Math.Min(3, recordList.Count);

            // Selecciona índices aleatorios sin repetir
            var selectedIndices = Enumerable
                .Range(0, recordList.Count)
                .OrderBy(_ => random.Next())
                .Take(itemsToPopulate);

            int mockIndex = 0;
            foreach (int idx in selectedIndices)
            {
                var mock = mockVehicles[mockIndex % mockVehicles.Count];
                mock.Plate = recordList[idx].Plate; // Sincroniza la patente real de la fila
                recordList[idx].VehicleInfo = mock;
                mockIndex++;
            }
        }
    }
}
