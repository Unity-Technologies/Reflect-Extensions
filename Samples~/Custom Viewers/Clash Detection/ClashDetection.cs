using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Highlight the clash detections between two categories.
    /// </summary>
    /// <remarks>Right clicking on script in editor allows to run this from the context menu "Check for Clashes".</remarks>
    public class ClashDetection : MonoBehaviour, IObserveReflectRoot
    {
        [Header("For Runtime Detection")]
        [Tooltip("First category to use for clash detection.")]
        [SerializeField] Dropdown firstCategory = default;
        [Tooltip("Second category to use for clash detection.")]
        [SerializeField] Dropdown secondCategory = default;
        [Tooltip("Material to use for highlighting clashes.")]
        [SerializeField] Material highlightMaterial = default;
        [Tooltip("Button in the menu to enter clash detection.")]
        [SerializeField] Button clashButton = default;

        [Header("Detection in Editor (Right-click on script)")]
        [Tooltip("First category to use for clash detection.")]
        [SerializeField] string clashCategory1 = default;
        [Tooltip("Second category to use for clash detection.")]
        [SerializeField] string clashCategory2 = default;
        [Tooltip("The objects that match category 1 when run in the editor.")]
        [SerializeField] List<GameObject> filteredObjects1 = default;
        [Tooltip("The objects that match category 2 when run in the editor.")]
        [SerializeField] List<GameObject> filteredObjects2 = default;
        [Tooltip("The objects that result in clashing when run in the editor.")]
        [SerializeField] List<GameObject> ClashingObjects = default;

        Dictionary<string, List<GameObject>> categoryLookup;
        List<GameObject> Highlights = new List<GameObject>();
        bool foundParameter;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch("Category", ReflectMetadataManager.Instance.AnyValue, false));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        void Start()
        {
            if (firstCategory != null && secondCategory != null)
            {

            }
        }

        /// <summary>
        /// Starts the clash detection process. Call this from a button or something similar.
        /// </summary>
        public void SetName()
        {
            if (NullChecks())
            {
                if (FilterObjects())
                    CheckForClashes();
            }
        }

        // For running this in the editor
        [ContextMenu("Check for Clashes")]
        void RunInEditor()
        {
            if (Highlights == null)
                Highlights = new List<GameObject>();
            filteredObjects1 = new List<GameObject>();
            filteredObjects2 = new List<GameObject>();
            Metadata[] metas = FindObjectsOfType<Metadata>();

            foreach (var meta in metas)
            {
                if (meta.GetParameter("Category") == clashCategory1)
                {
                    if (!filteredObjects1.Contains(meta.gameObject))
                        filteredObjects1.Add(meta.gameObject);
                }
                else if (meta.GetParameter("Category") == clashCategory2)
                {
                    if (!filteredObjects2.Contains(meta.gameObject))
                        filteredObjects2.Add(meta.gameObject);
                }
            }

            CheckForClashes();
        }

        void CheckForClashes()
        {
            ClashingObjects = new List<GameObject>();

            foreach (var filteredObjects1 in filteredObjects1)
            {
                foreach (var filteredObjects2 in filteredObjects2)
                {
                    if (filteredObjects1.GetComponent<Renderer>().bounds.Intersects(
                        filteredObjects2.GetComponent<Renderer>().bounds))
                    {
                        ClashingObjects.Add(filteredObjects1);
                        ClashingObjects.Add(filteredObjects2);
                    }
                }
            }

            HighlightClashes();
        }


        void HighlightClashes()
        {
            foreach (var item in Highlights)
            {
                GameObject.DestroyImmediate(item);
            }

            Highlights = new List<GameObject>();

            foreach (var item in ClashingObjects)
            {
                Bounds itemBounds = item.GetComponent<Renderer>().bounds;
                GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                highlight.transform.SetParent(item.transform);
                highlight.transform.position = itemBounds.center;
                highlight.transform.localScale = new Vector3(
                    itemBounds.size.x * 1.01f, itemBounds.size.y * 1.01f, itemBounds.size.z * 1.01f);
                highlight.GetComponent<MeshRenderer>().material = highlightMaterial;

                Highlights.Add(highlight);
            }
        }

        void FillDropdown()
        {
            if (firstCategory != null && secondCategory != null && categoryLookup != null)
            {
                if (categoryLookup.Keys.Count > 0)
                {
                    firstCategory.ClearOptions();
                    firstCategory.AddOptions(categoryLookup.Keys.ToList());
                    secondCategory.ClearOptions();
                    secondCategory.AddOptions(categoryLookup.Keys.ToList());
                }
            }
        }

        bool NullChecks()
        {
            if (firstCategory != null && secondCategory != null && categoryLookup != null)
            {
                if (!string.IsNullOrEmpty(firstCategory.options[firstCategory.value].text) &&
                    !string.IsNullOrEmpty(secondCategory.options[secondCategory.value].text))
                {
                    return true;
                }
            }
            return false;
        }

        bool FilterObjects()
        {
            if (categoryLookup.ContainsKey(firstCategory.options[firstCategory.value].text) && 
                categoryLookup.ContainsKey(secondCategory.options[secondCategory.value].text))
            {
                filteredObjects1 = new List<GameObject>(categoryLookup[firstCategory.options[firstCategory.value].text]);
                filteredObjects2 = new List<GameObject>(categoryLookup[secondCategory.options[secondCategory.value].text]);
                return true;
            }
            return false;
        }

        void MakeButtonNotInteractable()
        {
            if (clashButton != null)
            {
                clashButton.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (clashButton != null)
            {
                clashButton.interactable = true;
            }
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            categoryLookup = new Dictionary<string, List<GameObject>>();
            MakeButtonNotInteractable();
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
                if (categoryLookup.ContainsKey(result))
                {
                    var thisList = categoryLookup[result];
                    if (!thisList.Contains(reflectObject))
                        categoryLookup[result].Add(reflectObject);
                }
                else
                {
                    categoryLookup.Add(result, new List<GameObject> { reflectObject });
                }
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        {
            if (foundParameter)
            {
                FillDropdown();
                MakeButtonInteractable();
            }
        }
        #endregion
    }
}