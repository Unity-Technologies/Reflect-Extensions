using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect;

public class MetadataUtilities : MonoBehaviour
{
    public GameObject[] filteredObjects;
    // Start is called before the first frame update
    void Start()
    {
        Metadata[] metas = FindObjectsOfType<Metadata>();
        filteredObjects = FilterbyCategory(metas, "Floors");
    }


    public static GameObject[] FilterbyCategory(Metadata[] metas, string category)
    {
        List<GameObject> filteredList = new List<GameObject>();

        foreach (var meta in metas)
        {
            if(meta.GetParameter("Category") == category)
            {
                filteredList.Add(meta.gameObject);
            }
        }
        return filteredList.ToArray();
    }
}
