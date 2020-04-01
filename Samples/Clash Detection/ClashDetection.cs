using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    public class ClashDetection : MonoBehaviour, IObserveReflectRoot
    {
        public InputField inputField1;
        public InputField inputField2;
        public Material highlightMaterial;

        public string clashCategory1;
        public string clashCategory2;
        public List<GameObject> filteredObjects1;
        public List<GameObject> filteredObjects2;
        public List<GameObject> ClashingObjects;

        List<GameObject> Highlights;

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
            Highlights = new List<GameObject>();
        }

        public void SetName()
        {
            clashCategory1 = inputField1.text;
            clashCategory2 = inputField2.text;
            CheckClashes();
        }

        [ContextMenu("Check for Clashes")]
        void RunInEditor()
        {
            filteredObjects1 = new List<GameObject>();
            filteredObjects2 = new List<GameObject>();
            ClashingObjects.Clear();
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

            CheckClashes();
        }

        public void CheckClashes()
        {
            //ClashingObjects.Clear();
            //Metadata[] metadatas = FindObjectsOfType<Metadata>();
            //filteredObjects1 = MetadataUtilities.FilterbyCategory(metadatas, clashCategory1);
            //filteredObjects2 = MetadataUtilities.FilterbyCategory(metadatas, clashCategory2);

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
            
            Highlights.Clear();
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

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            filteredObjects1 = new List<GameObject>();
            filteredObjects2 = new List<GameObject>();
            ClashingObjects.Clear();
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
                Debug.Log(reflectObject);
                if (clashCategory1 == result && !filteredObjects1.Contains(reflectObject))
                    filteredObjects1.Add(reflectObject);
                else if (clashCategory2 == result && !filteredObjects2.Contains(reflectObject))
                    filteredObjects2.Add(reflectObject);
            }
        }

        /// <summary>
        /// What to do after finishing the search on Metatdata components
        /// </summary>
        public void NotifyAfterSearch()
        { }
        #endregion
    }
}