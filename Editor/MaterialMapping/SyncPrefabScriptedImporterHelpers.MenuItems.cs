
namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    internal static partial class SyncPrefabScriptedImporterHelpers
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

        [MenuItem("Assets/Reflect/Find Materials")]
        static void FindMaterialsForSelectedSyncPrefabImporter()
        {
            FindMaterialsForSyncPrefabImporter(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        [MenuItem("Assets/Reflect/Apply Generic Mappings", true)]
        [MenuItem("Assets/Reflect/Sort Mappings", true)]
        [MenuItem("Assets/Reflect/Reset Mappings", true)]
        [MenuItem("Assets/Reflect/Extract Materials", true)]
        [MenuItem("Assets/Reflect/Find Materials", true)]
        static bool SelectionFirstGuidIsSyncPrefabScriptedImporter()
        {
            return Selection.assetGUIDs.Length == 1 &&
                AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]))?.GetType() == typeof(SyncPrefabScriptedImporter);
        }
    }
}