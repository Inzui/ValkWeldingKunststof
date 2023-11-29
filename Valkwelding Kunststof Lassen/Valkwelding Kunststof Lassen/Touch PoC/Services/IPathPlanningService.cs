using System.Collections.Generic;
using System.Threading.Tasks;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IPathPlanningService
    {
        void Detect(IEnumerable<CobotPosition> measurePoints, int pointsBetween);
        void Start();
    }
}