using System.Data.SQLite;
using System.IO;
using SistemaPatentesCIF.models;

namespace SistemaPatentesCIF.data
{
    static class DataBaseManager
    {
        private static readonly string DbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "alpr.db"
        );

        private static readonly string ConnectionString = $"Data Source={DbPath}";

        public static void Initialize()
        {
            using var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            string sql = """
                CREATE TABLE IF NOT EXISTS Detections
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,

                    Plate TEXT NOT NULL,

                    Confidence REAL,

                    Country TEXT,

                    DetectionDate TEXT,

                    ImagePath TEXT
                );
                """;

            var command = new SQLiteCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public static void SaveDetection(DetectionRecord record)
        {
            using var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            string sql = """
                INSERT INTO Detections
                (
                    Plate,
                    Confidence,
                    Country,
                    DetectionDate,
                    ImagePath
                )
                VALUES
                (
                    $plate,
                    $confidence,
                    $country,
                    $date,
                    $image
                );
                """;

            var command = new SQLiteCommand(sql, connection);

            command.Parameters.AddWithValue("$plate", record.Plate);

            command.Parameters.AddWithValue("$confidence", record.Confidence);

            command.Parameters.AddWithValue("$country", record.Country);

            command.Parameters.AddWithValue("$date", record.DetectionDate.ToString("O"));

            command.Parameters.AddWithValue("$image", record.ImagePath);

            command.ExecuteNonQuery();
        }

        public static List<DetectionRecord> GetAllDetections()
        {
            List<DetectionRecord> result = new();

            using var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            string sql = "SELECT * FROM Detections ORDER BY Id DESC";

            var command = new SQLiteCommand(sql, connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(
                    new DetectionRecord
                    {
                        Id = reader.GetInt32(0),

                        Plate = reader.GetString(1),

                        Confidence = reader.GetDouble(2),

                        Country = reader.GetString(3),

                        DetectionDate = DateTime.Parse(reader.GetString(4)),

                        ImagePath = reader.GetString(5),
                    }
                );
            }

            return result;
        }

        public static DetectionRecord? GetLastDetection()
        {
            using var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            string sql = "SELECT * FROM Detections ORDER BY DetectionDate DESC LIMIT 1";

            var command = new SQLiteCommand(sql, connection);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new DetectionRecord
                {
                    Id = reader.GetInt32(0),
                    Plate = reader.GetString(1),
                    Confidence = reader.GetDouble(2),
                    Country = reader.GetString(3),
                    DetectionDate = DateTime.Parse(reader.GetString(4)),
                    ImagePath = reader.GetString(5),
                };
            }
            else
            {
                return null;
            }

            //List<DetectionRecord> lastDetection = GetAllDetections();
            //return lastDetection.FirstOrDefault();
        }
    }
}
