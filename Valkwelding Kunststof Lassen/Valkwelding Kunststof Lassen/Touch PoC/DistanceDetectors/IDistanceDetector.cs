using ValkWelding.Welding.PolyTouchApplication.Types;

namespace ValkWelding.Welding.PolyTouchApplication.DistanceDetectors
{
    public interface IDistanceDetector
    {
        bool Connected { get; }
        DetectorResponse SendCommand(DetectorCommand command);
        bool Connect(string comPort);
    }
}