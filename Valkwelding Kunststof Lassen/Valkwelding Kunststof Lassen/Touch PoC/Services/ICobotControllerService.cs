using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        float Speed { get; set; }
        float StepSize { get; set; }

        CobotPosition GetCobotPosition();
        void MoveStepToObject(CobotPosition startingPosition);
        void MoveToDirect(CobotPosition destination);
        void MoveToSteps(float[] point);
    }
}