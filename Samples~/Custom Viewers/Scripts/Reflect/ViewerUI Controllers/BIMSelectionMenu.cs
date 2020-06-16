using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Assign colliders to items with Interactable Yes parameter. Displays data when item is touched/clicked.
    /// </summary>
    public class BIMSelectionMenu : MonoBehaviour, IObserveReflectRoot
    {
        [Tooltip("Transform where content will be added to the scroll view.")]
        [SerializeField]
        RectTransform scrollContent = default;
        [Tooltip("Transform of scroll rect component.")]
        [SerializeField]
        RectTransform scrollView = default;
        [Tooltip("Transform the menu items that will be added under the scroll content.")]
        [SerializeField]
        RectTransform menuItem = default;
        [Tooltip("Transform of the button to close the scroll view.")]
        [SerializeField]
        RectTransform closeButton = default;
        [Tooltip("The BIM Selection menu button.")]
        [SerializeField]
        Button menuButton = default;
        [Tooltip("The Room Floor Image Tracking Handler component.")]
        [SerializeField]
        RoomFloorImageTrackingHandler imageTracking = default;
        [Tooltip("Maximum raycast distance from the camera to be able to select an item to show BIM data.")]
        [SerializeField]
        float raycastDistance = 5f;
        [Tooltip("Parameter name to search for in Metadata component.\nIf this parameter's value is Yes then it can be selected.")]
        [SerializeField]
        string parameterName = "Interactable";
        [Tooltip("Show the metadata for any object touched, not just the ones with the matching true Interactable parameter name")]
        [SerializeField]
        bool showEveryObjectTouched = false;
        [Tooltip("BIM Parameter values you want to display in your menu when item is selected.")]
        [SerializeField]
        List<string> bimParametersToShow = default;
        Ray ray;
        const float ITEMSPACE = 30f;
        float y; //Menu item placement
        bool lookingForHits;
        bool interactivityFound;
        PointerEventData eventDataCurrentPosition;
        List<RaycastResult> results;
        List<GameObject> interactableObjects = new List<GameObject>();
        List<string> bimDataToDisplay = new List<string>();

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch(parameterName, "Yes", false));
        }

        void OnDisable()
        {
            DisableAndReset();
            ReflectMetadataManager.Instance.Detach(this);
        }

        // Looking for touches or clicks
        void Update()
        {
            // Get hit data
            if (interactivityFound && lookingForHits && Input.GetMouseButtonDown(0))
            {
                Vector2 touch = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (IsTouchOverUIObject(touch))
                    return;

                if (imageTracking != null && imageTracking.InARImageTracking)
                {
                    if (imageTracking.ArCamera != null)
                        ray = imageTracking.ArCamera.ScreenPointToRay(Input.mousePosition);
                }
                else
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    if (hit.transform != null)
                    {
                        if (showEveryObjectTouched || interactableObjects.Contains(hit.transform.gameObject))
                            GetMetadata(hit.transform);
                    }
                }
            }
        }

        bool IsTouchOverUIObject(Vector2 touch)
        {
            eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = touch;
            results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        // Interactable item touched so get the Metadata
        void GetMetadata(Transform tran)
        {
            Metadata meta = tran.GetComponent<Metadata>();
            if (meta != null)
            {
                // Clear the data and fields
                DisableAndReset();

                // Get new data and it to the list to display
                var possibleGroups = meta.SortedByGroup();
                foreach (string data in bimParametersToShow)
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (possibleGroups.ContainsKey(data))
                        {
                            var groupedData = possibleGroups[data];
                            if (groupedData != null && groupedData.Count > 0)
                            {
                                // Add Header title to list
                                bimDataToDisplay.Add(data);
                                foreach (KeyValuePair<string, Metadata.Parameter> kvp in groupedData)
                                {
                                    bimDataToDisplay.Add("   " + kvp.Value.value);
                                }
                            }
                        }
                        else
                        {
                            string thisData = meta.GetParameter(data);
                            if (!string.IsNullOrEmpty(thisData))
                            {
                                bimDataToDisplay.Add(data+": " + thisData);
                            }
                        }
                    }
                }
            }
            AddMenuItems();

            // Now looking for raycasts
            lookingForHits = true;
        }

        /// <summary>
        /// Signal to start looking for touches and clicks after disabling and resetting
        /// </summary>
        public void StartLookingForHits()
        {
            lookingForHits = true;
        }

        /// <summary>
        /// Clear the scoll list and stop looking for touches/clicks
        /// </summary>
        public void DisableAndReset()
        {
            // Not looking for raycasts
            lookingForHits = false;

            // Remove menu items
            if (scrollContent != null && scrollContent.transform.childCount > 1)
            {
                for (int i = 1; i < scrollContent.transform.childCount; i++)
                {
                    // Don't destroy Title though
                    GameObject.Destroy(scrollContent.transform.GetChild(i).gameObject);
                }
            }

            // Disable the Close Button and Scroll view
            if (closeButton != null)
                closeButton.gameObject.SetActive(false);
            if (scrollView != null)
                scrollView.gameObject.SetActive(false);

            // Clear the BIM display list
            bimDataToDisplay = new List<string>();
        }

        // Build the scoll menu
        void AddMenuItems()
        {
            // Initialize placement of items
            y = -30f;
            // Turn on scroll view
            scrollView.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);

            foreach (string data in bimDataToDisplay)
            {
                FillMenuItem(data);
            }
        }

        void FillMenuItem(string menuItem, bool indent = false)
        {
            var newItem = NewMenuItem();
            if (newItem != null)
            {
                var itemText = newItem.GetComponentInChildren<Text>();
                if (itemText != null)
                {
                    itemText.text = menuItem;
                }
                if (indent)
                    newItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(8f, y);
                else
                    newItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, y);
                y -= ITEMSPACE;
                newItem.SetActive(true);
            }
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
            interactivityFound = false;
            interactableObjects = new List<GameObject>();
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
                // If yes to interactable then add collider
                if (reflectObject.GetComponent<Renderer>() != null)
                {
                    if (reflectObject.gameObject.GetComponent<Collider>() == null)
                        reflectObject.gameObject.AddComponent<MeshCollider>();
                    interactivityFound = true;
                    if (!interactableObjects.Contains(reflectObject))
                        interactableObjects.Add(reflectObject);
                }
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (menuButton != null)
                menuButton.interactable = interactivityFound;
        }
        #endregion
    }
}