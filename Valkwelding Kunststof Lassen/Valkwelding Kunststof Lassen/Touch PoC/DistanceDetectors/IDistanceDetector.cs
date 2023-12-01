namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public interface IDistanceDetector
    {
        bool Connected { get; }
        bool ObjectDetected { get; }

        void Start();
        void EnableProbe();
    }
}