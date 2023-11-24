using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        float Speed { get; set; }
        float StepSize { get; set; }

        CobotPosition GetCobotPosition();
        void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction);
        void MoveToDirect(CobotPosition destination);
    }
}