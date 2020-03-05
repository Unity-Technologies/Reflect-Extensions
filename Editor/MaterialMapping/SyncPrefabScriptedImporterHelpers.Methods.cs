using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Extensions.MaterialMapping;
using System.Linq;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    internal static partial class SyncPrefabScriptedImporterHelpers
    {
        static void FindMaterialsForSyncPrefabImporter(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                FindMaterialsForSyncPrefabImporter(importer);
        }

        internal static void FindMaterialsForSyncPrefabImporter(SyncPrefabScriptedImporter importer)
        {
            var targetPath = EditorUtility.OpenFolderPanel(
            "Pick Materials Location",
            Application.dataPath,
            "");

            if (targetPath == string.Empty)
                return;

            // issue error if path is outside of Project's Assets
            if (!targetPath.StartsWith(Application.dataPath))
            {
                Debug.LogError("Cannot save materials outside of project's assets folder!");
                return;
            }
            targetPath = "Assets" + targetPath.Substring(Application.dataPath.Length);

            var materialsGUIDS = AssetDatabase.FindAssets("t:Material", new string[1] { targetPath });
            var materials = new Dictionary<string, Material>();
            foreach (string guid in materialsGUIDS)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat)
                    materials.Add(mat.name, mat);
            }
            var remaps = new Dictionary<string, Material>();
            importer.GetRemaps(out remaps);
            switch(ReflectEditorPreferences.materialSearchMatchType)
            {
                case MaterialsOverride.MatchType.A_Equals_B:
                    foreach (KeyValuePair<string, Material> kvp in materials)
                    {
                        if (remaps.ContainsKey(kvp.Key) && remaps[kvp.Key] == null) // TODO : add an option to override existing materials ?
                            remaps[kvp.Key] = kvp.Value;
                    }
                    break;
                default:
                    var remapsNames = remaps.Keys.ToList();
                    foreach (KeyValuePair<string, Material> kvp_m in materials)
                    {
                        foreach (string r in remapsNames)
                            if (MaterialsOverride.Match(kvp_m.Key, r, ReflectEditorPreferences.materialSearchMatchType) && remaps[r] == null)
                                remaps[r] = kvp_m.Value;
                    }
                    break;
            }
            importer.SetRemaps(remaps);
        }

        static void ExtractMaterialsFromSyncPrefabImporter(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                ExtractMaterialsFromSyncPrefabImporter(importer);
        }

        internal static void ExtractMaterialsFromSyncPrefabImporter(SyncPrefabScriptedImporter importer)
        {
            var targetPath = EditorUtility.SaveFolderPanel(
            "Save Extracted Materials",
            Application.dataPath,
            "");
            
            if (targetPath == string.Empty)
                return;

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

        internal static void ExtractMaterialFromSyncPrefabImporter(SyncPrefabScriptedImporter importer, int materialIndex)
        {
            var materialName = importer.GetRemapNames()[materialIndex];
            var targetPath = EditorUtility.SaveFilePanel(
            "Save Extracted Material as",
            Application.dataPath,
            materialName, "mat");

            if (targetPath == string.Empty)
                return;

            // issue error if path is outside of Project's Assets
            if (!targetPath.StartsWith(Application.dataPath))
            {
                Debug.LogError("Cannot save materials outside of project's assets folder!");
                return;
            }
            targetPath = "Assets" + targetPath.Substring(Application.dataPath.Length);

            System.Action<Material> postAction = ReflectEditorPreferences.convertExtractedMaterials ? materialConversions[(int)ReflectEditorPreferences.extractedMaterialsConverionMethod] : null;
            importer.ExtractMaterial(materialName, targetPath, ReflectEditorPreferences.dontExtractRemappedMaterials, ReflectEditorPreferences.autoAssignRemapsOnExtract, postAction);
        }

        static void SortRemaps(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                SortRemaps(importer);
        }

        internal static void SortRemaps(SyncPrefabScriptedImporter importer)
        {
            importer.SortRemaps();
        }

        static void ResetRemaps(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                ResetRemaps(importer);
        }

        internal static void ResetRemaps(SyncPrefabScriptedImporter importer)
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
    }
}