using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Extensions;

namespace UnityEditor.Reflect.Extensions
{
    public static class SyncPrefabScriptedImporterHelpers
    {
        [MenuItem("Assets/Reflect/Apply Generic Mappings")]
        static void AssignMaterialRemapsToSelection()
        {
            AssignMaterialRemaps(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        //[MenuItem("Assets/Reflect/Sort Mappings")] // UNDONE : sorting remaps seems to cause problems
        static void SortSelectedRemaps()
        {
            SortRemaps(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        [MenuItem("Assets/Reflect/Reset Mappings")]
        static void ResetSelectedRemaps()
        {
            ResetRemaps(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        [MenuItem("Assets/Reflect/Extract Materials")]
        static void ExtractMaterialsFromSelectedSyncPrefabImporter()
        {
            ExtractMaterialsFromSyncPrefabImporter(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        [MenuItem("Assets/Reflect/Apply Generic Mappings", true)]
        [MenuItem("Assets/Reflect/Sort Mappings", true)]
        [MenuItem("Assets/Reflect/Reset Mappings", true)]
        [MenuItem("Assets/Reflect/Extract Materials", true)]
        static bool SelectionFirstGuidIsSyncPrefabScriptedImporter()
        {
            return Selection.assetGUIDs.Length == 1 &&
                AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]))?.GetType() == typeof(SyncPrefabScriptedImporter);
        }

        static void ExtractMaterialsFromSyncPrefabImporter(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                ExtractMaterialsFromSyncPrefabImporter(importer);
        }

        static void ExtractMaterialsFromSyncPrefabImporter(SyncPrefabScriptedImporter importer)
        {
            var targetPath = EditorUtility.SaveFolderPanel(
            "Save Extracted Materials",
            Application.dataPath,
            "");

            // issue error if path is outside of Project's Assets
            if (!targetPath.StartsWith(Application.dataPath))
            {
                Debug.LogError("Cannot save materials outside of project's assets folder!");
                return;
            }
            targetPath = "Assets" + targetPath.Substring(Application.dataPath.Length);

            System.Action<Material> postAction = ReflectEditorPreferences.convertExtractedMaterials ? materialConversions[(int)ReflectEditorPreferences.extractedMaterialsConverionMethod] : null;
            importer.ExtractMaterials(targetPath, ReflectEditorPreferences.dontExtractRemappedMaterials, ReflectEditorPreferences.autoAssignRemapsOnExtract, postAction);
        }

        static void SortRemaps(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                SortRemaps(importer);
        }

        static void SortRemaps(SyncPrefabScriptedImporter importer)
        {
            importer.SortRemaps();
        }

        static void ResetRemaps(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                ResetRemaps(importer);
        }

        static void ResetRemaps(SyncPrefabScriptedImporter importer)
        {
            // disabling auto extract for next import
            var auto = ReflectEditorPreferences.autoExtractMaterialsOnImport;
            ReflectEditorPreferences.autoExtractMaterialsOnImport = false;
            importer.ResetRemaps();
            ReflectEditorPreferences.autoExtractMaterialsOnImport = auto;
        }

        static void AssignMaterialRemaps(string assetPath)
        {
            if (assetPath.Contains("Reflect"))
            {
                var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
                if (importer)
                    AssignMaterialRemaps(importer);
            }
        }

        static void AssignMaterialRemaps(SyncPrefabScriptedImporter importer)
        {
            if (importer)
            {
                // find all ScriptableObject based remaps
                if (!AssetDatabase.IsValidFolder("Assets/Reflect"))
                    return;

                List<MaterialMappings> remappers = new List<MaterialMappings>();
                foreach (string guid in AssetDatabase.FindAssets("t:MaterialMappings", new string[] { "Assets/Reflect" }))
                {
                    var remapper = AssetDatabase.LoadAssetAtPath<MaterialMappings>(AssetDatabase.GUIDToAssetPath(guid));
                    if (remapper.enabled)
                        remappers.Add(remapper);
                }

                // sort remaps by priority (to prioritize conflicting remaps)
                remappers.Sort((a, b) => a.priority.CompareTo(b.priority));

                // for every material names, loop through remaps to find an override
                Dictionary<string, Material> remaps;
                importer.GetRemaps(out remaps);
                var names = importer.GetRemapNames();
                foreach (string name in names)
                {
                    foreach (MaterialMappings remapper in remappers)
                    {
                        var remapperNames = remapper.materialNames;
                        if (remapperNames.Contains(name) && (remaps[name] == null || remapper.overwrite))
                        {
                            //Debug.Log(string.Format("{0} => {1}", name, remapper.materialRemaps[remapperNames.FindIndex(x => x == name)].remappedMaterial));
                            remaps[name] = remapper[remapperNames.FindIndex(x => x == name)].remappedMaterial;
                            break;
                        }
                    }
                }

                // assign override
                importer.SetRemaps(remaps);
            }
        }

        public enum MaterialConversion : int
        {
            ReflectToStandard = 0
        }

        // TODO : implement other material conversions (URP, HDRP).
        internal static System.Action<Material>[] materialConversions = new System.Action<Material>[1] {
            new System.Action<Material>((m) => {
                bool isTransparent = m.shader.name == "UnityReflect/Standard Transparent";
                m.shader = isTransparent ? Shader.Find("Standard (Specular setup)") : Shader.Find("Standard");
                m.SetFloat("_Mode", isTransparent ? 3.0f : 0.0f);
            })
        };
    }
}