using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

public class ClashDetection : MonoBehaviour
{
    public InputField inputField1;
    public InputField inputField2;
    public Material highlightMaterial;

    public string clashCategory1;
    public string clashCategory2;
    public GameObject[] FilteredObjects1;
    public GameObject[] FilteredObjects2;
    public List<GameObject> ClashingObjects;

    List<GameObject> Highlights;

    // Start is called before the first frame update
    void Start()
    {
        Highlights = new List<GameObject>();
        StartCoroutine(RepeatCheck());
    }

    public void SetName()
    {
        clashCategory1 = inputField1.text;
        clashCategory2 = inputField2.text;
        Debug.Log(clashCategory1);
        Debug.Log(clashCategory2);

        StartCoroutine(RepeatCheck());
    }

    [ContextMenu("Check for Clashes")]
    public void CheckClashes()
    {
        ClashingObjects.Clear();
        Metadata[] metadatas = FindObjectsOfType<Metadata>();
        FilteredObjects1 = MetadataUtilities.FilterbyCategory(metadatas, clashCategory1);
        FilteredObjects2 = MetadataUtilities.FilterbyCategory(metadatas, clashCategory2);

        foreach (var filteredObjects1 in FilteredObjects1)
        {
            foreach (var filteredObjects2 in FilteredObjects2)
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


    private void HighlightClashes()
    {
        foreach (var item in Highlights)
        {
            GameObject.DestroyImmediate(item);
        }        {
        Highlights.Clear();
        }
        Highlights = new List<GameObject>();

        foreach (var item in ClashingObjects)
        {
            Bounds itemBounds = item.GetComponent<Renderer>().bounds;
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlight.transform.SetParent(item.transform);
            highlight.transform.position = itemBounds.center;
            highlight.transform.localScale = new Vector3(
                itemBounds.size.x*1.01f, itemBounds.size.y*1.01f, itemBounds.size.z*1.01f);
            highlight.GetComponent<MeshRenderer>().material = highlightMaterial;

            Highlights.Add(highlight);
        }
    }

    IEnumerator RepeatCheck()
    {
        yield return new WaitForSeconds(2f);
        CheckClashes(); 
    }
}
