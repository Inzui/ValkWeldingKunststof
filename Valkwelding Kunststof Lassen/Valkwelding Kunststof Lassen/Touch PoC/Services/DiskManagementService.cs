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
        JsonSerializerOptions _jsonOptions;

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
