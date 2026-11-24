using System.Data.SQLite;
using System.IO;
using SistemaPatentesCIF.models;

namespace SistemaPatentesCIF.data
{
    static class DataBaseManager
    {
        private static readonly string DbPath = Path.Combine(
            SettingsManager.FolderPath,
            "alprlocal.db"
        );

        private static readonly string ConnectionString = $"Data Source={DbPath}";

        public static void Initialize()
        {
           Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);

            using var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            CreateTables(connection);

            CreateIndexes(connection);
        }

        private static void CreateTables( SQLiteConnection connection)
        {
            string sql = @"

            CREATE TABLE IF NOT EXISTS Plates
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Plate TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS VehicleInfo
            (
                PlateId INTEGER PRIMARY KEY,
                Make TEXT,
                Model TEXT,
                Year INTEGER,
                PlateFormat TEXT,
                VehicleKind TEXT,
                Province TEXT,
                City TEXT,
                LastLookup TEXT,
                FOREIGN KEY(PlateId)
                    REFERENCES Plates(Id)
            );

            CREATE TABLE IF NOT EXISTS Detections
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlateId INTEGER NOT NULL,
                Confidence REAL,
                Country TEXT,
                DetectionDate TEXT,
                ImagePath TEXT,
                FOREIGN KEY(PlateId)
                    REFERENCES Plates(Id)
            );";

            using var command =
                new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void CreateIndexes(SQLiteConnection connection)
        {
            string sql = @"
                CREATE INDEX IF NOT EXISTS
                IX_Plates_Plate
                ON Plates(Plate);

                CREATE INDEX IF NOT EXISTS
                IX_Detections_PlateId
                ON Detections(PlateId);

                CREATE INDEX IF NOT EXISTS
                IX_Detections_Date
                ON Detections(DetectionDate);";

            using var command =
                new SQLiteCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        private static int GetOrCreatePlateId ( string plate )
        {
            using var connection =
                new SQLiteConnection(ConnectionString);

            connection.Open();

            // Comando SQL para seleccionar el Id de la placa si existe
            const string selectSql = @"
                SELECT Id
                FROM Plates
                WHERE Plate = $plate;";

            // Establecemos la conexion a usar y pasamos el parametro string con la consulta SQL
            using var selectCmd =
                new SQLiteCommand(selectSql, connection);

            // Agregamos el parametro con el valor actual de "plate" a la consulta SQL
            selectCmd.Parameters
                .AddWithValue("$plate", plate);

            // Ejecutamos la consulta y obtenemos el resultado
            var result = selectCmd.ExecuteScalar();

            // Si el resultado no es nulo, significa que la placa ya existe en la base de datos
            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                // Si el resultado es nulo, significa que la placa no existe en la base de datos
                const string insertSql = @"
                    INSERT INTO Plates (Plate)
                    VALUES ($plate);
                    SELECT last_insert_rowid();";

                // Creamos un nuevo comando SQL para insertar la nueva placa en la base de datos
                using var insertCmd =
                    new SQLiteCommand(insertSql, connection);

                // Agregamos el parametro con el valor actual de "plate" a la consulta SQL
                insertCmd.Parameters
                    .AddWithValue("$plate", plate);

                // Ejecutamos la consulta y obtenemos el Id de la nueva placa insertada
                return Convert.ToInt32(
                    insertCmd.ExecuteScalar()); 
            }
        }

        public static void SaveDetection ( DetectionRecord detection )
        {
            using var connection =
                new SQLiteConnection(ConnectionString);

            connection.Open();

            int plateId = GetOrCreatePlateId( detection.Plate );

            const string sql = @"
                INSERT INTO Detections
                (
                    PlateId,
                    Confidence,
                    Country,
                    DetectionDate,
                    ImagePath
                )
                VALUES
                (
                    $plateId,
                    $confidence,
                    $country,
                    $detectionDate,
                    $imagePath
                );";

            using var command =
                new SQLiteCommand(sql, connection);

            command.Parameters.AddWithValue(
                "$plateId",
                plateId);

            command.Parameters.AddWithValue(
                "$confidence",
                detection.Confidence);

            command.Parameters.AddWithValue(
                "$country",
                detection.Country);

            command.Parameters.AddWithValue(
                "$detectionDate",
                detection.DetectionDate.ToString("O"));

            command.Parameters.AddWithValue(
                "$imagePath",
                detection.ImagePath);

            command.ExecuteNonQuery();
        }

        public static DetectionRecord? GetLastDetection ()
        {
            using var connection =
                    new SQLiteConnection(ConnectionString);

            connection.Open();

            string sql = @"SELECT d.Id, p.Plate, d.Confidence, d.Country, d.DetectionDate, d.ImagePath
                               FROM Detections d
                               INNER JOIN Plates p
                               ON p.Id = d.PlateId
                               ORDER BY d.DetectionDate DESC LIMIT 1;";

            using var command =
                    new SQLiteCommand(sql, connection);

            using var reader =
                    command.ExecuteReader();

            if (reader.Read())
                {
                    return new DetectionRecord
                    {
                        Id = reader.GetInt32(0),
                        Plate = reader.GetString(1),
                        Confidence = reader.GetDouble(2),
                        Country = reader.GetString(3),
                        DetectionDate =
                            DateTime.Parse(
                                reader.GetString(4)),
                        ImagePath = reader.GetString(5)
                    };
            }
            return null;
        }

        public static List<DetectionRecord> GetAllDetections()
        {
            List<DetectionRecord> result = new();

            using var connection =
                new SQLiteConnection(ConnectionString);

            connection.Open();

            // Comando SQL para seleccionar todas las detecciones junto con la información de la placa
            const string sql = @"SELECT d.Id, p.Plate, d.Confidence, d.Country, d.DetectionDate, d.ImagePath
                                 FROM Detections d
                                 INNER JOIN Plates p
                                 ON p.Id = d.PlateId
                                 ORDER BY d.DetectionDate DESC;";
            // Agregamos el comando SQL a la conexión y ejecutamos la consulta
            using var command =
                new SQLiteCommand(sql, connection);

            using var reader =
                command.ExecuteReader();

            // Iteramos sobre los resultados de la consulta y agregamos cada detección a la lista de resultados
            while (reader.Read())
            {
                result.Add(
                    new DetectionRecord
                    {
                        Id = reader.GetInt32(0),
                        Plate = reader.GetString(1),
                        Confidence = reader.GetDouble(2),
                        Country = reader.GetString(3),
                        DetectionDate =
                            DateTime.Parse(
                                reader.GetString(4)),
                        ImagePath =
                            reader.GetString(5)
                    });
            }

            return result;
        }

        public static List<string> GetAllPlates ()
        {
                List<string> result = new();
                using var connection = new SQLiteConnection(ConnectionString);
                connection.Open();
                string sql = "SELECT Plate FROM Plates ORDER BY Plate ASC";
                var command = new SQLiteCommand(sql, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                return result;
        }

        public static VehicleInfoRecord? GetVehicleInfo( string plate )
        {
            
            using var connection =
                new SQLiteConnection(ConnectionString);

            connection.Open();

            const string sql = @"
                SELECT v.Make, v.Model, v.Year, v.PlateFormat,
                       v.VehicleKind, v.Province, v.City, v.LastLookup
                FROM VehicleInfo v
                INNER JOIN Plates p ON p.Id = v.PlateId
                WHERE p.Plate = $plate;";
            
            using var command =
                new SQLiteCommand(sql, connection);
            
            command.Parameters.AddWithValue(
                "$plate", plate);
            
            using var reader =
                command.ExecuteReader();
            
            if (reader.Read())
            {
                return new VehicleInfoRecord
                {
                    Plate = plate,
                    Make = reader.GetString(0),
                    Model = reader.GetString(1),
                    Year = reader.GetInt32(2),
                    PlateFormat = reader.GetString(3),
                    VehicleKind = reader.GetString(4),
                    Province = reader.GetString(5),
                    City = reader.GetString(6),
                    LastLookup = DateTime.Parse(reader.GetString(7))
                };
            }
            else
            {
                return null;
            }

        }

        public static void SaveVehicleInfo(VehicleInfoRecord info)
        {
            using var connection =
                new SQLiteConnection(ConnectionString);

            connection.Open();

            int plateId =
                GetOrCreatePlateId(info.Plate);

            const string sql = @"
            INSERT INTO VehicleInfo
            (
                PlateId,
                Make,
                Model,
                Year,
                PlateFormat,
                VehicleKind,
                Province,
                City,
                LastLookup
            )
            VALUES
            (
                $plateId,
                $make,
                $model,
                $year,
                $plateFormat,
                $vehicleKind,
                $province,
                $city,
                $lastLookup
            )

            ON CONFLICT(PlateId)
            DO UPDATE SET
                Make = excluded.Make,
                Model = excluded.Model,
                Year = excluded.Year,
                PlateFormat = excluded.PlateFormat,

            VehicleKind = excluded.VehicleKind,

            Province = excluded.Province,

            City = excluded.City,

            LastLookup = excluded.LastLookup;
    ";

            using var command =
                new SQLiteCommand(sql, connection);

            command.Parameters.AddWithValue(
                "$plateId", plateId);

            command.Parameters.AddWithValue(
                "$make", info.Make);

            command.Parameters.AddWithValue(
                "$model", info.Model);

            command.Parameters.AddWithValue(
                "$year", info.Year);

            command.Parameters.AddWithValue(
                "$plateFormat", info.PlateFormat);

            command.Parameters.AddWithValue(
                "$vehicleKind", info.VehicleKind);

            command.Parameters.AddWithValue(
                "$province", info.Province);

            command.Parameters.AddWithValue(
                "$city", info.City);

            command.Parameters.AddWithValue(
                "$lastLookup",
                info.LastLookup.ToString("O"));

            command.ExecuteNonQuery();
        }

        public static bool ExistDataBase()
        {
            
            if (File.Exists(DbPath))
            {
                var info = new FileInfo(DbPath);
                if (info.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }   
            }
            else
            {
                return false;
            }
        }
    }
}
