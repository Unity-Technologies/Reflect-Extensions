namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    ///  Interface for ways to locate image targets for any Image Target Handler in the ImageTrackingManager
    /// </summary>
    public interface ILocateImageTargets
    {
        void LocateImageTarget(Bounds _bounds);
        void LocateImageTarget(Vector3 movePosition);
    }
}