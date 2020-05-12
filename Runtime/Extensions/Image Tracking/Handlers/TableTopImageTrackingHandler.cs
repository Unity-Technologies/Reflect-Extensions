using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Example of handling AR image tracking in the scene - what to do when tracking is found and lost, when entering and exiting AR, how to handle UI elements, etc.
    /// This particular example uses the image to display Table Top AR.
    /// </summary>
    /// <remarks>The System.ComponentModel.Description attribute is used to describe this handler in the Image Tracking Manager editor.
    /// Start this Image Tacking Handler by calling the StartHandlingAR public method.</remarks>
#if UNITY_EDITOR
    [System.ComponentModel.Description("Table Top Image Tracking")]
#endif
    public class TableTopImageTrackingHandler : MonoBehaviour, IHandleImageTargets, ILocateImageTargets
    {
        [Tooltip("Scale for the model on the target.")]
        [SerializeField] float scaleForModelOnTarget = 0.01f;
        [Tooltip("The AR Table Top Camera Controller component used for manipulating the AR camera in table top mode.")]
        [SerializeField] UnityEngine.Reflect.Controller.Controller aRController = default;
        [Tooltip("The Gameobject containing the Model View camera.")]
        [SerializeField] GameObject screenMode = default;
        [Tooltip("The Gameobject containing the AR camera and session origin.")]
        [SerializeField] GameObject aRMode = default;
        [Tooltip("The AR Session - there should only be one.")]
        [SerializeField] ARSession aRSession = default;
        [Tooltip("The UI Gameobject with the find image target helper.")]
        [SerializeField] GameObject targetHelper = default;
        [Tooltip("Button to leave AR mode.")]
        [SerializeField] Button leaveARButton = default;
        [Tooltip("Button to reset tracking.")]
        [SerializeField] Button resetTrackingButton = default;
        [Tooltip("The AR Session Origin to be used by this tracking handler.")]
        [SerializeField] ARSessionOrigin sessionOrigin = default;
        [Tooltip("The AR Camera to be used for this tracking handler.")]
        [SerializeField] Camera aRCamera = default;
        /// <summary>
        /// Scale for the model on the target. This scale will be inversed for the session space scale.
        /// More initutive for user to think smaller number means scale down the model.
        /// </summary>
        /// <value>Scale for the model on the target</value>
        public float ScaleForModelOnTarget { get => scaleForModelOnTarget; }
        int showCameraMask;
        float initialClippingPlane;
        Quaternion initialSessionOriginRotation;
        Vector3 initialSessionOriginPosition;
        Vector3 initialSessionOriginScale;
        Vector3 targetLocationToBeUsed;
        bool inARImageTracking;

        void Start()
        {
            // Save initial values
            if (aRCamera != null)
            {
                showCameraMask = aRCamera.cullingMask;
                initialClippingPlane = aRCamera.farClipPlane;
            }
            if (sessionOrigin != null)
            {
                initialSessionOriginRotation = sessionOrigin.transform.rotation;
                initialSessionOriginPosition = sessionOrigin.transform.position;
                initialSessionOriginScale = sessionOrigin.transform.localScale;
            }
        }

        void OnDisable()
        {
            ImageTrackingManager.Instance.DetachTrackingHandler(this);
            ImageTrackingManager.Instance.DetachLocater(this);
        }

        #region IHandleImageTargets implementation
        /// <summary>
        /// If currently in AR
        /// </summary>
        /// <value>True if in AR, false otherwise</value>
        public bool InARImageTracking { get => inARImageTracking; }

        /// <summary>
        /// Start AR
        /// </summary>
        public void StartHandlingAR()
        {
            if (ImageTrackingManager.Instance.ARSupported)
                ImageTrackingManager.Instance.AttachLocater(this);
        }

        /// <summary>
        /// Stop AR
        /// </summary>
        public void StopHandlingAR()
        {
            ExitAR();
            // This is for an external Stop call (e.g. Exit AR button)
            if (ImageTrackingManager.Instance.ARSupported)
            {
                ImageTrackingManager.Instance.DetachLocater(this);
                ImageTrackingManager.Instance.DetachTrackingHandler(this);
            }
        }

        /// <summary>
        /// What happens when the Tracking Found notification occurs from the ImageTrackingManager
        /// </summary>
        /// <param name="trackedImage">The AR Tracked Image</param>
        public void FoundTracking(ARTrackedImage trackedImage)
        {
            if (inARImageTracking) // Helps to control unexpected calls
            {
                if (targetHelper != null)
                    targetHelper.SetActive(false);

                if (trackedImage != null && sessionOrigin != null)
                {
                    //Reset the session origin and clipping plane
                    sessionOrigin.transform.rotation = initialSessionOriginRotation;
                    sessionOrigin.transform.position = initialSessionOriginPosition;
                    sessionOrigin.transform.localScale = initialSessionOriginScale;
                    if (aRCamera != null)
                        aRCamera.farClipPlane = initialClippingPlane;

                    // Scale down to session origin to fit on the target by inversing scale
                    sessionOrigin.transform.localScale = new Vector3(1 / scaleForModelOnTarget, 1 / scaleForModelOnTarget, 1 / scaleForModelOnTarget);

                    // Create the content position
                    var targetLocation = new GameObject();
                    targetLocation.transform.position = targetLocationToBeUsed;
                    // Create the rotation to view the content using the tracked image's up rotation
                    var newRotation = Quaternion.Euler(new Vector3(
                        sessionOrigin.transform.eulerAngles.x, trackedImage.transform.localEulerAngles.y, sessionOrigin.transform.eulerAngles.z));
                    // Make it appear and then destroy the temporary location object
                    sessionOrigin.MakeContentAppearAt(targetLocation.transform, trackedImage.transform.position, newRotation);
                    GameObject.Destroy(targetLocation);

                    // Correctly scale the clipping plane of the ar camera
                    aRCamera.farClipPlane = initialClippingPlane / scaleForModelOnTarget;
                    // Allow camera to see content if it was masked
                    if (aRCamera != null && aRCamera.cullingMask == 1 << LayerMask.NameToLayer("UI"))
                    {
                        aRCamera.cullingMask = showCameraMask;
                    }
                }
            }
        }

        /// <summary>
        /// What happens when the Tracking Lost notification occurs from the ImageTrackingManager
        /// </summary>
        /// <param name="trackedImage">The AR Tracked Image</param>
        public void LostTracking(ARTrackedImage trackedImage)
        {
            if (inARImageTracking) // Helps to control unexpected calls
            {
                // Do not show content, only UI. Could also mask the raycast as well if desired.
                if (aRCamera != null)
                    aRCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
                if (targetHelper != null)
                    targetHelper.SetActive(true);
            }
        }

        /// <summary>
        /// Reset the tracking session
        /// </summary>
        public void ResetTracking()
        {
            if (aRSession != null)
                aRSession.Reset();
            LostTracking(null);
        }
        #endregion

        void EnterAR()
        {
            // Clean detach so no session resets call lost tracking if we were already in AR mode
            ImageTrackingManager.Instance.DetachTrackingHandler(this);
            HandleUI(true);
            if (aRController != null)
                aRController.enabled = true;

            inARImageTracking = true;
            // Call this manually to mask the camera
            LostTracking(null);

            // Enable the AR Session
            if (aRMode != null && aRSession != null)
            {
                aRMode.SetActive(true); //For first time in AR
                aRSession.enabled = true;
                aRSession.Reset();
            }

            // Turn off model view
            if (screenMode != null)
                screenMode.SetActive(false);

            // Listen for tracking
            ImageTrackingManager.Instance.AttachTrackingHandler(this);
        }

        void ExitAR()
        {
            if (inARImageTracking)
            {
                // Stop listening
                ImageTrackingManager.Instance.DetachTrackingHandler(this);

                HandleUI(false);
                if (aRController != null)
                    aRController.enabled = false;

                // Turn model view back on
                if (screenMode != null)
                    screenMode.SetActive(true);

                // Set camera to show nothing
                if (aRCamera != null)
                {
                    aRCamera.farClipPlane = initialClippingPlane;
                    aRCamera.cullingMask = 0;
                }
                // Disable the AR Session
                if (aRSession != null)
                {
                    aRSession.enabled = false;
                }

                //Reset the session origin
                sessionOrigin.transform.rotation = initialSessionOriginRotation;
                sessionOrigin.transform.position = initialSessionOriginPosition;
                sessionOrigin.transform.localScale = initialSessionOriginScale;

                inARImageTracking = false;
            }
        }

        #region ILocateImageTargets implementation
        /// <summary>
        /// Locate the image target using the Bounds property of the mesh or renderer
        /// </summary>
        /// <param name="_bounds">Bounds property</param>
        public void LocateImageTarget(Bounds _bounds)
        {
            if (ImageTrackingManager.Instance.ARSupported)
            {
                // Use the center of the bounds
                targetLocationToBeUsed = _bounds.center - new Vector3(0, _bounds.extents.y, 0);
                EnterAR();
            }
        }

        /// <summary>
        /// Locate the image target using a Vector3 position
        /// </summary>
        /// <param name="movePosition">Vector3 position</param>
        public void LocateImageTarget(Vector3 movePosition)
        {
            if (ImageTrackingManager.Instance.ARSupported)
            {
                // Use the position of the transform
                targetLocationToBeUsed = movePosition;
                EnterAR();
            }
        }
        #endregion

        void HandleUI(bool goingIntoAR)
        {
            if (goingIntoAR)
            {
                if (targetHelper != null)
                    targetHelper.SetActive(true);
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(true);
                if (resetTrackingButton != null)
                    resetTrackingButton.gameObject.SetActive(true);
            }
            else
            {
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(false);
                if (resetTrackingButton != null)
                    resetTrackingButton.gameObject.SetActive(false);
                if (targetHelper != null)
                    targetHelper.SetActive(false);
            }
        }
    }
}