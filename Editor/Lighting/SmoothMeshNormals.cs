using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityEditor.Reflect.Extensions.Meshes
{
    public class SmoothMeshNormals : EditorWindow
    {
        UnwrapParam unwrapParam = new UnwrapParam();
        bool includeChildren = default;

        [MenuItem("Reflect/Recalculate Normals and Tangents")]
        static void Init()
        {
            SmoothMeshNormals window = GetWindow<SmoothMeshNormals>();
            window.titleContent = new GUIContent("Smooth Normals");
            window.Show();
        }

        private void OnEnable()
        {
            UnwrapParam.SetDefaults(out unwrapParam);
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);

            if (GUILayout.Button("Revert Selection"))
                RevertSelection(includeChildren);

            if (GUILayout.Button("Smooth Selection"))
                ApplySelection(includeChildren);
        }

        private void ApplySelection(bool indludeChildren = false)
        {
            SyncPrefabScriptedImporterNormals.SmoothSelectedMeshFilters(unwrapParam, includeChildren);
        }

        private void RevertSelection(bool includeChildren = false)
        {
            SyncPrefabScriptedImporterNormals.RevertSelectedMeshFilters(includeChildren);
        }
    }

    public static class SyncPrefabScriptedImporterNormals
    {
        internal static void SmoothSelectedMeshFilters(UnwrapParam unwrapParam, bool includeChildren = false)
        {
            List<Mesh> meshesToUpdate = new List<Mesh>();
            Dictionary<Mesh, Mesh> meshes = new Dictionary<Mesh, Mesh>();
            var meshFilters = includeChildren ?
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable | SelectionMode.Deep) :
                Selection.GetFiltered<MeshFilter>(SelectionMode.OnlyUserModifiable);

            if (meshFilters.Length == 0)
                return;

            var undoLvl = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Smooth Meshes");

            for (int i = 0; i < meshFilters.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Smoothing Meshes...", meshFilters[i].gameObject.name, (float)i / (float)meshFilters.Length);
                Undo.RecordObject(meshFilters[i], "Smooth");

                // updating existing meshes
                var path = AssetDatabase.GetAssetPath(meshFilters[i].sharedMesh);
                if (Path.GetExtension(path) != ".SyncMesh")
                {
                    if (!meshesToUpdate.Contains(meshFilters[i].sharedMesh))
                    {
                        meshFilters[i].sharedMesh.RecalculateNormals();
                        meshFilters[i].sharedMesh.RecalculateTangents();

                        meshesToUpdate.Add(meshFilters[i].sharedMesh);
                    }
                    continue;
                }

                if (!meshes.ContainsKey(meshFilters[i].sharedMesh))
                {
                    var dir = Path.Combine(Path.GetDirectoryName(path), "Smooth");
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

                    newmesh.RecalculateNormals();
                    newmesh.RecalculateTangents();

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