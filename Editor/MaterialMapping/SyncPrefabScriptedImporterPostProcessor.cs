using UnityEngine;
using System.IO;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    public class SyncPrefabScriptedImporterPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            System.Action<Material> postAction = ReflectEditorPreferences.convertExtractedMaterials ? SyncPrefabScriptedImporterHelpers.materialConversions[(int)ReflectEditorPreferences.extractedMaterialsConverionMethod] : null;
            foreach (string assetPath in importedAssets)
            {
                var importer = AssetImporter.GetAtPath(assetPath) as SyncPrefabScriptedImporter;
                if (!importer)
                    continue;
                if (ReflectEditorPreferences.autoExtractMaterialsOnImport)
                {
                    var destination = Path.Combine(Path.GetDirectoryName(assetPath), ReflectEditorPreferences.autoExtractRelativePath);
                    if (!Directory.Exists(destination))
                        Directory.CreateDirectory(destination);
                    importer.ExtractMaterials(destination, ReflectEditorPreferences.dontExtractRemappedMaterials, ReflectEditorPreferences.autoAssignRemapsOnExtract, postAction);
                }
            }
        }

        void OnPreprocessAsset()
        {
            if (assetImporter.GetType() != typeof(SyncPrefabScriptedImporter))
                return;            
        }
    }
}