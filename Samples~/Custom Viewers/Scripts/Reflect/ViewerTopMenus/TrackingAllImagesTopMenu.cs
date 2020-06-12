namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Top menu for turning on AR mode to search for image targets by name which uses the ImageNameTracking Handler
    /// </summary>
    public class TrackingAllImagesTopMenu : TopMenu
    {
        [Tooltip("The Image Name Tracking Handler component.")]
        [SerializeField] ImageNameTrackingHandler imageNameTrackingHandler = default;

        bool activated;

        void OnEnable()
        {
            OnVisiblityChanged += CheckVisibility;
            ImageTrackingManager.Instance.InitialARCapabilityCheck += ARCapabilty;
        }

        void OnDisable()
        {
            OnVisiblityChanged -= CheckVisibility;
            ImageTrackingManager.Instance.InitialARCapabilityCheck -= ARCapabilty;
        }

        // Use Top Menu's visibilty event to know if the button is activated or not
        void CheckVisibility(bool visible)
        {
            activated = visible;
        }

        // Will be visible only if AR is supported
        void ARCapabilty(bool arSupported)
        {
            gameObject.SetActive(arSupported);
            if (imageNameTrackingHandler == null || !imageNameTrackingHandler.enabled)
            {
                if (button != null)
                    button.interactable = false;
            }
        }

        /// <summary>
        /// If button is clicked hide or show appropriately and call disabling/enabling methods on the different menus
        /// </summary>
        public override void OnClick()
        {
            if (activated)
            {
                Deactivate();
            }
            else
            {
                if (imageNameTrackingHandler != null && imageNameTrackingHandler.enabled && !imageNameTrackingHandler.InARImageTracking)
                {
                    // Stop AR mode in case some other tracking is going on
                    ImageTrackingManager.Instance.StopARMode();
                    // Start listening for AR
                    imageNameTrackingHandler.StartHandlingAR();
                }
                Activate();
            }
        }

        /// <summary>
        /// Force exiting of this menu (e.g. Exit AR button)
        /// </summary>
        public void Exit()
        {
            if (imageNameTrackingHandler != null && imageNameTrackingHandler.enabled)
            {
                ImageTrackingManager.Instance.StopARMode();
            }
            activated = true;
            OnClick();
        }
    }
}