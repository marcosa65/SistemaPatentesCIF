using GMap.NET;
using GMap.NET.MapProviders;
using SistemaPatentesCIF.models;
using SistemaPatentesCIF.services;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SistemaPatentesCIF
{
    /// <summary>
    /// Lógica de interacción para VehicleInfo.xaml
    /// </summary>
    public partial class VehicleInfoWindow : Window
    {
        public VehicleInfoModel? VehicleInfo { get; set; }
        public BitmapImage? vehicleImage;
        public string? PlateToLookup { get; set; }

        public VehicleInfoWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Establecer el proveedor de mapas (OpenStreetMap, GoogleMap, BingMap, etc.)
            GMapControl.MapProvider = GMapProviders.OpenStreetMap;

            // 2. Establecer el modo de acceso (ServerOnly, CacheOnly o ServerAndCache)
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            // 3. Configurar el UserAgent para cumplir con las políticas de uso de los proveedores de mapas
            GMapProvider.UserAgent = "SistemaPatentesCIF/1.0 (labinformaticacif@gmail.com)";
            

            Dispatcher.BeginInvoke(
                new Action(async () =>
                {
                    // Realizar la consulta de información del vehículo al servicio web
                    await LookupInfo();
                    // Asignar valores sin disparar eventos
                    AssignValuesToControls();
                }),
                DispatcherPriority.Input
            );
            

        }

        // Método para realizar la consulta de información del vehículo al servicio web
        private async Task LookupInfo()
        {
            string? json = await PlateLookupApi.LookupPlate(PlateToLookup ?? string.Empty);
            // Deserializar la respuesta JSON en un objeto ApiInfo
            ApiInfo apiInfo = JsonSerializer.Deserialize<ApiInfo>(json ?? string.Empty) ?? new ApiInfo();
            // asignar la información del vehículo a la propiedad vehicleInfo de la ventana VehicleInfoWindow
            VehicleInfo = apiInfo.Data ?? new VehicleInfoModel();
        }
        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static string TextFormatter(string texto)
        {
            texto = texto.Replace( "_", " ");
            texto = texto.Replace("-", " ");

            TextInfo ti =
                CultureInfo.CurrentCulture.TextInfo;

            return ti.ToTitleCase(texto.ToLower());
        }
        public static string PlateFormatter(string formato)
        {
            return formato switch
            {
                "automotor_mercosur" => "Automotor Mercosur",
                "automotor" => "Automotor",
                "motovehiculo" => "Motovehículo",
                "camion" => "Camión",
                _ => TextFormatter(formato)
            };
        }

        private void AssignValuesToControls()
        {
            if (VehicleInfo.HasValue)
            {
                var v = VehicleInfo.Value;
                TxtPlate.Text = v.Plate ?? "N/A";
                TxtMake.Text = TextFormatter(v.Make ?? "N/A");
                TxtModel.Text = TextFormatter(v.Model ?? "N/A");
                TxtAnio.Text = v.Year?.ToString() ?? "N/A";
                TxtPlateFormat.Text = PlateFormatter(v.PlateFormat ?? "N/A");
                TxtVehicleKind.Text = PlateFormatter(v.VehicleKind ?? "N/A");
                TxtProvince.Text = TextFormatter(v.CurrentLocation?.Province ?? "N/A");
                TxtCity.Text = TextFormatter(v.CurrentLocation?.City ?? "N/A");
                SetGmapByProvinceAndCity( 
                    v.CurrentLocation?.Province ?? "N/A", 
                    v.CurrentLocation?.City ?? "N/A",
                    "Argentina");
            }
            ImgVehicle.Source = vehicleImage;
        }

        private void SetGmapByProvinceAndCity(string pais, string provincia, string ciudad)
        {
            // Concatenamos los strings para darle un formato claro al buscador (ej: "Mendoza, Argentina")
            string direccionBusqueda = $"{ciudad}, {provincia}, {pais}";
            
            GeoCoderStatusCode status;
            if (GMapControl.MapProvider is GeocodingProvider gp)
            {
                // Aquí llamamos directamente al método del GeocodingProvider
                PointLatLng? pos = gp.GetPoint(direccionBusqueda, out status);
                Debug.Print($"Geocoding status: {status}, Position: {pos?.Lat}, {pos?.Lng}");
                if (status == GeoCoderStatusCode.OK && pos.HasValue)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 1. Definir los límites de zoom que se le permiten al control
                        GMapControl.MinZoom = 2;
                        GMapControl.MaxZoom = 20; // OSM soporta un nivel de detalle excelente hasta nivel 18-20

                        // 2. Establecer la posición y el nuevo nivel de acercamiento detallado
                        GMapControl.Position = pos.Value;
                        GMapControl.Zoom = 12; // Prueba valores entre 14 y 17 para máxima cercanía

                        // 3. Forzar el refresco visual del mapa en WPF
                        GMapControl.ReloadMap();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    MessageBox.Show($"No se pudo geocodificar la dirección. Estado: {status}");
                }
               
            }
        }
    }
}
