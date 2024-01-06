using System.Collections.Generic;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;

namespace ValkWelding.Welding.PolyTouchApplication.Services
{
    public interface IDiskManagementService
    {
        void ExportPositions(IEnumerable<CobotPosition> positions);
        IEnumerable<CobotPosition> ImportPositions();
        void WriteSettings(SettingsModel settingsModel);
        SettingsModel LoadSettings();
    }
}