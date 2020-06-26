using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Extensions.MaterialMapping;
using System.Linq;
using System.IO;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    internal static partial class SyncPrefabScriptedImporterHelpers
    {
        static void WhiteBoxSyncPrefabImporter(string assetPath)
        {
            var importer = (SyncPrefabScriptedImporter)AssetImporter.GetAtPath(assetPath);
            if (importer)
                WhiteBoxSyncPrefabImporter(importer);
        }

        internal static void WhiteBoxSyncPrefabImporter(SyncPrefabScriptedImporter importer)
        {
            string path = "Assets/Materials/White Box";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Material whiteBoxMaterial;
            var whiteBoxMaterialPath = Path.Combine(path, "white.mat");
            if (File.Exists(whiteBoxMaterialPath))
            {
                whiteBoxMaterial = AssetDatabase.LoadAssetAtPath<Material>(whiteBoxMaterialPath);
            }
            else
            {
                whiteBoxMaterial = new Material(Shader.Find("Standard"));
                // TODO : set material default values
                whiteBoxMaterial.color = new Color(.9f, .9f, .9f, 1);
                whiteBoxMaterial.SetFloat("_Glossiness", 0.2f);
                AssetDatabase.CreateAsset(whiteBoxMaterial, whiteBoxMaterialPath);
                AssetDatabase.SaveAssets();
            }

            Material glassBoxMaterial;
            var glassBoxMaterialPath = Path.Combine(path, "glass.mat");
            if (File.Exists(glassBoxMaterialPath))
            {
                glassBoxMaterial = AssetDatabase.LoadAssetAtPath<Material>(glassBoxMaterialPath);
            }
            else
            {
                glassBoxMaterial = new Material(Shader.Find("Standard (Specular setup)"));
                // TODO : set material default values
                glassBoxMaterial.color = new Color(0, 0, 0, 0);
                glassBoxMaterial.SetColor("_SpecColor", new Color(0.32f, 0.4f, 0.4f, 1));
                glassBoxMaterial.SetFloat("_Mode", 3.0f);
                glassBoxMaterial.SetFloat("_Glossiness", 0.9f);
                AssetDatabase.CreateAsset(glassBoxMaterial, glassBoxMaterialPath);
                AssetDatabase.SaveAssets();
            }

            var remaps = new Dictionary<string, Material>();
            importer.GetRemaps(out remaps);

            var remapsNames = remaps.Keys.ToList();
            foreach (string r in remapsNames)
            {
                if (r.ToLower().Contains("glass")) //  TODO : use a more accurate method based on material itself ?
                    remaps[r] = glassBoxMaterial;
                else
                    remaps[r] = whiteBoxMaterial;
            }

            importer.SetRemaps(remaps);
        }

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
            switch (ReflectEditorPreferences.materialSearchMatchType)
            {
                case MaterialMappings.MatchType.A_Equals_B:
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
                    //if (remapper.enabled)
                        remappers.Add(remapper);
                }

                // sort remaps by priority (to prioritize conflicting remaps)
                //remappers.Sort((a, b) => a.priority.CompareTo(b.priority));

                // for every material names, loop through remaps to find an override
                Dictionary<string, Material> remaps;
                importer.GetRemaps(out remaps);
                var names = importer.GetRemapNames();
                foreach (string name in names)
                {
                    foreach (MaterialMappings remapper in remappers)
                    {
                        var remapperNames = remapper.materialNames;
                        if (remapperNames.Contains(name) && (remaps[name] == null/* || remapper.overwrite*/))
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