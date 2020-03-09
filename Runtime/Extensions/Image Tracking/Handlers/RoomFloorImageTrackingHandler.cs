using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Example of handling AR image tracking in the scene - what to do when tracking is found and lost, when entering and exiting AR, how to handle UI elements, etc.
    /// This particular example uses an image on the floor with ability to add dimensions from one of the directional walls in a rectangular room.
    /// </summary>
    /// <remarks>The System.ComponentModel.Description attribute is used to describe this handler in the Image Tracking Manager editor.
    /// Start this Image Tacking Handler by calling the StartHandlingAR public method.</remarks>
#if UNITY_EDITOR
    [System.ComponentModel.Description("Room Scale Floor Image Tracking")]
#endif
    public class RoomFloorImageTrackingHandler : MonoBehaviour, IHandleImageTargets, ILocateImageTargets
    {
        [Tooltip("Button to leave AR mode.")]
        [SerializeField] Button leaveARButton = default;
        [Tooltip("Button to reset tracking.")]
        [SerializeField] Button resetTrackingButton = default;
        [Tooltip("Gameobject with the Toggle to switch to viewing in AR.")]
        [SerializeField] GameObject viewInARToggle = default;
        [Tooltip("Gameobject with the Toggle to switch to entering manual dimensions.")]
        [SerializeField] GameObject dimensionsToggle = default;
        [Tooltip("Gameobject with the dimensions UI elements.")]
        [SerializeField] GameObject dimensionsGameObject = default;
        [Tooltip("Input Field for east wall dimensions.")]
        [SerializeField] InputField eastWallDimension = default;
        [Tooltip("Input Field for south wall dimensions.")]
        [SerializeField] InputField southWallDimension = default;
        [Tooltip("Input Field for west wall dimensions.")]
        [SerializeField] InputField westWallDimension = default;
        [Tooltip("Input Field for north wall dimensions.")]
        [SerializeField] InputField northWallDimension = default;
        [Tooltip("The Gameobject containing the Model View camera.")]
        [SerializeField] GameObject screenMode = default;
        [Tooltip("The Gameobject containing the AR camera and session origin.")]
        [SerializeField] GameObject aRMode = default;
        [Tooltip("The Gameobject containing the VR camera and canvas.")]
        [SerializeField] GameObject vRMode = default;
        [Tooltip("The AR Session - there should only be one.")]
        [SerializeField] ARSession aRSession = default;
        [Tooltip("The AR Table Top Camera Controller component used for manipulating the AR camera in table top mode.")]
        [SerializeField] ARTableTopCameraController aRController = default;
        [Tooltip("The UI Gameobject with the find image target helper.")]
        [SerializeField] GameObject targetHelper = default;
        [Tooltip("The AR Session Origin to be used by this tracking handler.")]
        [SerializeField] ARSessionOrigin sessionOrigin = default;
        [Tooltip("The AR Camera to be used for this tracking handler.")]
        [SerializeField] Camera aRCamera = default;
        /// <summary>
        /// The AR Camera to be used for this tracking handler
        /// </summary>
        /// <value>AR Camera</value>
        public Camera ArCamera { get => aRCamera; }
        /// <summary>
        /// If currently in AR
        /// </summary>
        /// <value>True if in AR, false otherwise</value>
        public bool InARImageTracking { get => inARImageTracking; }
        int initialCameraMask;
        Vector3 targetLocationToBeUsed;
        float xPos, zPos;
        bool inARImageTracking;

        void Start()
        {
            // Save initial values
            if (aRCamera != null)
                initialCameraMask = aRCamera.cullingMask;
        }

        void OnEnable()
        {
            ImageTrackingManager.Instance.InitialARCapabilityCheck += CheckARAvailability;
        }

        void OnDisable()
        {
            ImageTrackingManager.Instance.DetachTrackingHandler(this);
            ImageTrackingManager.Instance.DetachLocater(this);
            ImageTrackingManager.Instance.InitialARCapabilityCheck -= CheckARAvailability;
        }

        void CheckARAvailability(bool supported)
        {
            // Turn on/off toggle depending if AR is supported
            if (viewInARToggle != null)
                viewInARToggle.SetActive(supported);
            if (dimensionsToggle != null)
                dimensionsToggle.SetActive(supported);
        }

        #region IHandleImageTargets implementation
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
            if (ImageTrackingManager.Instance.ARSupported)
                ImageTrackingManager.Instance.DetachLocater(this);
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
                    //Reset the session origin
                    sessionOrigin.transform.rotation = Quaternion.identity;
                    sessionOrigin.transform.position = Vector3.zero;

                    // Create the content position
                    var targetLocation = new GameObject();
                    targetLocation.transform.position = targetLocationToBeUsed;
                    var newRotation = Quaternion.Euler(new Vector3(
                        sessionOrigin.transform.eulerAngles.x, trackedImage.transform.localEulerAngles.y, sessionOrigin.transform.eulerAngles.z));
                    // Make it appear and then destroy the temporary location object
                    sessionOrigin.MakeContentAppearAt(targetLocation.transform, trackedImage.transform.position, newRotation);
                    GameObject.Destroy(targetLocation);

                    // Allow camera to see content if it was masked
                    if (aRCamera != null && aRCamera.cullingMask == 1 << LayerMask.NameToLayer("UI"))
                    {
                        aRCamera.cullingMask = initialCameraMask;
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
            if (aRMode != null)
                aRMode.SetActive(true);
            if (screenMode != null)
                screenMode.SetActive(false);
            if (vRMode != null)
                vRMode.SetActive(false);
            if (aRSession != null)
                aRSession.Reset();
            inARImageTracking = true;
            // Call this manually to mask the camera
            LostTracking(null);
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
                if (aRMode != null)
                    aRMode.SetActive(false);
                if (screenMode != null)
                    screenMode.SetActive(true);
                if (vRMode != null)
                    vRMode.SetActive(true);

                // Set camera back to what its initial culling mask if it was changed
                if (aRCamera != null && aRCamera.cullingMask == 1 << LayerMask.NameToLayer("UI"))
                {
                    aRCamera.cullingMask = initialCameraMask;
                }

                if (aRSession != null)
                    aRSession.Reset();

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
                if (!CalculateTargetPosition(_bounds))
                {
                    // Use the center of the bounds if there is no dimensional input
                    targetLocationToBeUsed = _bounds.center + new Vector3(0, _bounds.extents.y, 0);
                }
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

        // Enable and disable UI elements as we enter and leave AR mode
        void HandleUI(bool goingIntoAR)
        {
            if (goingIntoAR)
            {
                if (viewInARToggle != null)
                    viewInARToggle.SetActive(false);
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(true);
                if (resetTrackingButton != null)
                    resetTrackingButton.gameObject.SetActive(true);
                if (targetHelper != null)
                    targetHelper.SetActive(true);
                if (dimensionsToggle != null)
                    dimensionsToggle.SetActive(false);
                if (dimensionsGameObject != null)
                    dimensionsGameObject.SetActive(false);
                // Clear out dimension fields so they are not used by mistake
                if (eastWallDimension != null)
                    eastWallDimension.text = "";
                if (westWallDimension != null)
                    westWallDimension.text = "";
                if (northWallDimension != null)
                    northWallDimension.text = "";
                if (southWallDimension != null)
                    southWallDimension.text = "";
            }
            else
            {
                if (leaveARButton != null)
                    leaveARButton.gameObject.SetActive(false);
                if (resetTrackingButton != null)
                    resetTrackingButton.gameObject.SetActive(false);
                if (viewInARToggle != null)
                    viewInARToggle.SetActive(ImageTrackingManager.Instance.ARSupported);
                if (targetHelper != null)
                    targetHelper.SetActive(false);
                if (dimensionsToggle != null)
                    dimensionsToggle.SetActive(ImageTrackingManager.Instance.ARSupported);
                if (dimensionsGameObject != null && dimensionsToggle != null && dimensionsToggle.activeSelf &&
                        dimensionsToggle.GetComponent<Toggle>() != null && dimensionsToggle.GetComponent<Toggle>().isOn)
                    dimensionsGameObject.SetActive(true);
            }
        }

        // Checking to see if dimensions were provided. Major assumptions here are this is a rectangular room and floor extends to walls
        bool CalculateTargetPosition(Bounds _floor)
        {
            if (eastWallDimension != null && !string.IsNullOrEmpty(eastWallDimension.text) &&
                !Mathf.Approximately(0f, float.Parse(eastWallDimension.text)))
            {
                float dimension = float.Parse(eastWallDimension.text);
                xPos = _floor.max.x - dimension;
            }
            else if (westWallDimension != null && !string.IsNullOrEmpty(westWallDimension.text) &&
                !Mathf.Approximately(0f, float.Parse(westWallDimension.text)))
            {
                float dimension = float.Parse(westWallDimension.text);
                xPos = _floor.min.x + dimension;
            }
            else
            {
                // Invalid input
                return false;
            }

            if (northWallDimension != null && !string.IsNullOrEmpty(northWallDimension.text) &&
                !Mathf.Approximately(0f, float.Parse(northWallDimension.text)))
            {
                float dimension = float.Parse(northWallDimension.text);
                zPos = _floor.max.z - dimension;
            }
            else if (southWallDimension != null && !string.IsNullOrEmpty(southWallDimension.text) &&
                !Mathf.Approximately(0f, float.Parse(southWallDimension.text)))
            {
                float dimension = float.Parse(southWallDimension.text);
                zPos = _floor.min.z + dimension;
            }
            else
            {
                // Invalid input
                return false;
            }

            // Use provided dimensions to locate target
            targetLocationToBeUsed = new Vector3(xPos, _floor.center.y + _floor.extents.y, zPos);
            return true;
        }
    }
}