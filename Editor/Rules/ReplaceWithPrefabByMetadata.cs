using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Extensions;

namespace UnityEditor.Reflect.Extensions
{
	public class ReplaceWithPrefabByMetadata : EditorWindow
	{
		public List<SearchCriteria> _searchCriterias = new List<SearchCriteria>() { new SearchCriteria ("Category", "Planting") };
		bool _matchAny, _matchHeight;//, _deleteOriginal;
		readonly bool _deleteOriginal = false;
		Object _prefab;

		//static GUIContent _deleteOriginalUI = new GUIContent("Delete Original", "");
		static GUIContent _matchAnyUI = new GUIContent("Match Any", "");
		static GUIContent _matchHeightUI = new GUIContent("Match Height", "");
		static GUIContent _prefabUI = new GUIContent("Prefab", "");

		SerializedObject so;
		SerializedProperty _criteriasProp;

		private void OnEnable()
		{
			so = new SerializedObject(this);
			_criteriasProp = so.FindProperty("_searchCriterias");
		}

		private void OnGUI()
		{
			EditorGUILayout.PropertyField(_criteriasProp, true);
			so.ApplyModifiedProperties();

			GUI.enabled = _searchCriterias.Count > 0;

			_matchAny = EditorGUILayout.Toggle(_matchAnyUI, _matchAny);

			if (GUILayout.Button("Search & Select"))
				SearchAndSelect();

			_prefab = EditorGUILayout.ObjectField(_prefabUI, _prefab, typeof(GameObject), false);

			GUI.enabled = _prefab != null;
			//_deleteOriginal = EditorGUILayout.Toggle(_deleteOriginalUI, _deleteOriginal); // UNDONE : Cannot delete objects from SyncPrefab Instance
			_matchHeight = EditorGUILayout.Toggle(_matchHeightUI, _matchHeight);

			if (GUILayout.Button("Replace Selection"))
				ReplaceSelection();
			if (GUILayout.Button("Search & Replace"))
				SearchAndReplace();
			GUI.enabled = true;
		}

		private void SearchAndSelect()
		{
			Selection.objects = Search(_searchCriterias, _matchAny).ToArray();
		}

		private void ReplaceSelection()
		{
			int undoLvl = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Replace Selection");
			foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.TopLevel))
			{
				ReplaceObject((GameObject)_prefab, g, _deleteOriginal, _matchHeight);
			}
			Undo.CollapseUndoOperations(undoLvl);
		}

		private void SearchAndReplace()
		{
			int undoLvl = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Search and Replace");
			foreach (GameObject g in Search(_searchCriterias, _matchAny))
			{
				ReplaceObject((GameObject)_prefab, g, _deleteOriginal, _matchHeight);
			}
			Undo.CollapseUndoOperations(undoLvl);
		}

		private void ReplaceObject (GameObject source, GameObject target, bool deleteOriginal = false, bool matchHeight = false)
		{
			Transform replacement;
			if (deleteOriginal)
			{
				replacement = ((GameObject)PrefabUtility.InstantiatePrefab(source, target.transform.parent)).transform;
				Undo.RegisterCreatedObjectUndo(replacement.gameObject, "Instantiate");
				replacement.position = target.transform.position;
				replacement.rotation = target.transform.rotation;
				Undo.DestroyObjectImmediate(target);
			}
			else
			{
				replacement = ((GameObject)PrefabUtility.InstantiatePrefab(source, target.transform)).transform;
				Undo.RegisterCreatedObjectUndo(replacement.gameObject, "Instantiate");
			}
			if (matchHeight)
			{
				var md = target.GetComponent<Metadata>();
				if (md && md.parameters.dictionary.ContainsKey("Height"))
				{
					var height = float.Parse(md.GetParameter("Height")) * 0.001f;
					Bounds rpcBounds = new Bounds();
					foreach (MeshFilter m in replacement.GetComponentsInChildren<MeshFilter>())
						rpcBounds.Encapsulate(m.sharedMesh.bounds);
					replacement.localScale = Vector3.one * (height / rpcBounds.size.y);
				}
			}
		}

		// Displays the Editor Window
		[MenuItem("Reflect/Tools/Search and Replace")]
		private static void ShowWindow()
		{
			var window = GetWindow<ReplaceWithPrefabByMetadata>();
			window.titleContent = new GUIContent("Search & Replace");
			window.Show();
		}

		private static List<GameObject> Search (List<SearchCriteria> criterias, bool matchAny = false)
		{
			// using Linq to collect gameobjects with Metadata matching the search criterias
			if (matchAny)
			{
				return (from item in FindObjectsOfType<Metadata>()
							where item.MatchAnyCriterias(criterias)
							select item.gameObject).ToList();
			}
			else
			{
				return (from item in FindObjectsOfType<Metadata>()
						where item.MatchAllCriterias(criterias)
						select item.gameObject).ToList();
			}
		}
	}
}