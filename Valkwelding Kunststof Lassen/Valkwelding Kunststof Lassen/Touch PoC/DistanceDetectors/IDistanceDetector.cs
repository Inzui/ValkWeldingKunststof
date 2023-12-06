using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public interface IDistanceDetector
    {
        bool Connected { get; }
        DetectorResponse SendCommand(DetectorCommand command);
        void Start();
    }
}