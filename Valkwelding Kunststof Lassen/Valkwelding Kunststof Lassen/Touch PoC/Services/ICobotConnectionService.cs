using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotConnectionService
    {
        bool CobotConnected { get; set; }

        Task CheckConnection(string ipAddress);
        int readError();
        float[] readPos();
        void sendCobotMove(float[] point, int speed);
        void sendCobotPos(float[] point, int speed);
    }
}