namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotConnectionService
    {
        float[] readPos();
        void sendCobotMove(float[] point, int speed);
        void sendCobotPos(float[] point, int speed);
    }
}