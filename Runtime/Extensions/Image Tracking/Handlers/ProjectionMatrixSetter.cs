using UnityEngine.XR.ARFoundation;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Uses the AR Foundation's Camera Manager projection matrix settings to set other cameras to match that setup
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ProjectionMatrixSetter : MonoBehaviour
    {
        [Tooltip("The AR Camera Manager for whose project matrix will be copied to the camera with this script on it.")]
        [SerializeField]
        ARCameraManager cameraManager = default;
        Camera thisCamera = default;

        void Start()
        {
            thisCamera = GetComponent<Camera>();
        }

        void OnFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (thisCamera != null && eventArgs.projectionMatrix.HasValue)
            {
                thisCamera.projectionMatrix = eventArgs.projectionMatrix.Value;
            }
        }

        /// <summary>
        /// Start matching the Camera Manager's camera projection matrix
        /// </summary>
        public void StartMatchingProjection()
        {
            if (cameraManager != null)
                cameraManager.frameReceived += OnFrameReceived;
        }

        /// <summary>
        /// Stop matching the Camera Manager's camera projection matrix
        /// </summary>
        public void StopMatchingProjection()
        {
            if (cameraManager != null)
                cameraManager.frameReceived -= OnFrameReceived;
            if (thisCamera != null)
                thisCamera.ResetProjectionMatrix();
        }
    }
}
