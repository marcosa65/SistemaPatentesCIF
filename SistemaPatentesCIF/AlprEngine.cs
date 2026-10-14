using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SistemaPatentesCIF
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using SistemaPatentesCIF.models;
    namespace AlprLab
    {
        public static class AlprEngine
        {
            public static ALPRResult StartEngine(string filename)
            {
                try
                {
                    // Asumir que el runtime de Python fue configurado e inicializado en el inicio de la aplicación.
                    if (!PythonEngine.IsInitialized)
                    {
                        Debug.WriteLine("Python runtime no inicializado. Configure Runtime.PythonDLL y llame a PythonEngine.Initialize() en App.OnStartup.");
                        return new ALPRResult { Success = false, Plates = null };
                    }

                    Debug.WriteLine($"Usando Python.NET inicializado. Procesando archivo {filename}");
                    using (Py.GIL())
                    {
                        dynamic sys = Py.Import("sys");
                        // Agregar la ruta del script al sys.path
                        string scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
                        sys.path.append(scriptsPath);
                        // Importar el script de Python para hacer la llamada al engine de ALPR
                        dynamic alpr = Py.Import("script_alpr");
                        // Llamar a la función detect_plate del script de Python y luego
                        // deserializar el resultado JSON a un objeto ALPRResult
                        string alprJson = alpr.detect_plate(filename);
                        ALPRResult? res = JsonSerializer.Deserialize<ALPRResult>(alprJson);
                        Debug.WriteLine($"Resultado de ALPR: {res?.Msg}");
                        if (res != null && res.Plates != null)
                        {
                            foreach (var plate in res.Plates)
                            {
                                Debug.WriteLine($"Placa: {plate.Text}, " +
                                $"Confianza: {plate.Confidence}, " +
                                $"Región: {plate.Region}, " +
                                $"Confianza de Región: {plate.RegionConfidence}");
                            }
                            return res;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al inicializar Python.NET: {ex.Message}");
                }
                return new ALPRResult { Success = false, Plates = null };
            }
        }
    }
}
