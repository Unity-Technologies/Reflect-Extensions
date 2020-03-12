using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace UnityEngine.Reflect.Extensions.Rules.Advanced
{
	/// <summary>
	/// A combined Metadata Search object.
	/// </summary>
	[CreateAssetMenu(fileName = "New Metadata Search", menuName = "Reflect/Rules/Metadata Search", order = 1)]
	public class MetadataSearch : ScriptableObject
	{
		[Header("Rules")]
		[Tooltip("Search Criterias")]
		[SerializeField] private List<SearchCriteria> _criterias = new List<SearchCriteria>() { new SearchCriteria("Category", "Planting")};
		[Tooltip("Search for any criteria if true.\nSearch for exact match if false.")]
		[SerializeField] private bool _matchAny = default;

		/// <summary>
		/// The list of SearchCriterias
		/// </summary>
		public List<SearchCriteria> Criterias { get => _criterias; }

		/// <summary>
		/// Search for any criteria if true.
		/// Search for exact match if false.
		/// </summary>
		public bool MatchAny { get => _matchAny; }

		/// <summary>
		/// Number of SearchCriterias
		/// </summary>
		public int Count { get => _criterias.Count; }

		/// <summary>
		/// Returns SearchCriterias at index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SearchCriteria this[int index]
		{
			get => _criterias[index];
		}

		/// <summary>
		/// Returns SearchCriterias with key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public SearchCriteria this[string key]
		{
			get => _criterias[_criterias.FindIndex(x => x.key == key)];
		}

		/// <summary>
		/// Finds all Metadata in Scene that matches the Search
		/// </summary>
		/// <returns>A list of GameObjects</returns>
		public List<Metadata> FindMatchesInScene ()
		{
			List<Metadata> objects = new List<Metadata>();
			var metadatas = FindObjectsOfType<Metadata>();
			if (metadatas.Length == 0)
				return objects;

			if (MatchAny)
			{
				for (int i = 0; i < metadatas.Length; i++)
					if (metadatas[i].MatchAnyCriterias(Criterias))
						objects.Add(metadatas[i]);
			}
			else
			{
				for (int i = 0; i < metadatas.Length; i++)
					if (metadatas[i].MatchAllCriterias(Criterias))
						objects.Add(metadatas[i]);
			}
			return objects;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Reflect/Rules/Select Objects in Scene")]
		static void FindObjectsInScene()
		{
			var metadataSearch = Selection.activeObject as MetadataSearch;
			var selection = metadataSearch.FindMatchesInScene();
			Selection.objects = (from item in selection
									 select item.gameObject).ToArray();
		}

		[UnityEditor.MenuItem("Assets/Reflect/Rules/Select Objects in Scene", true)]
		static bool FindObjectsInScene_Validate()
		{
			return Selection.activeObject?.GetType() == typeof(MetadataSearch);
		}
#endif
	}
}