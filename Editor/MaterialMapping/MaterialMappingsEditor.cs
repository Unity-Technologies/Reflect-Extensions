using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Reflect.Extensions.MaterialMapping;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    [CustomEditor(typeof(MaterialMappings))]
    public class MaterialMappingsEditor : Editor
    {
        SerializedProperty enabledProperty;
        SerializedProperty priorityProperty;
        SerializedProperty overwriteProperty;
        SerializedProperty materialRemapsProperty;

        GUIContent emptyLabel = new GUIContent("");
        GUIContent priorityLabel = new GUIContent("Priority");
        GUIContent overwriteLabel = new GUIContent("Overwrite");
        GUIContent enabledLabel = new GUIContent("Enabled");
        GUIStyle boldLabel;

        private void OnEnable()
        {
            enabledProperty = serializedObject.FindProperty("_enabled");
            priorityProperty = serializedObject.FindProperty("_priority");
            overwriteProperty = serializedObject.FindProperty("_overwrite");
            materialRemapsProperty = serializedObject.FindProperty("_materialRemaps");
        }

        public override void OnInspectorGUI()
        {
            if (boldLabel == null)
                boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

            EditorGUILayout.PropertyField(enabledProperty, enabledLabel);
            EditorGUILayout.PropertyField(priorityProperty, priorityLabel);
            EditorGUILayout.PropertyField(overwriteProperty, overwriteLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Material Mappings", boldLabel);
            if (GUILayout.Button("Find", GUILayout.Width(60)))
                FindMaterials();
            if (GUILayout.Button("Sort", GUILayout.Width(60)))
                ((MaterialMappings)target).Sort();
            if (GUILayout.Button("Clean", GUILayout.Width(60)))
                ((MaterialMappings)target).Clean();
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < materialRemapsProperty.arraySize; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                var item = materialRemapsProperty.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("syncMaterialName").stringValue = GUILayout.TextField(item.FindPropertyRelative("syncMaterialName").stringValue, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("remappedMaterial"), emptyLabel, GUILayout.Width(150));
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    materialRemapsProperty.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
            }

            // TODO : highlight name duplicates/conflicts

            if (GUILayout.Button("+"))
                materialRemapsProperty.arraySize++;

            serializedObject.ApplyModifiedProperties();
        }

        void FindMaterials()
        {
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(serializedObject.targetObject));

            var materials = (from item in AssetDatabase.FindAssets("t:Material", new string[2] { "Assets/Materials", path })
                             select AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(item))).ToArray();

            foreach (Material m in materials)
            {
                // bypass Reflect SyncMaterials
                if (m.shader.name.Contains("UnityReflect"))
                    continue;

                bool exists = false;
                for (int i = 0; i < materialRemapsProperty.arraySize; i++)
                {
                    if (materialRemapsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("remappedMaterial").objectReferenceValue == m)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                    continue;

                materialRemapsProperty.arraySize++;
                var item = materialRemapsProperty.GetArrayElementAtIndex(materialRemapsProperty.arraySize - 1);
                item.FindPropertyRelative("syncMaterialName").stringValue = m.name;
                item.FindPropertyRelative("remappedMaterial").objectReferenceValue = m;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// ReflectMaterialRemapperEditor
    /// Menu actions to create ReflectMaterialRemappers
    /// </summary>
    public static class ReflectMaterialRemapperFactory
    {
        [MenuItem("Assets/Create/Reflect/Materials/Mappings (from selected Materials)")]
        public static void CreateFromMaterialSelection()
        {
            var guids = Selection.assetGUIDs;
            if (guids.Length == 0)
                return;

            var remapsList = new List<Material>();
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                    continue;
                remapsList.Add(material);
            }

            var remapper = MaterialMappings.CreateInstance(remapsList.ToArray());

            var targetPath = EditorUtility.SaveFilePanel(
            "Save New Material Mappings",
            Application.dataPath,
            "",
            "asset");

            if (targetPath.Length != 0)
            {
                if (!targetPath.StartsWith(Application.dataPath))
                {
                    Debug.LogError("Cannot save asset outside of project's assets folder!");
                    return;
                }
                targetPath = "Assets" + targetPath.Substring(Application.dataPath.Length);
                AssetDatabase.CreateAsset(remapper, targetPath);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Assets/Create/Reflect/Materials/Mappings (from selected Materials)", true)]
        public static bool CreateFromMaterialSelection_Validate()
        {
            return (Selection.assetGUIDs.Length > 0 && AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])) != null);
        }

        [MenuItem("Assets/Create/Reflect/Materials/Mappings (from selected SyncPrefab Importers)")]
        public static void CreateFromSyncPrefabScriptedImporter()
        {
            var guids = Selection.assetGUIDs;
            if (guids.Length == 0)
                return;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(path);
                if (asset == null)
                    continue;

                Dictionary<string, Material> remaps;
                asset.GetRemaps(out remaps);

                var remapper = MaterialMappings.CreateInstance(remaps);

                AssetDatabase.CreateAsset(remapper, Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + "_material-mappings.asset");
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Assets/Create/Reflect/Materials/Mappings (from selected SyncPrefab Importers)", true)]
        public static bool CreateFromSyncPrefabScriptedImporter_Validate()
        {
            return (Selection.assetGUIDs.Length > 0 &&
                AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])) != null &&
                AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])).GetType() == typeof(SyncPrefabScriptedImporter));
        }
    }
}