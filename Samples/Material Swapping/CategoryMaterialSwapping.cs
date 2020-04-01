using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Reflect.Extensions
{
    public class CategoryMaterialSwapping : MonoBehaviour, IObserveReflectRoot
    {
        public string categoryToPaint;
        public Material newMaterial;
        List<GameObject> filteredObjects;

        void OnEnable()
        {
            ReflectMetadataManager.Instance.Attach(this, new MetadataSearch("Category", categoryToPaint, false));
        }

        void OnDisable()
        {
            ReflectMetadataManager.Instance.Detach(this);
        }

        [ContextMenu("Override Materials")]
        void RunInEditor()
        {
            filteredObjects = new List<GameObject>();
            Metadata[] metas = FindObjectsOfType<Metadata>();

            foreach (var meta in metas)
            {
                if (meta.GetParameter("Category") == categoryToPaint)
                {
                    if (!filteredObjects.Contains(meta.gameObject))
                        filteredObjects.Add(meta.gameObject);
                }
            }
            SwapMaterialByCategory();
        }

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

        #region IObserveReflectRoot implementation
        /// <summary>
        /// What to do before searching Metadata components
        /// </summary>
        public void NotifyBeforeSearch()
        {
            filteredObjects = new List<GameObject>();
        }

        /// <summary>
        /// What to do when a Metadata parameter is found
        /// </summary>
        /// <param name="reflectObject">The GameObject with the matching Metadata search pattern</param>
        /// <param name="result">The value of the found parameter in the Metadata component</param>
        public void NotifyReflectRootObservers(GameObject reflectObject, string result = null)
        {
            if (reflectObject != null)
            {
                if (!filteredObjects.Contains(reflectObject))
                    filteredObjects.Add(reflectObject);
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