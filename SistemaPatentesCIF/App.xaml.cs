using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using Python.Runtime;
using SistemaPatentesCIF.data;
using SistemaPatentesCIF.models;
using SistemaPatentesCIF.themes;

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

            // Establecer la ruta del intérprete de Python antes de inicializar el runtime
            Runtime.PythonDLL =
                @"C:\Users\Laboratorio\AppData\Local\Python\pythoncore-3.14-64\python314.dll";
            try
            {
                PythonEngine.Initialize();
                Debug.WriteLine("Python.NET inicializado en OnStartup.");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error inicializando Python.NET en OnStartup: {ex.Message}");
            }
            //DataBaseManager.Initialize();
            //DataBaseManager.SaveDetection(
            //    new DetectionRecord
            //        {
            //            Plate = "ABC123",
            //            Confidence = 95,
            //            Country = "Argentina",
            //            DetectionDate = DateTime.Now
            //        }
            //);
            //List<DetectionRecord> records = DataBaseManager.GetAllDetections();
            //foreach (var record in records)
            //{
            //    Debug.WriteLine($"Registro: {record.Plate}, Confianza: {record.Confidence}, País: {record.Country}, Fecha: {record.DetectionDate}");
            //}
            //IconBuilder.CreateMultiResIcon(@"C:\Temp\image_raw.png", @"C:\Temp\app_icon.ico");

            var settings = SettingsManager.Load();
            ThemeManager.ApplyTheme(settings.ThemeName);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (PythonEngine.IsInitialized)
                {
                    PythonEngine.Shutdown();
                    Debug.WriteLine("Python.NET apagado en OnExit.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error cerrando Python.NET en OnExit: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}
