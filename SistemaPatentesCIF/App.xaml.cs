using System.IO;
using System.Windows;
using Python.Runtime;
using SistemaPatentesCIF.data;
using SistemaPatentesCIF.models;
using SistemaPatentesCIF.themes;
using SistemaPatentesCIF.utils;
using SistemaPatentesCIF.views;

namespace SistemaPatentesCIF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string localPython = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                "Python",
                "python314",
                "python314.dll"
            );

            // Establecer la ruta del intérprete de Python antes de inicializar el runtime
            Runtime.PythonDLL = localPython;

            try
            {
                PythonEngine.Initialize();
                Logger.Info("App.xaml.cs", "Python.NET inicializado correctamente en OnStartup.");
            }
            catch (System.Exception ex)
            {
                Logger.Info(
                    "App.xaml.cs",
                    $"Error inicializando Python.NET en OnStartup: {ex.Message}"
                );
            }

            CreateDataBaseIfNotExists();
            var settings = SettingsManager.Load();
            ThemeManager.ApplyTheme(settings.ThemeName);
        }

        private void CreateDataBaseIfNotExists()
        {
            if (!DataBaseManager.ExistDataBase())
            {
                Logger.Info(
                    "App.xaml.cs",
                    "No se encontró la base de datos. Se creará una nueva base de datos."
                );
                DataBaseManager.Initialize();
                LoadDataForTesting(); // Carga de datos de prueba -- solo en modo de desarrollo o pruebas,
                // Esta linea debe eliminarse o comentarse en producción !!!
            }
            else
            {
                Logger.Info(
                    "App.xaml.cs",
                    "Se encontró la base de datos. Se cargará la base de datos existente."
                );
            }
        }

        private void LoadDataForTesting()
        {
            // Carga de datos de prueba para la ventana de historial
            // solo en modo de desarrollo o pruebas, no en producción
            DataBaseManager.SaveDetection(
                new DetectionRecord
                {
                    Plate = "AH067UT",
                    Confidence = 98,
                    Country = "Argentina",
                    DetectionDate = DateTime.Now.AddMinutes(-15),
                    ImagePath = "demo3.jpg",
                }
            );

            DataBaseManager.SaveDetection(
                new DetectionRecord
                {
                    Plate = "AE502GN",
                    Confidence = 97,
                    Country = "Argentina",
                    DetectionDate = DateTime.Now.AddMinutes(-12),
                    ImagePath = "demo5.jpg",
                }
            );
            DataBaseManager.SaveDetection(
                new DetectionRecord
                {
                    Plate = "AB123CD",
                    Confidence = 95,
                    Country = "Argentina",
                    DetectionDate = DateTime.Now.AddMinutes(-10),
                    ImagePath = "demo1.jpg",
                }
            );
            DataBaseManager.SaveDetection(
                new DetectionRecord
                {
                    Plate = "XY987ZT",
                    Confidence = 92,
                    Country = "Argentina",
                    DetectionDate = DateTime.Now.AddMinutes(-5),
                    ImagePath = "demo2.jpg",
                }
            );
            DataBaseManager.SaveDetection(
                new DetectionRecord
                {
                    Plate = "LM456OP",
                    Confidence = 90,
                    Country = "Argentina",
                    DetectionDate = DateTime.Now.AddMinutes(-2),
                    ImagePath = "demo4.jpg",
                }
            );

            DataBaseManager.SaveVehicleInfo(
                new VehicleInfoRecord
                {
                    Plate = "AH067UT",
                    Make = "Ford",
                    Model = "Focus III Titanium",
                    Year = 2018,
                    VehicleKind = "Sedán 5 Puertas",
                    PlateFormat = "Mercosur (AA000AA)",
                    Province = "Formosa",
                    City = "Clorinda",
                    LastLookup = DateTime.Now,
                }
            );
            DataBaseManager.SaveVehicleInfo(
                new VehicleInfoRecord
                {
                    Plate = "AE502GN",
                    Make = "Toyota",
                    Model = "Hilux SRX",
                    Year = 2022,
                    VehicleKind = "Pick-Up",
                    PlateFormat = "Mercosur (AA000AA)",
                    Province = "Formosa",
                    City = "Formosa Capital",
                    LastLookup = DateTime.Now,
                }
            );
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (PythonEngine.IsInitialized)
                {
                    PythonEngine.Shutdown();
                    Logger.Info("App.xaml.cs", "Python.NET apagado en OnExit.");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Info("App.xaml.cs", $"Error cerrando Python.NET en OnExit: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}
