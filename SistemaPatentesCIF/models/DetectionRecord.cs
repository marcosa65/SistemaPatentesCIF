using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaPatentesCIF.models
{
    public class DetectionRecord
    {
        public int Id { get; set; }

        public string Plate { get; set; } = string.Empty;

        public double Confidence { get; set; }

        public string Country { get; set; } = string.Empty;

        public DateTime DetectionDate { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        // Información adicional obtenida de la API / Base de datos
        public VehicleInfoModel? VehicleInfo { get; set; }

        // Propiedad que controla si el botón de la fila se habilita o no
        public bool HasVehicleInfo => VehicleInfo.HasValue;

    }
}
