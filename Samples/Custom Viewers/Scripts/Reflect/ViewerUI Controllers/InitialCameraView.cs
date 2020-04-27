namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Looks for the 3D View object in the Metadata and uses that transform to set initial camera view on Sync Object creation
    /// </summary>
    public class InitialCameraView : MonoBehaviour, IObserveMetadata
    {
        [Tooltip("Camera to move on start.")]
        [SerializeField] Transform cameraToMove = default;
        [Tooltip("UI Toggle on Project view menu.")]
        [SerializeField] UnityEngine.UI.Toggle ifCheckingCamera = default;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch("Family", "3D View", true));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        /// <summary>
        /// What to do when a Metadata parameter is found during a sync object creation
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            // If the toggle is checked on the project menu
            if (ifCheckingCamera != null && ifCheckingCamera.isOn && reflectObject != null)
            {
                if (cameraToMove != null)
                {
                    // Use the found transform
                    cameraToMove.SetPositionAndRotation(reflectObject.transform.position, reflectObject.transform.rotation);
                }
            }
        }
    }
}