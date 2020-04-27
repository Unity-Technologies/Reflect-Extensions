using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Creates the camera locations menu using the parameter from Metadata
    /// </summary>
    public class CameraSelectionMenu : MonoBehaviour, IObserveReflectRoot
    {
        [Tooltip("Transform where content will be added to the scroll view.")]
        [SerializeField] RectTransform scrollContent = default;
        [Tooltip("Transform of scroll rect component.")]
        [SerializeField] RectTransform scrollView = default;
        [Tooltip("Transform the menu items that will be added under the scroll content.")]
        [SerializeField] RectTransform menuItem = default;
        [Tooltip("The menu item button in the UI.")]
        [SerializeField] Button cameraButton = default;
        [Tooltip("Camera to move to the selected location.")]
        [SerializeField] Transform cameraToMove = default;
        [Tooltip("UI Toggle component to allow viewing in AR.")]
        [SerializeField] Toggle viewInAR = default;
        [Tooltip("Parameter name to search for in Metadata component.\nIf this parameter is not empty then it will added to the lookup.")]
        [SerializeField] string parameterName = "Camera Location";

        /// <summary>
        /// The camera to move when the a location is selected
        /// </summary>
        /// <value>The transform of the camera</value>
        public Transform CameraToMove { get => cameraToMove; }
        /// <summary>
        /// UI Toggle component to allow viewing in AR
        /// </summary>
        /// <value>The UI toggle component</value>
        public Toggle ViewInAR { get => viewInAR; }
        /// <summary>
        /// Name of the camera location (e.g. room name) and the object transform to use for that location (e.g. floor)
        /// </summary>
        /// <value>The dictionary on which to perform lookups</value>
        public Dictionary<string, Transform> CameraPositionsLookup { get => cameraPositionsLookup; }

        Dictionary<string, Transform> cameraPositionsLookup = new Dictionary<string, Transform>();
        CameraRotateOnlyController cameraRotateOnlyController;
        CameraRotateOnlyController[] cameraRotateOnlyControllers;
        UnityEngine.Reflect.Controller.FreeCamController freeCamController;
        const float ITEMSPACE = 35f;
        float originalScrollHeight;
        bool foundParameter;

        void Start()
        {
            if (scrollView != null)
                originalScrollHeight = scrollView.rect.height;
        }

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch(parameterName, ReflectMetadataManager.Instance.AnyValue, false));
        }

        void OnDisable()
        {
            ReturnCameraControl();
            ReflectMetadataManager.Instance.Detach(this);
        }

        void MakeButtonNotInteractable()
        {
            if (cameraButton != null)
            {
                cameraButton.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (cameraButton != null)
            {
                cameraButton.interactable = true;
            }
        }

        // Remove scroll menu items
        void DisableAndReset()
        {
            if (scrollContent != null)
            {
                foreach (Transform rt in scrollContent.transform)
                {
                    GameObject.Destroy(rt.gameObject);
                }
            }
        }

        /// <summary>
        /// Returns control back to the 3D view camera controller
        /// </summary>
        public void ReturnCameraControl()
        {
            // Set cameras back to original states
            if (cameraRotateOnlyController != null)
                cameraRotateOnlyController.enabled = false;
            if (freeCamController != null)
                freeCamController.enabled = true;
        }

        /// <summary>
        /// Disable the 3D view camera controller and add or enable the Rotate Only Camera
        /// </summary>
        public void GiveCameraControl()
        {
            if (cameraToMove != null && foundParameter)
            {
                freeCamController = cameraToMove.GetComponent<UnityEngine.Reflect.Controller.FreeCamController>();
                if (freeCamController != null)
                    freeCamController.enabled = false;

                if (cameraRotateOnlyController == null)
                {
                    cameraRotateOnlyControllers = cameraToMove.GetComponentsInChildren<CameraRotateOnlyController>(true);
                    if (cameraRotateOnlyControllers == null || cameraRotateOnlyControllers.Length < 1)
                        cameraRotateOnlyController = cameraToMove.gameObject.AddComponent<CameraRotateOnlyController>();
                    else
                        cameraRotateOnlyController = cameraRotateOnlyControllers[0];
                }
                cameraRotateOnlyController.enabled = true;
            }
        }

        // Create the scroll menu
        void AddMenuItems()
        {
            float y = 0;
            foreach (string room in cameraPositionsLookup.Keys)
            {
                var newRoom = NewMenuItem();
                if (newRoom != null)
                {
                    // Adding menu items
                    var menuItem = newRoom.GetComponent<CameraMenuItemSelection>();
                    if (menuItem != null && menuItem.RoomNameText != null)
                        menuItem.RoomNameText.text = room;
                    newRoom.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
                    y -= ITEMSPACE;
                    newRoom.SetActive(true);
                }
            }

            //  Set content scrollable size
            if (scrollContent != null)
                scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, -y);

            if (scrollView != null && originalScrollHeight > -y)
                scrollView.offsetMax = new Vector2(scrollView.offsetMax.x, -(originalScrollHeight + y));
        }

        GameObject NewMenuItem()
        {
            return Instantiate(menuItem.gameObject, scrollContent);
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            MakeButtonNotInteractable();
            DisableAndReset();
            cameraPositionsLookup = new Dictionary<string, Transform>();
            foundParameter = false;
        }

        /// <summary>
        /// What to do when a Metadata parameter is found
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>

        public void NotifyObservers(GameObject reflectObject, string result = null)
        {
            if (reflectObject != null && !string.IsNullOrEmpty(result))
            {
                foundParameter = true;
                // Add the camera location to be displayed in the menu
                if (!cameraPositionsLookup.ContainsKey(result))
                {
                    cameraPositionsLookup.Add(result, reflectObject.transform);
                }
                else
                    Debug.LogWarningFormat("There are duplicate parameter name for Camera Location. Be sure to use unique names for {0} on {1} and {2}.",
                        result, reflectObject.name, cameraPositionsLookup[result]);
            }
        }


        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (foundParameter)
            {
                AddMenuItems();
                // Make button interactable now        
                MakeButtonInteractable();
            }
        }
        #endregion
    }
}