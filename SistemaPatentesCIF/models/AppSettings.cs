using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaPatentesCIF.models
{
    public class AppSettings
    {
        public string ThemeName { get; set; } = "Batman";
        public string CameraSource { get; set; } = "0";
        public double MinConfidence { get; set; } = 75.0;
        public bool AutoSaveDetections { get; set; } = true;
        public string ApiUrl { get; set; } = "https://clasific.ar";
        public string ExportPath { get; set; } = @"C:\CIF\Exportaciones";
        public bool SandboxMode { get; set; } = false;
    }
}
