using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaPatentesCIF.models
{
    public class VehicleInfoRecord
    {
        public string Plate { get; set; } = string.Empty;

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int? Year { get; set; }

        public string PlateFormat { get; set; } = string.Empty;

        public string VehicleKind { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public DateTime LastLookup { get; set; }
    }
}
