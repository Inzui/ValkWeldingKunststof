namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotConnectionService
    {
        void Connect(string ipAddress);
        int readError();
        float[] readPos();
        void sendCobotMove(float[] point, int speed);
        void sendCobotPos(float[] point, int speed);
    }
}