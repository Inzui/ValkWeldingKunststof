namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        int Speed { get; set; }
        float StepSize { get; set; }

        void moveToDirect(float[] point);
        void moveToSteps(float[] point);
    }
}