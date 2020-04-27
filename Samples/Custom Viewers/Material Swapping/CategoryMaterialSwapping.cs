using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Swaps the material of the entered category to the designated material
    /// </summary>
    /// <remarks>Right clicking on script in editor allows to run this from the context menu "Override Materials".</remarks>
    public class CategoryMaterialSwapping : MonoBehaviour, IObserveReflectRoot
    {
        [Tooltip("Category to swap the material.")]
        [SerializeField] string categoryToSwapMaterial = default;
        [Tooltip("Material to use for category.")]
        [SerializeField] Material newMaterial = default;
        [Tooltip("Button in the menu to enter material swapping.")]
        [SerializeField] Button swapButton = default;

        List<GameObject> filteredObjects;
        bool foundParameter;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch("Category", categoryToSwapMaterial, false));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        // For running this in the editor
        [ContextMenu("Override Materials")]
        void RunInEditor()
        {
            filteredObjects = new List<GameObject>();
            Metadata[] metas = FindObjectsOfType<Metadata>();

            foreach (var meta in metas)
            {
                if (meta.GetParameter("Category") == categoryToSwapMaterial)
                {
                    if (!filteredObjects.Contains(meta.gameObject))
                        filteredObjects.Add(meta.gameObject);
                }
            }
            SwapMaterialByCategory();
        }

        /// <summary>
        /// Starts the material swapping process. Call this from a button or something similar.
        /// </summary>
        public void SwapMaterialByCategory()
        {
            foreach (var filteredObject in filteredObjects)
            {
                MeshRenderer meshRend = filteredObject.GetComponent<MeshRenderer>();

                if (Application.isPlaying)
                {
                    Material[] newMatArray = new Material[meshRend.materials.Length];
                    for (int i = 0; i < newMatArray.Length; i++)
                    {
                        newMatArray[i] = newMaterial;
                    }

                    meshRend.materials = newMatArray;
                }
                else
                {
                    Material[] newMatArray = new Material[meshRend.sharedMaterials.Length];
                    for (int i = 0; i < newMatArray.Length; i++)
                    {
                        newMatArray[i] = newMaterial;
                    }
                    #if UNITY_EDITOR
                    Undo.RecordObject(meshRend, "Override Materials");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(meshRend);
                    #endif
                    meshRend.sharedMaterials = newMatArray;
                }


            }
        }

        void MakeButtonNotInteractable()
        {
            if (swapButton != null)
            {
                swapButton.interactable = false;
            }
        }

        void MakeButtonInteractable()
        {
            if (swapButton != null)
            {
                swapButton.interactable = true;
            }
        }

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            filteredObjects = new List<GameObject>();
            foundParameter = false;
            MakeButtonNotInteractable();
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
                foundParameter = true;
                if (!filteredObjects.Contains(reflectObject))
                    filteredObjects.Add(reflectObject);
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