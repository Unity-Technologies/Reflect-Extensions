using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    /// <summary>
    /// Extensions for SyncPrefabScriptedImporter class to read/write MaterialRemaps
    /// </summary>
    public static class SyncPrefabScriptedImporterExtensions
    {
        /// <summary>
        /// Returns a List of Material Remapping Names
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <returns>A List of Material Remapping Names</returns>
        public static List<string> GetRemapNames(this SyncPrefabScriptedImporter syncPrefabImporter)
        {
            List<string> names = new List<string>();

            var so = new SerializedObject(syncPrefabImporter);

            var sourceRemaps = so.FindProperty("m_MaterialRemaps");

            for (int i = 0; i < sourceRemaps.arraySize; i++)
            {
                SerializedProperty item = sourceRemaps.GetArrayElementAtIndex(i);
                names.Add(item.FindPropertyRelative("syncMaterialName").stringValue);
            }
            return names;
        }

        /// <summary>
        /// Returns a List of Remapping Materials
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <returns>A List of Remapping Materials</returns>
        public static List<Material> GetRemapMaterials(this SyncPrefabScriptedImporter syncPrefabImporter)
        {
            List<Material> materials = new List<Material>();

            var so = new SerializedObject(syncPrefabImporter);

            var sourceRemaps = so.FindProperty("m_MaterialRemaps");

            for (int i = 0; i < sourceRemaps.arraySize; i++)
            {
                SerializedProperty item = sourceRemaps.GetArrayElementAtIndex(i);
                materials.Add((Material)item.FindPropertyRelative("remappedMaterial").objectReferenceValue);
            }

            return materials;
        }

        /// <summary>
        /// Provides a Dictionary of Material Remapping Names and Materials
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <param name="remaps"></param>
        public static void GetRemaps(this SyncPrefabScriptedImporter syncPrefabImporter, out Dictionary<string, Material> remaps)
        {
            // using out to force storing result in calling method and prevent multiple calls to this.

            remaps = new Dictionary<string, Material>();

            var so = new SerializedObject(syncPrefabImporter);

            var sourceRemaps = so.FindProperty("m_MaterialRemaps");

            for (int i = 0; i < sourceRemaps.arraySize; i++)
            {
                SerializedProperty item = sourceRemaps.GetArrayElementAtIndex(i);
                remaps.Add(item.FindPropertyRelative("syncMaterialName").stringValue, (Material)item.FindPropertyRelative("remappedMaterial").objectReferenceValue);
            }
        }

        /// <summary>
        /// Assign a Dictionary of Material Remapping Names and Materials to Material Remaps
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <param name="remaps"></param>
        public static void SetRemaps(this SyncPrefabScriptedImporter syncPrefabImporter, Dictionary<string, Material> remaps)
        {
            var so = new SerializedObject(syncPrefabImporter);

            var targetRemaps = so.FindProperty("m_MaterialRemaps");
            targetRemaps.arraySize = remaps.Count;

            var list = remaps.Keys.ToList();
            //list.Sort(); // FIXME : why does this screw up the importer ?

            for (int i = 0; i < list.Count; i++)
            {
                SerializedProperty item = targetRemaps.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("syncMaterialName").stringValue = list[i];
                item.FindPropertyRelative("remappedMaterial").objectReferenceValue = remaps[list[i]];
            }

            so.ApplyModifiedProperties();
            syncPrefabImporter.SaveAndReimport();
        }

        /// <summary>
        /// Sorts the Material Remaps by names.
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        public static void SortRemaps(this SyncPrefabScriptedImporter syncPrefabImporter)
        {
            var so = new SerializedObject(syncPrefabImporter);

            var sourceRemaps = so.FindProperty("m_MaterialRemaps");
            var unsortedList = new List<string>();

            for (int i = 0; i < sourceRemaps.arraySize; i++)
            {
                SerializedProperty item = sourceRemaps.GetArrayElementAtIndex(i);
                unsortedList.Add(item.FindPropertyRelative("syncMaterialName").stringValue);
            }

            var sortedList = new List<string>(unsortedList);
            sortedList.Sort();

            for (int destinationIndex = 0; destinationIndex < sortedList.Count; destinationIndex++)
            {
                var sourceIndex = unsortedList.FindIndex(x => x == sortedList[destinationIndex]);
                sourceRemaps.MoveArrayElement(sourceIndex, destinationIndex) ;
                
                var item = unsortedList[sourceIndex];
                unsortedList.RemoveAt(sourceIndex);
                if (destinationIndex > sourceIndex)
                    destinationIndex--;
                unsortedList.Insert(destinationIndex, item);
            }

            so.ApplyModifiedProperties();
            syncPrefabImporter.SaveAndReimport();
        }

        /// <summary>
        /// Resets the Material Remaps.
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        public static void ResetRemaps(this SyncPrefabScriptedImporter syncPrefabImporter)
        {
            var so = new SerializedObject(syncPrefabImporter);

            var sourceRemaps = so.FindProperty("m_MaterialRemaps");

            for (int i = 0; i < sourceRemaps.arraySize; i++)
            {
                SerializedProperty item = sourceRemaps.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("remappedMaterial").objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            syncPrefabImporter.SaveAndReimport();
        }

        /// <summary>
        /// Returns the Imported Object Root Transform.
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <returns></returns>
        public static Transform GetRoot(this SyncPrefabScriptedImporter syncPrefabImporter)
        {
            return AssetDatabase.LoadAssetAtPath<Transform>(syncPrefabImporter.assetPath);
        }

        /// <summary>
        /// Collects all materials from Renderers found in the Imported object's hierarchy.
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <param name="materials"></param>
        public static void GetImportedMaterials(this SyncPrefabScriptedImporter syncPrefabImporter, out List<Material> materials)
        {
            // using out to force storing result in calling method and prevent multiple calls to this.

            materials = new List<Material>();
            var root = syncPrefabImporter.GetRoot();

            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
                foreach (Material m in r.sharedMaterials)
                    if (!materials.Contains(m))
                        materials.Add(m);
        }

        public static void ExtractMaterial(this SyncPrefabScriptedImporter syncPrefabImporter, string materialName, string targetPath, bool bypassRemappedMaterials = true, bool assignRemaps = false, Action<Material> postExtractAction = null)
        {
            // TODO : implement single material extraction
            if (targetPath.Length != 0)
            {
                // getting existing remaps
                var existingRemaps = new Dictionary<string, Material>();
                syncPrefabImporter.GetRemaps(out existingRemaps);

                // initializing new remaps
                var extractedRemaps = new Dictionary<string, Material>();

                // getting all materials from imported objects
                var materials = new List<Material>();
                syncPrefabImporter.GetImportedMaterials(out materials);

                // find material index // TODO : sorting materials by remaps order ?
                var index = materials.FindIndex(m => m.name == materialName);

                if (bypassRemappedMaterials && existingRemaps.ContainsValue(materials[index]))
                    return;

                // duplicating material to destination
                var sourceName = materials[index].name;
                Material mCopy = new Material(materials[index]);
                postExtractAction?.Invoke(mCopy);
                AssetDatabase.CreateAsset(mCopy, targetPath);
                extractedRemaps.Add(sourceName, mCopy);

                // saving new materials
                AssetDatabase.SaveAssets();

                if (assignRemaps)
                {
                    var newRemaps = new Dictionary<string, Material>();

                    // mixing existing and extracted remaps
                    foreach (KeyValuePair<string, Material> kvp in existingRemaps)
                    {
                        if (kvp.Value == null && extractedRemaps.ContainsKey(kvp.Key))
                            newRemaps[kvp.Key] = extractedRemaps[kvp.Key];
                        else
                            newRemaps[kvp.Key] = existingRemaps[kvp.Key];
                    }

                    syncPrefabImporter.SetRemaps(newRemaps);
                }
            }
        }

        /// <summary>
        /// Extracts Imported Materials and assigns Remaps.
        /// </summary>
        /// <param name="syncPrefabImporter"></param>
        /// <param name="targetPath">Assets relative path to save new materials to.</param>
        /// <param name="bypassRemappedMaterials">Bypass already remapped materials.</param>
        /// <param name="assignRemaps">Automatically assign remaps after extraction.</param>
        /// <param name="postExtractAction">Action to call on extracted material.</param>
        public static void ExtractMaterials(this SyncPrefabScriptedImporter syncPrefabImporter, string targetPath, bool bypassRemappedMaterials = true, bool assignRemaps = false, Action<Material> postExtractAction = null)
        {
            if (targetPath.Length != 0)
            {
                // getting existing remaps
                var existingRemaps = new Dictionary<string, Material>();
                syncPrefabImporter.GetRemaps(out existingRemaps);

                // initializing new remaps
                var extractedRemaps = new Dictionary<string, Material>();

                // getting all materials from imported objects
                var materials = new List<Material>();
                syncPrefabImporter.GetImportedMaterials(out materials);

                // duplicating materials to destination
                foreach (Material m in materials)
                {
                    if (bypassRemappedMaterials && existingRemaps.ContainsValue(m))
                        continue;
                    Material mCopy = new Material(m);
                    postExtractAction?.Invoke(mCopy);
                    AssetDatabase.CreateAsset(mCopy, Path.Combine(targetPath, m.name + ".mat"));
                    extractedRemaps.Add(mCopy.name, mCopy);
                }

                // saving new materials
                AssetDatabase.SaveAssets();

                if (assignRemaps)
                {
                    var newRemaps = new Dictionary<string, Material>();

                    // mixing existing and extracted remaps
                    foreach (KeyValuePair<string, Material> kvp in existingRemaps)
                    {
                        if (kvp.Value == null && extractedRemaps.ContainsKey(kvp.Key))
                            newRemaps[kvp.Key] = extractedRemaps[kvp.Key];
                        else
                            newRemaps[kvp.Key] = existingRemaps[kvp.Key];
                    }

                    syncPrefabImporter.SetRemaps(newRemaps);
                }
            }
        }
    }
}