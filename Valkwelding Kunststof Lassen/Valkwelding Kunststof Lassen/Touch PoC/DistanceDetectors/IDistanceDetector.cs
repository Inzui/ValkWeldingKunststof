namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public interface IDistanceDetector
    {
        bool ObjectDetected { get; set; }

        void Start();
    }
}