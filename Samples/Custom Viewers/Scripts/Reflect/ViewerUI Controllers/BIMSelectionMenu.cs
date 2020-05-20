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
        Ray ray;
        const float ITEMSPACE = 30f;
        float y; //Menu item placement
        string familyName;
        string manufacturerName;
        string modelName;
        string cost;
        string[] materials;
        bool lookingForHits;
        bool interactivityFound;
        PointerEventData eventDataCurrentPosition;
        List<RaycastResult> results;

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
                        GetMetadata(hit.transform);
                }
            }
        }

        bool IsTouchOverUIObject(Vector2 touch)
        {
            eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = touch;
            results = new List<RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
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
                // Get new data
                familyName = meta.GetParameter("Family");
                manufacturerName = meta.GetParameter("Manufacturer");
                modelName = meta.GetParameter("Model");
                cost = meta.GetParameter("Cost");
                var groups = meta.SortedByGroup();
                if (groups.ContainsKey("Materials and Finishes"))
                {
                    var mats = groups["Materials and Finishes"];
                    if (mats != null && mats.Count > 0)
                    {
                        materials = new string[mats.Count];
                        int i = 0;
                        foreach (KeyValuePair<string, Metadata.Parameter> kvp in mats)
                        {
                            materials[i] = kvp.Value.value;
                            i++;
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

            // Clear the fields
            familyName = string.Empty;
            manufacturerName = string.Empty;
            modelName = string.Empty;
            cost = string.Empty;
            materials = null;
        }

        // Build the scoll menu
        void AddMenuItems()
        {
            // Initialize placement of items
            y = -30f;
            // Turn on scroll view
            scrollView.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(familyName))
            {
                FillMenuItem("Family: " + familyName);
            }
            if (!string.IsNullOrEmpty(manufacturerName))
            {
                FillMenuItem("Manufacturer: " + manufacturerName);
            }
            if (!string.IsNullOrEmpty(modelName))
            {
                FillMenuItem("Model: " + modelName);
            }
            if (!string.IsNullOrEmpty(cost))
            {
                FillMenuItem("Cost: $" + cost);
            }
            if (materials != null)
            {
                FillMenuItem("Materials:");
                foreach (string mat in materials)
                    FillMenuItem(mat, true);
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
                    var coll = reflectObject.gameObject.AddComponent<MeshCollider>();
                    coll.convex = true;
                    interactivityFound = true;
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