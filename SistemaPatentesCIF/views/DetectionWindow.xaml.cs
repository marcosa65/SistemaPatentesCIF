using Microsoft.Win32;
using SistemaPatentesCIF.AlprLab;
using SistemaPatentesCIF.models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaPatentesCIF.views
{
    /// <summary>
    /// Lógica de interacción para DetectionWindow.xaml
    /// </summary>
    public partial class DetectionWindow : Window
    {
        private ObservableCollection<PlateData> PlateDataCollection { get; set; } = new();
        public ALPRResult? DetectionResult { get; set; } = new();
        private BitmapImage? bmp;
        // Estructura de datos para bindear los resultados de la detección de placas en la tabla
        // Solo utilizable en el contexto de la UI, no es parte del modelo de datos principal.
        private struct PlateData
        {
            public string? Hora { get; set; }
            public string? Placa { get; set; }
            public string? Confianza { get; set; }
            public RectangleData? Rectangle { get; set; }
            public string? ColorHex { get; set; }
        }

        // Estructura de datos para representar la escala de la imagen y los rectángulos de detección
        private struct ScaleData
        {
            public double ScaleX { get; set; }
            public double ScaleY { get; set; }
        }

        private struct TransFormData 
        {
            public double ScaleX { get; set; }
            public double ScaleY { get; set; }
            public double OffsetX { get; set; }
            public double OffsetY { get; set; }
        }

        private struct RectangleData
        {
            public double x1 { get; set; }
            public double y1 { get; set; }
            public double x2 { get; set; }
            public double y2 { get; set; }
        }

        public DetectionWindow()
        {
            InitializeComponent();
            PlateDataGrid.ItemsSource = PlateDataCollection;
        }

        // Método para calcular la escala de la imagen y ajustar las coordenadas del rectángulo
        private TransFormData CalculateImageTransform()
        {
            double imgWidth = Convert.ToDouble(bmp?.PixelWidth);
            double imgHeight = Convert.ToDouble(bmp?.PixelHeight);

            double controlWidth = imgPreview.ActualWidth;
            double controlHeight = imgPreview.ActualHeight;

            double imageRatio = imgWidth / imgHeight;
            double controlRatio = controlWidth / controlHeight;

            double displayedWidth;
            double displayedHeight;

            double offsetX = 0;
            double offsetY = 0;

            if (imageRatio > controlRatio)
            {
                // Ajustada por ancho
                displayedWidth = controlWidth;
                displayedHeight = controlWidth / imageRatio;
                offsetY =
                    (controlHeight - displayedHeight) / 2;
            }
            else
            {
                // Ajustada por alto
                displayedHeight = controlHeight;
                displayedWidth = controlHeight * imageRatio;
                offsetX =
                    (controlWidth - displayedWidth) / 2;
            }

            double scaleX = displayedWidth / imgWidth;
            double scaleY = displayedHeight / imgHeight;

            return new TransFormData
            { 
                ScaleX = scaleX, 
                ScaleY = scaleY, 
                OffsetX = offsetX, 
                OffsetY = offsetY 
            };
               
        }

        // Método para dibujar un rectángulo con texto alrededor de la placa detectada
        private void DrawRectangle( double x1, double y1, double x2, double y2, string plateText = "")
        {
            // Calcular la escala de la imagen para ajustar las coordenadas del rectángulo
            TransFormData transform = CalculateImageTransform();

            // Escalar las coordenadas del rectángulo según la escala calculada
            double scaledX1 = (x1 * transform.ScaleX) + transform.OffsetX;
            double scaledY1 = (y1 * transform.ScaleY) + transform.OffsetY;
            double width = (x2 - x1) * transform.ScaleX;
            double height = (y2 - y1) * transform.ScaleY;

            // Crear un TextBlock para mostrar el texto de la placa
            TextBlock TxtBlock = new TextBlock
            {
                Text = plateText,
                Background = Brushes.LimeGreen,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(4),
            };

            // Crear un rectángulo para resaltar la placa
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Magenta,
                StrokeThickness = 3,
            };

            // Posicionar el rectángulo y el TextBlock en el Canvas
            Canvas.SetLeft(rect, scaledX1);
            Canvas.SetTop(rect, scaledY1);
            Canvas.SetLeft(TxtBlock, scaledX1);
            Canvas.SetTop(TxtBlock, scaledY1 - 48);

            overlayCanvas.Height = imgPreview.ActualHeight;
            overlayCanvas.Width = imgPreview.ActualWidth;
            // Agregar el rectángulo y el TextBlock al Canvas
            overlayCanvas.Children.Add(rect);
            overlayCanvas.Children.Add(TxtBlock);
        }

        // Método para determinar el color basado en la confianza
        private static Color GetColorHex(double confidence)
        {
            if (confidence > 0.90)
            {
                return Colors.Green; // Verde
            }
            else if (confidence > 0.70)
            {
                return Colors.Yellow; // Amarillo
            }
            else
            {
                return Colors.Red; // Rojo
            }
        }

        private async void LoadImageButton_Click(object sender, RoutedEventArgs e) 
        {
            Button? btn = (Button)sender;
            btn?.IsEnabled = false; // Deshabilita el botón mientras se procesa la imagen
            OpenFileDialog selectFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.png;*.jpeg",
            };

            if (selectFileDialog.ShowDialog() == true)
            {
                string imagePath = selectFileDialog.FileName;
                if (!File.Exists(imagePath))
                {
                    MessageBox.Show(
                        "Error con la seleccion de imagen.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    btn?.IsEnabled = true; // Habilita el botón si ocurre un error
                    return;
                }
                bmp = new BitmapImage(new Uri(imagePath));
                overlayCanvas.Children.Clear();
                UpdateProgressbar(10, "Cargando imagen...");
                // Pausa el procesamiento por 1 segundo (1000 milisegundos)
                await Task.Delay(1000);
                imgPreview.Source = bmp;
                UpdateProgressbar(30, "Procesando imagen...");
                await Task.Delay(500);
                DetectionResult = AlprEngine.StartEngine(selectFileDialog.FileName);
                if (DetectionResult?.Plates?.Count > 0)
                {
                    foreach (var plate in DetectionResult.Plates)
                    {
                        Color ColorHexVar = GetColorHex(plate.Confidence);
                        PlateDataCollection.Add(
                            new PlateData
                            {
                                Hora = DateTime.Now.ToString("HH:mm:ss-dd/MM/yyyy"),
                                Placa = plate.Text,
                                Confianza = $"{plate.Confidence:P0}",
                                ColorHex = ColorHexVar.ToString(),
                                Rectangle = new RectangleData
                                {
                                    x1 = plate.x1,
                                    y1 = plate.y1,
                                    x2 = plate.x2,
                                    y2 = plate.y2,
                                },
                            }
                        );
                        TxtDetectedPlate.Text = plate.Text;
                        TxtConfidencePlate.Text = $"Confianza: " +
                            $"{plate.Confidence:p0}";
                        TxtConfidencePlate.Foreground = new SolidColorBrush(ColorHexVar);
                        PaisDetectado.Text = $"País: {plate.Region}";
                        ConfianzaPais.Text = $"Confianza País: " +
                            $"{plate.RegionConfidence:P0}";
                        string rectangleText = $"{plate.Text} - " +
                            $"{(plate.Confidence * 100):0}%\nReg:" +
                            $"{plate.Region}" ?? "No detectada";
                        // 
                        DrawRectangle( plate.x1, plate.y1, plate.x2, plate.y2, rectangleText );
                    }
                    UpdateProgressbar(70, "Ejecutando  OCR...");
                    await Task.Delay(500);
                }
                else
                {
                    MessageBox.Show(
                        "No fue posible detectar placas \nen la imagen aportada.",
                        "Atencion!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                UpdateProgressbar(100, "Proceso finalizado...");
                // Habilita el botón después de procesar la imagen
                await Task.Delay(1000); // Pausa el procesamiento por 1 segundo (1000 milisegundos)
                UpdateProgressbar(0, "Esperando accion...");
            }
            else
            {
                MessageBox.Show(
                    "No se seleccionó ninguna imagen.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            btn?.IsEnabled = true; // Habilita el botón después de procesar la imagen
        }

        private void UpdateProgressbar(double value, string message)
        {
            OcrProgress.Value = value;
            StatusBarInfo.Text = message;
        }

        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        private void CleanButton_Click( object sender, RoutedEventArgs e) 
        {
            // Vacía el origen de la imagen
            imgPreview.Source = null;
            // Fuerza al motor de WPF a redibujar el control inmediatamente
            imgPreview.InvalidateVisual();
            TxtDetectedPlate.Text = "-";
            TxtConfidencePlate.Text = "Confianza: -";
            TxtConfidencePlate.Foreground = new SolidColorBrush(Colors.White);
            PaisDetectado.Text = "País: -";
            ConfianzaPais.Text = "Confianza País: -";
            PlateDataCollection.Clear();
            overlayCanvas.Children.Clear();
            
        }

        private void PlateDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (PlateDataGrid.SelectedItem is PlateData selectedPlate)
            {
                TxtDetectedPlate.Text = selectedPlate.Placa;
                TxtConfidencePlate.Text = $"Confianza: {selectedPlate.Confianza}";
                TxtConfidencePlate.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(selectedPlate.ColorHex ?? "#FFFFFF")
                );
                PaisDetectado.Text =
                    $"País: {DetectionResult?.Plates?.FirstOrDefault(p => p.Text == selectedPlate.Placa)?.Region ?? "Argentina"}";
                ConfianzaPais.Text =
                    $"Confianza País: {DetectionResult?.Plates?.FirstOrDefault(p => p.Text == selectedPlate.Placa)?.RegionConfidence.ToString("P0") ?? "97"}";
            }
        }

        private async void GetVehicleInfo_Click(object sender, RoutedEventArgs e)
        {
            if (PlateDataGrid.SelectedItem is PlateData selectedPlate)
            {
                // Crear una instancia de la ventana VehicleInfoWindow
                VehicleInfoWindow? vehicleInfoWindow = new VehicleInfoWindow();
                // Asignar la Placa detectada 
                vehicleInfoWindow?.PlateToLookup = selectedPlate.Placa ?? string.Empty;
                // Asignar la imagen cargada a la propiedad vehicleImage de la ventana VehicleInfoWindow
                vehicleInfoWindow?.vehicleImage = bmp;
                // Mostrar la ventana VehicleInfoWindow como un diálogo modal
                vehicleInfoWindow?.ShowDialog();
            }
            else
            {
                MessageBox.Show(
                    "Seleccione un numero de patente de la tabla\n"
                        + "para obtener información del vehículo.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
