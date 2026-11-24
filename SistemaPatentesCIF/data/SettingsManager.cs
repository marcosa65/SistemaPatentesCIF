using SistemaPatentesCIF.models;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SistemaPatentesCIF
{
    public static class SettingsManager
    {

        public static readonly string FolderPath = AppContext.BaseDirectory;
        //public static readonly string FolderPath = Path.Combine(
        //    Environment.GetFolderPath(
        //        Environment.SpecialFolder.LocalApplicationData),
        //    "SistemaPatentesCIF"
        //);

        private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

        public static AppSettings Current { get; private set; } = Load();

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                        return Current;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message, 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning
                );
                // Si el archivo está corrupto, devuelve los valores por defecto
            }

            Current = new AppSettings();
            Save(Current);
            return Current;
        }

        public static void Save(AppSettings settings)
        {
            Directory.CreateDirectory(FolderPath);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(FilePath, json);
            Current = settings;
        }
    }
}