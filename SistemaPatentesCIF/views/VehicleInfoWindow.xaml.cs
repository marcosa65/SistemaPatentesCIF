using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
using SistemaPatentesCIF.data;
using SistemaPatentesCIF.models;
using SistemaPatentesCIF.services;
using SistemaPatentesCIF.views;

namespace SistemaPatentesCIF
{
    public partial class VehicleInfoWindow : Window
    {
        public VehicleInfoRecord? VehicleInfo { get; set; }
        public BitmapImage? vehicleImage;
        public string? PlateToLookup { get; set; }

        private MemoryLayer? _markerLayer;

        private const string UserAgent = "SistemaPatentesCIF_Formosa/1.0";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", UserAgent } },
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

            VehicleInfoRecord? info = DataBaseManager.GetVehicleInfo(PlateToLookup);

            if (info != null)
            {
                MessageBoxResult result = CustomMessageBox.Show(
                    $"Se encontró información en la base de datos para la patente {PlateToLookup} \n"
                        + $"desea actualizarla?",
                    "Información Encontrada",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information
                );
                if (result == MessageBoxResult.Cancel)
                {
                    // El usuario eligió no actualizar, asignar la información existente
                    VehicleInfo = info;
                    Dispatcher.BeginInvoke(
                        new Action(async () =>
                        {
                            await AssignValuesToControlsAsync();
                        }),
                        DispatcherPriority.Input
                    );
                    return;
                }
            }
            // Si no se encontró información o el usuario eligió actualizar, realizar la consulta a la API
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
            if (MapControl.Map == null)
                return;

            LoggingWidget.ShowLoggingInMap = ActiveMode.No;
            MapControl.Map.Widgets.Clear();
            MapControl.Map.Layers.Clear();

            string host = "server" + "." + "arcgisonline" + "." + "com";

            // 1. Capa base oscura (calles y relieve)
            string basePath =
                "/ArcGIS/rest/services/Canvas/World_Dark_Gray_Base/MapServer/tile/{z}/{y}/{x}";
            var darkTileSource = new HttpTileSource(
                new GlobalSphericalMercator(),
                "https://" + host + basePath,
                name: "Esri Dark Gray",
                attribution: new BruTile.Attribution("© Esri, OpenStreetMap contributors"),
                configureHttpRequestMessage: (request) =>
                {
                    request.Headers.TryAddWithoutValidation(
                        "User-Agent",
                        "SistemaPatentesCIF_Formosa/1.0"
                    );
                }
            );
            MapControl.Map.Layers.Add(new TileLayer(darkTileSource));

            // 2. Capa transparente superior con nombres de ciudades, barrios y rutas
            string refPath =
                "/ArcGIS/rest/services/Canvas/World_Dark_Gray_Reference/MapServer/tile/{z}/{y}/{x}";
            var refTileSource = new HttpTileSource(
                new GlobalSphericalMercator(),
                "https://" + host + refPath,
                name: "Esri Dark Gray Labels",
                configureHttpRequestMessage: (request) =>
                {
                    request.Headers.TryAddWithoutValidation(
                        "User-Agent",
                        "SistemaPatentesCIF_Formosa/1.0"
                    );
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
                    Outline = new Pen(Color.White, 2),
                },
            };
            MapControl.Map.Layers.Add(_markerLayer);

            // 5. Vista inicial centrada en Formosa Capital
            MoveMapToCoordinates(-26.1849, -58.1731, zoom: 12, showPin: false);
        }

        private void MoveMapToCoordinates(double lat, double lon, int zoom, bool showPin)
        {
            if (MapControl.Map == null || _markerLayer == null)
                return;

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

            // No se obtuvo respuesta de la API
            if (json == null)
            {
                CustomMessageBox.Show(
                    "No se pudo obtener respuesta de la API.",
                    "Error de Consulta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            ApiInfo apiInfo = JsonSerializer.Deserialize<ApiInfo>(json) ?? new ApiInfo();

            // Hay una respuesta de la API
            if (apiInfo.Success)
            {
                // Hubo respuesta exitosa de la API, pero no se encontró información del vehículo
                if (!apiInfo.Found)
                {
                    CustomMessageBox.Show(
                        "No se encontró información del vehículo para la patente proporcionada.",
                        "Información No Encontrada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;
                }
                else
                {
                    // Se encontró información del vehículo, se guarda en la base de datos y se asigna a los controles
                    VehicleInfoModel model = apiInfo.Data ?? new VehicleInfoModel();

                    VehicleInfo = new VehicleInfoRecord
                    {
                        Plate = model.Plate ?? "N/A",
                        Make = model.Make ?? "N/A",
                        Model = model.Model ?? "N/A",
                        Year = model.Year ?? DateTime.Now.Year,
                        PlateFormat = model.PlateFormat ?? "N/A",
                        VehicleKind = model.VehicleKind ?? "N/A",
                        Province = model.CurrentLocation?.Province ?? "N/A",
                        City = model.CurrentLocation?.City ?? "N/A",
                        LastLookup = DateTime.Now,
                    };
                    DataBaseManager.SaveVehicleInfo(VehicleInfo);
                    await AssignValuesToControlsAsync();
                }
            }
            else
            {
                // Hubo un error en la respuesta de la API
                CustomMessageBox.Show(
                    "Ocurrió un error al procesar la respuesta de la API.",
                    "Error de Consulta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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
            if (VehicleInfo != null)
            {
                var v = VehicleInfo;
                string provinciaFormateada = TextFormatter(v.Province);
                string ciudadFormateada = TextFormatter(v.City);
                TxtPlate.Text = v.Plate;
                TxtMake.Text = TextFormatter(v.Make);
                TxtModel.Text = TextFormatter(v.Model);
                TxtAnio.Text = v.Year.ToString();
                TxtPlateFormat.Text = PlateFormatter(v.PlateFormat);
                TxtVehicleKind.Text = PlateFormatter(v.VehicleKind);
                TxtProvince.Text = provinciaFormateada;
                TxtCity.Text = ciudadFormateada;

                await SetMapByProvinceAndCityAsync(
                    "Argentina",
                    provinciaFormateada,
                    ciudadFormateada
                );
            }
        }

        private void LookupButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearchPlate.Text))
            {
                CustomMessageBox.Show(
                    "Por favor, ingrese una patente válida.",
                    "Error de Consulta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            VehicleInfoRecord? info = DataBaseManager.GetVehicleInfo(TxtSearchPlate.Text.Trim());

            if (info != null)
            {
                MessageBoxResult result = CustomMessageBox.Show(
                    $"Se encontró información en la base de datos para la patente "
                        + $"{TxtSearchPlate.Text.Trim()} \n"
                        + $"desea actualizarla?",
                    "Información Encontrada",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );

                if (result == MessageBoxResult.No)
                {
                    // El usuario eligió no actualizar, asignar la información existente
                    VehicleInfo = info;
                    Dispatcher.BeginInvoke(
                        new Action(async () =>
                        {
                            await AssignValuesToControlsAsync();
                        }),
                        DispatcherPriority.Input
                    );
                    return;
                }
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

        private async Task SetMapByProvinceAndCityAsync(
            string pais,
            string provincia,
            string ciudad
        )
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

        private async Task<(double Lat, double Lon)?> GeocodeLocationAsync(
            string pais,
            string provincia,
            string ciudad
        )
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
                string url =
                    $"https://{geoHost}/search?q={Uri.EscapeDataString(queryText)}&format=json&limit=1";
                string json = await _httpClient.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    if (
                        double.TryParse(
                            first.GetProperty("lat").GetString(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out double lat
                        )
                        && double.TryParse(
                            first.GetProperty("lon").GetString(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out double lon
                        )
                    )
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
