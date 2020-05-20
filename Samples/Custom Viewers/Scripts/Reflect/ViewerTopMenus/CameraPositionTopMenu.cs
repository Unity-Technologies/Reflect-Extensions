namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Camera Position and BIM selection button top menu
    /// </summary>
    public class CameraPositionTopMenu : TopMenu
    {
        [Tooltip("The Camera Selection Top Menu component.")]
        [SerializeField] CameraSelectionMenu cameraSelectionMenu = default;
        [Tooltip("The BIM Selection Top Menu component.")]
        [SerializeField] BIMSelectionMenu bimSelectionMenu = default;
        [Tooltip("The Room Floor Image Tracking Handler component.")]
        [SerializeField] RoomFloorImageTrackingHandler roomFloorImageTrackingHandler = default;

        bool activated;

        void OnEnable()
        {
            OnVisiblityChanged += CheckVisibility;
        }

        void OnDisable()
        {
            OnVisiblityChanged -= CheckVisibility;
        }

        new void Start()
        {
            if (cameraSelectionMenu == null)
            {
                cameraSelectionMenu = GetComponent<CameraSelectionMenu>();
                if (cameraSelectionMenu == null)
                    Debug.LogWarning("If you intend on using Camera Selection, you need to add a CameraSelectionMenu to the empty field on " + this);
            }
            if (bimSelectionMenu == null)
            {
                bimSelectionMenu = GetComponent<BIMSelectionMenu>();
                if (bimSelectionMenu == null)
                    Debug.LogWarning("If you intend on using BIM Selection, you need to add a BIMSelectionMenu to the empty field on " + this);
            }
            base.Start();
        }

        // Use Top Menu's visibilty event to know if the button is activated or not
        void CheckVisibility(bool visible)
        {
            activated = visible;
        }

        /// <summary>
        /// If button is clicked hide or show appropriately and call disabling/enabling methods on the different menus
        /// </summary>
        public override void OnClick()
        {
            if (activated)
            {
                // Returns control back to the 3D view camera controller
                if (cameraSelectionMenu != null)
                    cameraSelectionMenu.ReturnCameraControl();
                // Clear the scoll list and stop looking for touches/clicks
                if (bimSelectionMenu != null)
                    bimSelectionMenu.DisableAndReset();
                Deactivate();
            }
            else
            {
                // Stop AR mode in case some other tracking is going on
                if (roomFloorImageTrackingHandler != null && roomFloorImageTrackingHandler.enabled && !roomFloorImageTrackingHandler.InARImageTracking)
                    ImageTrackingManager.Instance.StopARMode();
                // Disable the 3D view camera controller and add or enable the Rotate Only Camera
                if (cameraSelectionMenu != null)
                    cameraSelectionMenu.GiveCameraControl();
                // To be able to signal to look for touches and clicks after disabling and resetting
                if (bimSelectionMenu != null)
                    bimSelectionMenu.StartLookingForHits();
                // Start listening for AR
                if (roomFloorImageTrackingHandler != null && roomFloorImageTrackingHandler.enabled && !roomFloorImageTrackingHandler.InARImageTracking)
                    roomFloorImageTrackingHandler.StartHandlingAR();
                Activate();
            }
        }

        /// <summary>
        /// Force exiting of this menu (e.g. Exit AR button)
        /// </summary>
        public void Exit()
        {
            if (roomFloorImageTrackingHandler != null && roomFloorImageTrackingHandler.enabled)
            {
                ImageTrackingManager.Instance.StopARMode();
            }
            activated = true;
            OnClick();
        }
    }
}