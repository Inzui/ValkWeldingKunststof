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
using System.Threading.Tasks;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class DiskManagementService : IDiskManagementService
    {
        private readonly SaveFileDialog _saveFileDialog;
        private readonly OpenFileDialog _openFileDialog;
        private readonly CsvConfiguration _csvConfig;


        public DiskManagementService()
        {
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
    }
}
