using UnityEngine.XR.ARFoundation;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Interface for using different image target handlers designed to be attached to the ImageTrackingManager.
    /// What to do when tracking is found and lost, when entering and exiting AR and resetting.
    /// </summary>
    public interface IHandleImageTargets
    {
        void FoundTracking(ARTrackedImage trackedImage);
        void LostTracking(ARTrackedImage trackedImage);
        void StartHandlingAR();
        void StopHandlingAR();
        void ResetTracking();
        bool InARImageTracking { get; }
    }
}