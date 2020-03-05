using UnityEngine;

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    [InitializeOnLoad]
    class MaterialRemapPropertyDrawerMenuItems
    {
        static MaterialRemapPropertyDrawerMenuItems()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;

            void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
            {
                if (property.serializedObject.targetObject.GetType() != typeof(SyncPrefabScriptedImporter))
                    return;

                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    return;

                var remapsProperty = property.serializedObject.FindProperty("m_MaterialRemaps");
                var importer = (SyncPrefabScriptedImporter)property.serializedObject.targetObject;

                if (property.objectReferenceValue == null)
                {
                    menu.AddItem(new GUIContent("Extract Material"), false, () =>
                    {
                        int index;
                        string[] fullPathSplit = property.propertyPath.Split('.');
                        string ending = fullPathSplit[fullPathSplit.Length - 2];
                        if (!int.TryParse(ending.Replace("data[", "").Replace("]", ""), out index))
                            return;
                        SyncPrefabScriptedImporterHelpers.ExtractMaterialFromSyncPrefabImporter(importer, index);
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent("Reset"), false, () =>
                    {
                        property.objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();
                        importer.SaveAndReimport();
                    });
                }

                menu.AddItem(new GUIContent("Extract All Materials"), false, () =>
                {
                    SyncPrefabScriptedImporterHelpers.ExtractMaterialsFromSyncPrefabImporter(importer);
                });

                menu.AddItem(new GUIContent("Find Materials"), false, () =>
                {
                    SyncPrefabScriptedImporterHelpers.FindMaterialsForSyncPrefabImporter(importer);
                });

                //menu.AddItem(new GUIContent("Sort"), false, () =>
                //{
                //    SyncPrefabScriptedImporterHelpers.SortRemaps(importer);
                //});

                menu.AddItem(new GUIContent("Reset All"), false, () =>
                {
                    SyncPrefabScriptedImporterHelpers.ResetRemaps(importer);
                });
            }
        }
    }
}