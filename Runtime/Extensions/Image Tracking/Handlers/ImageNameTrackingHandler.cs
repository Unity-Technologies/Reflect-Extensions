using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Example of handling AR image tracking in the scene - what to do when tracking is found and lost, when entering and exiting AR, how to handle UI elements, etc.
    /// This particular example uses the image name of the found tracked image to then drive to which location the session origin is moved.
    /// The image can be either on a horizontal or vertical surfaces. The image name matches that of the one used in the Reference Image Library.
    /// </summary>
    /// <remarks>The System.ComponentModel.Description attribute is used to describe this handler in the Image Tracking Manager editor.
    /// Start this Image Tacking Handler by calling the StartHandlingAR public method.</remarks>
#if UNITY_EDITOR
    [System.ComponentModel.Description("Image Name Driven Room Scale Tracking")]
#endif
    public class ImageNameTrackingHandler : MonoBehaviour, IHandleImageTargets
    {
        [Tooltip("The Image Name Location Controller for the top menu and button.")]
        [SerializeField] ImageTargetPositions imageNameLocator = default;
        [Tooltip("Button to leave AR mode.")]
        [SerializeField] Button leaveARButton = default;
        [Tooltip("Button to reset tracking.")]
        [SerializeField] Button resetButton = default;
        [Tooltip("The Gameobject containing the Model View camera.")]
        [SerializeField] GameObject screenMode = default;
        [Tooltip("The Gameobject containing the AR camera and session origin.")]
        [SerializeField] GameObject aRMode = default;
        [Tooltip("The AR Session - there should only be one.")]
        [SerializeField] ARSession aRSession = default;
        [Tooltip("The AR Table Top Camera Controller component used for manipulating the AR camera in table top mode.")]
        [SerializeField] UnityEngine.Reflect.Controller.Controller aRController = default;
        [Tooltip("The UI Gameobject with the find image target helper.")]
        [SerializeField] GameObject targetHelper = default;
        [Tooltip("The AR Session Origin to be used by this tracking handler.")]
        [SerializeField] ARSessionOrigin sessionOrigin = default;
        [Tooltip("The AR Camera to be used for this tracking handler.")]
        [SerializeField] Camera aRCamera = default;
        int showCameraMask;
        Transform targetLocationToBeUsed;
        bool inARImageTracking;

        void Start()
        {
            // Save initial values
            if (aRCamera != null)
                showCameraMask = aRCamera.cullingMask;
        }

        void OnDisable()
        {
            ImageTrackingManager.Instance.DetachTrackingHandler(this);
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
            EnterAR();
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

                if (trackedImage != null && sessionOrigin != null && imageNameLocator != null)
                {
                    if (imageNameLocator.ImageTargetPositionsLookup != null && imageNameLocator.ImageTargetPositionsLookup.ContainsKey(trackedImage.referenceImage.name))
                    {
                        // Get image location from Tracked Image name
                        targetLocationToBeUsed = imageNameLocator.ImageTargetPositionsLookup[trackedImage.referenceImage.name];

                        // Create the content position
                        var targetLocation = new GameObject();
                        targetLocation.transform.position = targetLocationToBeUsed.position;
                        targetLocation.transform.rotation = targetLocationToBeUsed.rotation;

                        // Reset session origin
                        sessionOrigin.transform.position = Vector3.zero;
                        sessionOrigin.transform.rotation = Quaternion.identity;

                        // On a horizontal surface
                        if (Mathf.Abs(Vector3.Dot(new Vector3(1, 0, 1), targetLocation.transform.eulerAngles)) < 2f)
                        {
                            var newRotation = Quaternion.Euler(new Vector3(
                                targetLocation.transform.eulerAngles.x, trackedImage.transform.eulerAngles.y, targetLocation.transform.eulerAngles.z));
                            sessionOrigin.MakeContentAppearAt(targetLocation.transform, trackedImage.transform.position, newRotation);
                        }
                        // On a vertical surface
                        else if (targetLocation.transform.forward.y > 0.95f)
                        {
                            // Rotate into the vertical plane
                            sessionOrigin.transform.rotation = Quaternion.AngleAxis(90, targetLocation.transform.right) * targetLocation.transform.rotation;
                            // Move position
                            sessionOrigin.MakeContentAppearAt(targetLocation.transform, trackedImage.transform.position);
                            // Adjustment for being to the left or right of the target
                            var adjustment = targetLocation.transform.rotation * Quaternion.Inverse(trackedImage.transform.rotation);
                            // Figure out if you are to the left or right and then make the adjustment
                            if (Vector3.Dot(adjustment.eulerAngles, targetLocation.transform.forward) < 90f)
                            {
                                sessionOrigin.transform.rotation = Quaternion.AngleAxis(Vector3.Dot(adjustment.eulerAngles, targetLocation.transform.forward), Vector3.up) * sessionOrigin.transform.rotation;
                            }
                            else
                            {
                                sessionOrigin.transform.rotation = Quaternion.AngleAxis(Vector3.Dot(adjustment.eulerAngles, targetLocation.transform.forward) - 360f, Vector3.up) * sessionOrigin.transform.rotation;
                            }
                        }
                        // Don't need the target location holder any longer
                        GameObject.Destroy(targetLocation);

                        // Allow camera to see content if it was masked
                        if (aRCamera != null && aRCamera.cullingMask == 1 << LayerMask.NameToLayer("UI"))
                        {
                            aRCamera.cullingMask = showCameraMask;
                        }
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
                // Do not show content, only UI. Could look into masking the raycast as well.
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

        // Enter AR mode, reset session and start listening for tracking changes
        void EnterAR()
        {
            // Clean detach so no session resets call lost tracking if we were already in AR mode
            ImageTrackingManager.Instance.DetachTrackingHandler(this);

            HandleUI(true);
            if (aRController != null)
                aRController.enabled = false;

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

        // Leave AR mode
        void ExitAR()
        {
            if (inARImageTracking)
            {
                // Stop listening
                ImageTrackingManager.Instance.DetachTrackingHandler(this);
                HandleUI(false);

                // Turn model view on
                if (screenMode != null)
                    screenMode.SetActive(true);

                // Set camera to show nothing
                if (aRCamera != null)
                {
                    aRCamera.cullingMask = 0;
                }
                // Disable the AR Session
                if (aRSession != null)
                {
                    aRSession.enabled = false;
                }

                inARImageTracking = false;
            }
        }

        // Enable and disable UI elements as we enter and leave AR mode
        void HandleUI(bool goingIntoAR)
        {
            if (goingIntoAR)
            {
                if (resetButton != null)
                    resetButton.gameObject.SetActive(true);
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(true);
                if (targetHelper != null)
                    targetHelper.SetActive(true);
            }
            else
            {
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(false);
                if (resetButton != null)
                    resetButton.gameObject.SetActive(false);
                if (targetHelper != null)
                    targetHelper.SetActive(false);
            }
        }
    }
}