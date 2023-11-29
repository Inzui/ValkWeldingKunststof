using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotConnectionService
    {
        bool CobotConnected { get; set; }
        bool CobotInRunMode { get; }

        Task CheckConnection(string ipAddress);
        int ReadError();
        float[] readPos();
        void sendCobotMove(float[] point, float speed);
        void sendCobotPos(float[] point, float speed);
        void SetCoilValue(int address, bool value);
        void StopCobot();
    }
}