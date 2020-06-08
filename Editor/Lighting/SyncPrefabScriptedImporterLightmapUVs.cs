using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.Reflect.Extensions.Lightmapping
{
    public class LightmapUVSettings : EditorWindow
    {
        UnwrapParam unwrapParam = new UnwrapParam();
        bool includeChildren = default;

        [MenuItem("Reflect/Lightmap UVs")]
        static void Init()
        {
            LightmapUVSettings window = GetWindow<LightmapUVSettings>();
            window.titleContent = new GUIContent("Reflect Lightmap UVs");
            window.Show();
        }

        private void OnEnable()
        {
            UnwrapParam.SetDefaults(out unwrapParam);
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            
            unwrapParam.packMargin = EditorGUILayout.FloatField("Pack Margin", unwrapParam.packMargin);
            unwrapParam.areaError = EditorGUILayout.FloatField("Area Error", unwrapParam.areaError);
            unwrapParam.angleError = EditorGUILayout.FloatField("Angle Error", unwrapParam.angleError);
            unwrapParam.hardAngle = EditorGUILayout.FloatField("Hard Angle", unwrapParam.hardAngle);

            includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);

            if (GUILayout.Button("Revert Selection"))
                RevertSelection(includeChildren);

            if (GUILayout.Button("Generate UVs on Selection"))
                ApplySelection(includeChildren);
        }

        private void ApplySelection(bool indludeChildren = false)
        {
            SyncPrefabScriptedImporterLightmapUVs.GenerateLightmapUVsForSelectedMeshFilters(unwrapParam, includeChildren);
        }

        private void RevertSelection(bool includeChildren = false)
        {
            SyncPrefabScriptedImporterLightmapUVs.RevertSelectedMeshFilters(includeChildren);
        }
    }

    public static class SyncPrefabScriptedImporterLightmapUVs
    {
        internal static void GenerateLightmapUVsForSelectedMeshFilters(UnwrapParam unwrapParam, bool includeChildren = false)
        {
            List<Mesh> meshesToUpdate = new List<Mesh>();
            Dictionary<Mesh, Mesh> meshes = new Dictionary<Mesh, Mesh>();
            var meshFilters = includeChildren ?
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable | SelectionMode.Deep) :
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable);

            if (meshFilters.Length == 0)
                return;

            var undoLvl = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Generate Lightmap UVs");

            for (int i = 0; i < meshFilters.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Unwrapping Lightmap UVs", meshFilters[i].gameObject.name, (float)i / (float)meshFilters.Length);
                Undo.RecordObject(meshFilters[i], "Unwrap");

                // updating existing meshes
                var path = AssetDatabase.GetAssetPath(meshFilters[i].sharedMesh);
                if (Path.GetExtension(path) != ".SyncMesh")
                {
                    if (!meshesToUpdate.Contains(meshFilters[i].sharedMesh))
                    {
                        Unwrapping.GenerateSecondaryUVSet(meshFilters[i].sharedMesh);
                        meshesToUpdate.Add(meshFilters[i].sharedMesh);
                    }
                    continue;
                }

                if (!meshes.ContainsKey(meshFilters[i].sharedMesh))
                {
                    var dir = Path.Combine(Path.GetDirectoryName(path), "Lightmapping");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    var name = Path.GetFileName(path);
                    var newPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + ".mesh");

                    Mesh newmesh;

                    if (File.Exists(newPath))
                    {
                        newmesh = AssetDatabase.LoadAssetAtPath<Mesh>(newPath);
                    }
                    else
                    {
                        newmesh = Object.Instantiate<Mesh>(meshFilters[i].sharedMesh);
                    }

                    Unwrapping.GenerateSecondaryUVSet(newmesh);

                    AssetDatabase.CreateAsset(newmesh, newPath);

                    meshes.Add(meshFilters[i].sharedMesh, newmesh);

                    meshFilters[i].sharedMesh = newmesh;
                }
                else
                {
                    meshFilters[i].sharedMesh = meshes[meshFilters[i].sharedMesh];
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            Undo.CollapseUndoOperations(undoLvl);
        }

        internal static void RevertSelectedMeshFilters(bool includeChildren = false)
        {
            var meshFilters = includeChildren ?
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable | SelectionMode.Deep) :
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable);

            if (meshFilters.Length == 0)
                return;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                PrefabUtility.RevertObjectOverride(meshFilters[i], InteractionMode.AutomatedAction);
            }
        }
    }
}