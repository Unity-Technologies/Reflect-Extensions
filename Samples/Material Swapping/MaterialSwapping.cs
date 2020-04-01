using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MaterialSwapping : MonoBehaviour
{
    public string categoryToPaint;
    public Material newMaterial;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    [ContextMenu("Override Materials")]
    public void SwapMaterialByCategory()
    {
        GameObject[] filteredObjects = MetadataUtilities.FilterbyCategory(FindObjectsOfType<Metadata>(), categoryToPaint);

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
}
