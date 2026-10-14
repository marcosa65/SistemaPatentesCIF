using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Text.Json.Serialization;

namespace SistemaPatentesCIF.models
{
    public class ALPRResult
    {
        public bool Success { get; set; }
        public List<PlateResult>? Plates { get; set; }
        public string? Msg { get; set; }
    }

    public class PlateResult
    {
        public string? Text { get; set; }
        public double Confidence { get; set; }
        public string? Region { get; set; }
        public double RegionConfidence { get; set; }
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
    }

    public class ApiInfo
    {
        [JsonPropertyName("found")]
        public bool Found { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public VehicleInfoModel? Data { get; set; }
    }

    public struct VehicleInfoModel
    {
        [JsonPropertyName("plate")]
        public string? Plate { get; set; }

        [JsonPropertyName("make")]
        public string? Make { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("plateFormat")]
        public string? PlateFormat { get; set; }

        [JsonPropertyName("vehicleKind")]
        public string? VehicleKind { get; set; }

        [JsonPropertyName("currentLocation")]
        public VehicleLocationInfoModel? CurrentLocation { get; set; }
    }

    public struct VehicleLocationInfoModel
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }
    }
}
