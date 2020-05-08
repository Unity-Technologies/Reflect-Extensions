namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Top menu for turning on AR mode to use table top image targets by name which uses the TableTopImageTrackingHandler Handler
    /// </summary>
    public class TableTopImageTargetARTopMenu : TopMenu, IObserveReflectRoot
    {
        [Tooltip("The Table Top Image Tracking Handler component to use.")]
        [SerializeField] TableTopImageTrackingHandler tableTopImageTrackingHandler = default;
        [Tooltip("The Metadata parameter to search on.")]
        [SerializeField] string modelSearchParamter = "Family";
        [Tooltip("The value of the Metadata parameter to use.")]
        [SerializeField] string parameterValue = "Floor";
        bool activated;
        Vector3 movePosition;

        new void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(modelSearchParamter))
            {
                Debug.LogWarningFormat("Model Search Parameter is empty on {0}. Using default value of Family.", this);
                modelSearchParamter = "Family";
            }
            if (string.IsNullOrEmpty(parameterValue))
            {
                Debug.LogWarningFormat("Parameter Value is empty on {0}. Using default value of Floor.", this);
                parameterValue = "Floor";
            }
        }

        void OnEnable()
        {
            OnVisiblityChanged += CheckVisibility;
            ImageTrackingManager.Instance.InitialARCapabilityCheck += ARCapabilty;
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch(modelSearchParamter, parameterValue, true));
        }

        void OnDisable()
        {
            OnVisiblityChanged -= CheckVisibility;
            ImageTrackingManager.Instance.InitialARCapabilityCheck -= ARCapabilty;
            ReflectMetadataManager.Instance.Detach(this);
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
        }

        void MakeButtonNotInteractable()
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (button != null)
            {
                button.interactable = true;
            }
        }

        /// <summary>
        /// If button is clicked hide or show appropriately and call disabling/enabling methods for AR
        /// </summary>
        public override void OnClick()
        {
            if (activated)
            {
                Deactivate();
            }
            else
            {
                if (tableTopImageTrackingHandler != null && tableTopImageTrackingHandler.enabled && !tableTopImageTrackingHandler.InARImageTracking)
                {
                    // Stop AR mode in case some other tracking is going on
                    ImageTrackingManager.Instance.StopARMode();
                    // Start AR and locate the target in a smart place
                    tableTopImageTrackingHandler.StartHandlingAR();
                    ImageTrackingManager.Instance.RelocateImageTarget(movePosition);
                }
                Activate();
            }
        }

        /// <summary>
        /// Force exiting of this menu (e.g. Exit AR button)
        /// </summary>
        public void Exit()
        {
            if (tableTopImageTrackingHandler != null && tableTopImageTrackingHandler.enabled)
            {
                ImageTrackingManager.Instance.StopARMode();
            }
            activated = true;
            OnClick();
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            MakeButtonNotInteractable();
            // Default value if nothing is found
            movePosition = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// What to do when a Metadata parameter is found
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            if (reflectObject != null)
            {
                // Move the target to bottom of the floor
                if (reflectObject.GetComponent<Renderer>() != null)
                {
                    movePosition = reflectObject.GetComponent<Renderer>().bounds.center -
                        new Vector3(0, reflectObject.GetComponent<Renderer>().bounds.extents.y, 0);
                }
                else if (reflectObject.GetComponent<MeshFilter>() != null)
                {
                    movePosition = reflectObject.GetComponent<MeshFilter>().mesh.bounds.center -
                        new Vector3(0, reflectObject.GetComponent<MeshFilter>().mesh.bounds.extents.y, 0);
                }
                if (tableTopImageTrackingHandler != null && tableTopImageTrackingHandler.enabled)
                    MakeButtonInteractable();
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch() { }
        #endregion
    }
}