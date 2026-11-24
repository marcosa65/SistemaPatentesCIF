using System.Windows;
using System.Windows.Controls;
using SistemaPatentesCIF.data;
using SistemaPatentesCIF.models;

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

            // Cargar los registros de detección desde la base de datos local
            recordList = DataBaseManager.GetAllDetections();

            // Para cada registro, obtenemos la información del vehículo desde la base de datos local
            // Esto asegura que cada registro tenga su información vehicular asociada si está disponible
            foreach (DetectionRecord record in recordList)
            {
                VehicleInfoRecord? info = DataBaseManager.GetVehicleInfo(record.Plate);
                record.VehicleInfo = info;
            }

            //InjectMockVehicleDetails(recordList); // Inyecta detalles de vehículos simulados

            // Actualizar la fuente de datos del DataGrid con los registros obtenidos
            UpdateDataGridSource(recordList);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateDataGridSource(List<DetectionRecord> detections)
        {
            dgDetections.ItemsSource = detections;
            TxtTotalRecords.Text = $"Total de registros: {detections.Count}";
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
            // Obtiene el botón que disparó el clic y extrae el objeto de esa fila (DetectionRecord)
            if (sender is Button btn && btn.DataContext is DetectionRecord selectedRecord)
            {
                // Verifica que el registro realmente tenga datos vehiculares cargados
                if (selectedRecord.HasVehicleInfo)
                {
                    // Se instancia de Informacion vehicular pasando el VehicleInfoRecord por el constructor
                    var detailWindow = new 
                        VehicleHistoryDetailWindow
                        (
                            selectedRecord.VehicleInfo ?? new VehicleInfoRecord()
                        );
                        {
                            detailWindow.Owner = this; // Centra la ventana emergente sobre HistoryWindow
                        }


                    // 4. La muestras como ventana modal (bloquea la de atrás hasta que se cierre)
                    detailWindow.ShowDialog();
                }
            }
        }

        // Método para inyectar detalles de vehículos simulados en registros aleatorios
        // Solo para fines de prueba y desarrollo, debe comentarse en producción
        private void InjectMockVehicleDetails(List<DetectionRecord> recordList)
        {
            if (recordList == null || recordList.Count == 0)
                return;

            var mockVehicles = new List<VehicleInfoRecord>
            {
                new VehicleInfoRecord
                {
                    Make = "Toyota",
                    Model = "Hilux 4x4 SRX",
                    Year = 2022,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Pick-Up",
                    Province = "Formosa",
                    City = "Formosa Capital",
                },
                new VehicleInfoRecord
                {
                    Make = "Chevrolet",
                    Model = "Onix LTZ",
                    Year = 2020,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Hatchback",
                    Province = "Chaco",
                    City = "Resistencia",
                },
                new VehicleInfoRecord
                {
                    Make = "Renault",
                    Model = "Duster Oroch",
                    Year = 2019,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Pick-Up",
                    Province = "Corrientes",
                    City = "Corrientes Capital",
                },
                new VehicleInfoRecord
                {
                    Make = "Ford",
                    Model = "Focus III Titanium",
                    Year = 2018,
                    PlateFormat = "Mercosur (AA000AA)",
                    VehicleKind = "Sedán 5 Puertas",
                    Province = "Formosa",
                    City = "Clorinda",
                },
            };

            var random = new Random();
            int itemsToPopulate = Math.Min(4, recordList.Count);

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
