using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SistemaPatentesCIF.models;
using SistemaPatentesCIF.services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SistemaPatentesCIF
{
    public partial class VehicleInfoWindow : Window
    {
        public VehicleInfoModel? VehicleInfo { get; set; }
        public BitmapImage? vehicleImage;
        public string? PlateToLookup { get; set; }

        private MemoryLayer? _markerLayer;

        private const string UserAgent = "SistemaPatentesCIF_Formosa/1.0";
        
        

        private static readonly HttpClient _httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", UserAgent }
            }
        };

        private static readonly Dictionary<string, (double Lat, double Lon)> _cacheCoordenadas =
            new Dictionary<string, (double Lat, double Lon)>(StringComparer.OrdinalIgnoreCase);

        public VehicleInfoWindow()
        {
            InitializeComponent();
            InitializeMap();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlateToLookup))
                return;

            Dispatcher.BeginInvoke(
                new Action(async () =>
                {
                    await LookupInfo();
                }),
                DispatcherPriority.Input
            );
        }

        private void InitializeMap()
        {
            if (MapControl.Map == null) return;

            LoggingWidget.ShowLoggingInMap = ActiveMode.No;
            MapControl.Map.Widgets.Clear();
            MapControl.Map.Layers.Clear();

            string host = "server" + "." + "arcgisonline" + "." + "com";

            // 1. Capa base oscura (calles y relieve)
            string basePath = "/ArcGIS/rest/services/Canvas/World_Dark_Gray_Base/MapServer/tile/{z}/{y}/{x}";
            var darkTileSource = new HttpTileSource(
                new GlobalSphericalMercator(),
                "https://" + host + basePath,
                name: "Esri Dark Gray",
                attribution: new BruTile.Attribution("© Esri, OpenStreetMap contributors"),
                configureHttpRequestMessage: (request) =>
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", "SistemaPatentesCIF_Formosa/1.0");
                }
            );
            MapControl.Map.Layers.Add(new TileLayer(darkTileSource));

            // 2. Capa transparente superior con nombres de ciudades, barrios y rutas
            string refPath = "/ArcGIS/rest/services/Canvas/World_Dark_Gray_Reference/MapServer/tile/{z}/{y}/{x}";
            var refTileSource = new HttpTileSource(
                new GlobalSphericalMercator(),
                "https://" + host + refPath,
                name: "Esri Dark Gray Labels",
                configureHttpRequestMessage: (request) =>
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", "SistemaPatentesCIF_Formosa/1.0");
                }
            );
            MapControl.Map.Layers.Add(new TileLayer(refTileSource));

            // 3. Fondo oscuro mientras cargan los mosaicos
            MapControl.Map.BackColor = Color.FromString("#0C0D12");

            // 4. Capa para el pin de radicación en color ámbar
            _markerLayer = new MemoryLayer
            {
                Name = "RadicacionLayer",
                Style = new SymbolStyle
                {
                    SymbolScale = 0.9,
                    Fill = new Brush(Color.FromString("#F59E0B")),
                    Outline = new Pen(Color.White, 2)
                }
            };
            MapControl.Map.Layers.Add(_markerLayer);

            // 5. Vista inicial centrada en Formosa Capital
            MoveMapToCoordinates(-26.1849, -58.1731, zoom: 12, showPin: false);
        }

        private void MoveMapToCoordinates(double lat, double lon, int zoom, bool showPin)
        {
            if (MapControl.Map == null || _markerLayer == null) return;

            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            var point = new MPoint(x, y);

            _markerLayer.Features = showPin
                ? new List<IFeature> { new PointFeature(point) }
                : new List<IFeature>();

            _markerLayer.DataHasChanged();

            double resolution = 156543.03392 / Math.Pow(2, zoom);
            MapControl.Map.Navigator.CenterOn(point);
            MapControl.Map.Navigator.ZoomTo(resolution);
        }

        private async Task LookupInfo()
        {
            string? json = await PlateLookupApi.LookupPlate(PlateToLookup ?? string.Empty);

            if (json == null)
            {
                MessageBox.Show(
                    "No se pudo obtener información del vehículo. Por favor, verifique la patente e intente nuevamente.",
                    "Error de Consulta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            ApiInfo apiInfo = JsonSerializer.Deserialize<ApiInfo>(json) ?? new ApiInfo();
            VehicleInfo = apiInfo.Data ?? new VehicleInfoModel();
            await AssignValuesToControlsAsync();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static string TextFormatter(string texto)
        {
            texto = texto.Replace("_", " ");
            texto = texto.Replace("-", " ");

            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
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
                _ => TextFormatter(formato),
            };
        }

        private async Task AssignValuesToControlsAsync()
        {
            if (VehicleInfo.HasValue)
            {
                var v = VehicleInfo.Value;
                string provinciaFormateada = TextFormatter(v.CurrentLocation?.Province ?? "N/A");
                string ciudadFormateada = TextFormatter(v.CurrentLocation?.City ?? "N/A");

                TxtPlate.Text = v.Plate ?? "N/A";
                TxtMake.Text = TextFormatter(v.Make ?? "N/A");
                TxtModel.Text = TextFormatter(v.Model ?? "N/A");
                TxtAnio.Text = v.Year?.ToString() ?? "N/A";
                TxtPlateFormat.Text = PlateFormatter(v.PlateFormat ?? "N/A");
                TxtVehicleKind.Text = PlateFormatter(v.VehicleKind ?? "N/A");
                TxtProvince.Text = provinciaFormateada;
                TxtCity.Text = ciudadFormateada;

                await SetMapByProvinceAndCityAsync("Argentina", provinciaFormateada, ciudadFormateada);
            }
        }

        private void LookupButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearchPlate.Text))
            {
                MessageBox.Show(
                    "Por favor, ingrese una patente válida.",
                    "Error de Consulta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            PlateToLookup = TxtSearchPlate.Text.Trim();
            Dispatcher.BeginInvoke(
                new Action(async () =>
                {
                    await LookupInfo();
                }),
                DispatcherPriority.Input
            );
        }

        private async Task SetMapByProvinceAndCityAsync(string pais, string provincia, string ciudad)
        {
            if (provincia == "N/A" && ciudad == "N/A")
            {
                MoveMapToCoordinates(-26.1849, -58.1731, zoom: 12, showPin: false);
                return;
            }

            string ciudadLimpia = ciudad == "N/A" ? string.Empty : ciudad;
            string provinciaLimpia = provincia == "N/A" ? string.Empty : provincia;

            var coords = await GeocodeLocationAsync(pais, provinciaLimpia, ciudadLimpia);

            if (coords.HasValue)
            {
                MoveMapToCoordinates(coords.Value.Lat, coords.Value.Lon, 14, showPin: true);
            }
            else
            {
                Debug.Print($"No se pudo geocodificar: {ciudadLimpia}, {provinciaLimpia}, {pais}");
                MoveMapToCoordinates(-26.1849, -58.1731, zoom: 12, showPin: false);
            }
        }

        private async Task<(double Lat, double Lon)?> GeocodeLocationAsync(string pais, string provincia, string ciudad)
        {
            string cacheKey = $"{ciudad}|{provincia}|{pais}";
            if (_cacheCoordenadas.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                string queryText = string.IsNullOrWhiteSpace(ciudad)
                    ? $"{provincia}, {pais}"
                    : $"{ciudad}, {provincia}, {pais}";

                string geoHost = "nominatim" + "." + "openstreetmap" + "." + "org";
                string url = $"https://{geoHost}/search?q={Uri.EscapeDataString(queryText)}&format=json&limit=1";
                string json = await _httpClient.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    if (double.TryParse(first.GetProperty("lat").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(first.GetProperty("lon").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                    {
                        _cacheCoordenadas[cacheKey] = (lat, lon);
                        return (lat, lon);
                    }
                }

                // Si no encontró la ciudad exacta, intenta ubicar al menos la provincia
                if (!string.IsNullOrWhiteSpace(ciudad) && !string.IsNullOrWhiteSpace(provincia))
                {
                    return await GeocodeLocationAsync(pais, provincia, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error geocodificando ubicación: {ex.Message}");
            }

            return null;
        }

    }
}