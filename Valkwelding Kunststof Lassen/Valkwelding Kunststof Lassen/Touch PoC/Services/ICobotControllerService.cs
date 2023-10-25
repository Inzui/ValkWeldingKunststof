using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        int Speed { get; set; }
        float StepSize { get; set; }

        CobotPosition GetCobotPosition();
        void MoveToDirect(CobotPosition destination);
        void MoveToSteps(float[] point);
        void DetectObject(CobotPosition startingPosition);
    }
}