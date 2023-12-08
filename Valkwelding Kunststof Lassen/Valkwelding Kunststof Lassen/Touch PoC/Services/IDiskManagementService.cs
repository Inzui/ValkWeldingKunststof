using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IDiskManagementService
    {
        void ExportPositions(IEnumerable<CobotPosition> positions);
        IEnumerable<CobotPosition> ImportPositions();
        void WriteSettings(SettingsModel settingsModel);
        SettingsModel LoadSettings();
    }
}