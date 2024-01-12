using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;

namespace ValkWelding.Welding.PolyTouchApplication.Services
{
    public class DiskManagementService : IDiskManagementService
    {
        private readonly SaveFileDialog _saveFileDialog;
        private readonly OpenFileDialog _openFileDialog;
        private readonly CsvConfiguration _csvConfig;
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public DiskManagementService()
        {
            _filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Valk Welding", "PolyTouch", "Settings.json");
            _jsonOptions = new() { 
                WriteIndented = true 
            };

            _saveFileDialog = new()
            {
                FileName = "Export",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv"
            };
            _openFileDialog = new()
            {
                FileName = "Export",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv"
            };
            _csvConfig = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };
        }

        /// <summary>
        /// Exports a collection of CobotPositions to a CSV file.
        /// </summary>
        /// <param name="positions">A collection of CobotPositions to be exported.</param>
        /// <remarks>
        /// This method prompts the user to select a save location via a dialog box. If the user confirms, it opens a stream writer for the selected file and initializes a CSV writer with a specific configuration.
        /// It then writes the headers and records of the CobotPositions to the CSV file. Each record represents a position and includes its x, y, z, roll, pitch, and yaw values.
        /// After all positions have been written, it closes the stream writer, effectively saving the changes to the file.
        /// </remarks>
        public void ExportPositions(IEnumerable<CobotPosition> positions)
        {
            if (_saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new(_saveFileDialog.FileName))
                {
                    using (CsvWriter csv = new(writer, _csvConfig))
                    {
                        csv.WriteHeader<CobotPosition>();
                        csv.NextRecord();
                        foreach (CobotPosition pos in positions)
                        {
                            csv.WriteRecord(pos);
                            csv.NextRecord();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Imports a collection of CobotPositions from a CSV file.
        /// </summary>
        /// <returns>A collection of CobotPositions imported from the CSV file.</returns>
        /// <remarks>
        /// This method prompts the user to select a file via a dialog box. If the user confirms, it opens a stream reader for the selected file and initializes a CSV reader with a specific configuration.
        /// It then reads the records of the CSV file into a list of CobotPositions. Each record represents a position and includes its x, y, z, roll, pitch, and yaw values.
        /// After all positions have been read, it closes the stream reader and returns the list of positions.
        /// If the user cancels the file selection dialog, it returns null.
        /// </remarks>
        public IEnumerable<CobotPosition> ImportPositions() 
        {
            IEnumerable<CobotPosition> positions = null;

            if (_openFileDialog.ShowDialog() == true)
            {
                using (StreamReader reader = new(_openFileDialog.FileName))
                {
                    using (CsvReader csv = new(reader, _csvConfig))
                    {
                        positions = csv.GetRecords<CobotPosition>().ToList();
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Writes settings to a file in JSON format.
        /// </summary>
        /// <param name="settingsModel">The settings model to be written to the file.</param>
        /// <remarks>
        /// This method serializes the provided settings model to a JSON string using a specific JSON serialization configuration.
        /// It then checks if the directory of the file path exists and creates it if it doesn't.
        /// Finally, it writes the JSON string to the file at the specified file path.
        /// If any exception occurs during this process, it silently ignores it.
        /// </remarks>
        public void WriteSettings(SettingsModel settingsModel)
        {
            string jsonString = JsonSerializer.Serialize(settingsModel, _jsonOptions);
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(_filePath, jsonString);
            }
            catch { }
        }

        /// <summary>
        /// Loads settings from a file in JSON format.
        /// </summary>
        /// <returns>A SettingsModel object loaded from the file.</returns>
        /// <remarks>
        /// This method checks if the file at the specified file path exists. If it does, it reads the contents of the file into a string.
        /// It then attempts to deserialize this string into a SettingsModel object using a specific JSON deserialization configuration.
        /// If any exception occurs during this process, it returns a new SettingsModel object.
        /// If the file does not exist, it also returns a new SettingsModel object.
        /// </remarks>
        public SettingsModel LoadSettings()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string contents = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<SettingsModel>(contents, _jsonOptions);
                }
                catch
                {
                    return new SettingsModel();
                }
            }
            return new SettingsModel();
        }
    }
}
