using UnityEngine;
using UnityEditor;
using UnityEngine.Reflect;
using System.Linq;
using MenuItem = UnityEditor.MenuItem; // UnityEngine.Reflect contains a definition for MenuItem. This avoids ambiguity.

// Putting project classes in namespaces makes for clean structure and avoids classnames collisions.
namespace Reflect.Extenstions.EditorSamples.MetadataTools
{
    public class MetadataSearchAndInstantiateWindow : EditorWindow
    {
        string md_key = "Category";
        string md_value = "Planting";

        Metadata[] metadatas = new Metadata[0];

        GameObject replacement = default;
        float minHeight = 2;
        bool matchHeight = default;

        [MenuItem("Reflect/Samples/Search and Instantiate (by Metadata)")]
        static void Init()
        {
            var window = GetWindow<MetadataSearchAndInstantiateWindow>();
            window.titleContent = new GUIContent("Search & Instantiate");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Search", EditorStyles.boldLabel);

            md_key = EditorGUILayout.TextField("Key", md_key);
            md_value = EditorGUILayout.TextField("Value", md_value);

            if (GUILayout.Button("Search"))
            {
                metadatas = Search(md_key, md_value);
            }

            EditorGUILayout.LabelField(string.Format("{0} metadatas with {1} = {2}", metadatas.Length.ToString(), md_key, md_value));

            if (GUILayout.Button("Select"))
            {
                metadatas = Search(md_key, md_value);
                Selection.objects = (from item in metadatas
                                     select item.gameObject).ToArray();
            }

            replacement = (GameObject)EditorGUILayout.ObjectField("Replacement", replacement, typeof(GameObject), false);

            EditorGUI.BeginDisabledGroup(replacement == null);

            matchHeight = EditorGUILayout.Toggle("Match Height", matchHeight);
            minHeight = EditorGUILayout.Slider("Min. Height", minHeight, 0, 10);

            if (GUILayout.Button("Replace"))
            {
                metadatas = Search(md_key, md_value);
                Replace(metadatas, replacement, matchHeight, minHeight);
            }
            EditorGUI.EndDisabledGroup();
        }

        Metadata[] Search(string key, string value)
        {
            return (from item in FindObjectsOfType<Metadata>()
                    where item.parameters.dictionary.ContainsKey(key) && item.parameters.dictionary[key].value == value
                    select item).ToArray();
        }

        void Replace(Metadata[] metadatas, GameObject replacement, bool matchHeight = false, float minHeight = 0)
        {
            int undoLvl = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Replace by Metadata");
            var root = new GameObject("- [ Replacements ] -").transform;
            Undo.RegisterCreatedObjectUndo(root.gameObject, "");
            root.transform.position = Vector3.zero;
            foreach (Metadata md in metadatas)
            {
                if (minHeight > 0 && md.parameters.dictionary.ContainsKey("Height"))
                {
                    var height = float.Parse(md.GetParameter("Height")) * 0.001f;
                    if (height < minHeight)
                        continue;
                }

                var go = (GameObject)PrefabUtility.InstantiatePrefab(replacement, root);
                go.transform.localPosition = md.transform.position;
                go.transform.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);

                if (matchHeight && md.parameters.dictionary.ContainsKey("Height"))
                {
                    var height = float.Parse(md.GetParameter("Height")) * 0.001f;
                    Bounds rpcBounds = new Bounds();
                    foreach (MeshFilter mf in replacement.GetComponentsInChildren<MeshFilter>())
                        rpcBounds.Encapsulate(mf.sharedMesh.bounds);

                    go.transform.localScale = Vector3.one * (height / rpcBounds.size.y);
                }

                Undo.RegisterCreatedObjectUndo(go, "");
            }
            Undo.CollapseUndoOperations(undoLvl);
        }
    }
}