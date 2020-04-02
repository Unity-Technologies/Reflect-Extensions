using System.Collections.Generic;
using UnityEngine.UI;

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
        [SerializeField] InputField category1;
        [Tooltip("Second category to use for clash detection.")]
        [SerializeField] InputField category2;
        [Tooltip("Material to use for highlighting clashes.")]
        [SerializeField] Material highlightMaterial;
        [Tooltip("Button in the menu to enter clash detection.")]
        [SerializeField] Button clashButton;

        [Header("Detection in Editor (Right-click on script)")]
        [Tooltip("First category to use for clash detection.")]
        [SerializeField] string clashCategory1;
        [Tooltip("Second category to use for clash detection.")]
        [SerializeField] string clashCategory2;
        [Tooltip("The objects that match category 1 when run in the editor.")]
        [SerializeField] List<GameObject> filteredObjects1;
        [Tooltip("The objects that match category 2 when run in the editor.")]
        [SerializeField] List<GameObject> filteredObjects2;
        [Tooltip("The objects that result in clashing when run in the editor.")]
        [SerializeField] List<GameObject> ClashingObjects;

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

        /// <summary>
        /// Starts the clash detection process. Call this from a button or something similar.
        /// </summary>
        public void SetName()
        {
            if (NullChecks())
            {
                clashCategory1 = category1.text;
                clashCategory2 = category2.text;
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

        bool NullChecks()
        {
            if (category1 != null && category2 != null)
            {
                if (!string.IsNullOrEmpty(category1.text) && !string.IsNullOrEmpty(category2.text))
                    return true;
            }
            return false;
        }

        bool FilterObjects()
        {
            if (categoryLookup.ContainsKey(clashCategory1) && categoryLookup.ContainsKey(clashCategory2))
            {
                filteredObjects1 = new List<GameObject>(categoryLookup[clashCategory1]);
                filteredObjects2 = new List<GameObject>(categoryLookup[clashCategory2]);
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
        public void NotifyReflectRootObservers(GameObject reflectObject, string result = null)
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
                MakeButtonInteractable();
        }
        #endregion
    }
}