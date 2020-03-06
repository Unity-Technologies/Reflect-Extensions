using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Reflect.Extensions.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO : use Reflect.Populate.Rules and replace Filter with Criteria
// TODO : support for multiple custom Areas

namespace UnityEngine.Reflect.Extensions.AI
{
	/// <summary>
	/// Builds and Refreshes a NavMesh on incoming geometry.
	/// </summary>
	[AddComponentMenu ("Reflect/AI/ReflectNavMeshBuilder")]
	//[RequireComponent (typeof(ReflectEventsManager))] // UNDONE : now using Singleton
	[DisallowMultipleComponent]
	public class ReflectNavMeshBuilder : MonoBehaviour
	{
		[System.Serializable]
		public struct Filter
		{
			public string key;
			public string value;

			public Filter(string key, string value)
			{
				this.key = key;
				this.value = value;
			}
		}

		[Header ("Physics")]
		[Tooltip ("Add Mesh Colliders to incoming Geometry.")]
		[SerializeField] bool addMeshColliders = false;

		[Tooltip("Mesh Colliders are Triggers.")]
		[SerializeField] bool isTrigger = false;

		[Header("Navigation")]
		[Tooltip("Ignore Objects by Metadata Key/Value")]
		[SerializeField] Filter[] ignoreFilters = new Filter[2] {
			new Filter ("Category", "Doors"),
			new Filter ("Category", "Ceilings")
		};

		[Tooltip("Set Not Walkable by Metadata Key/Value")]
		[SerializeField]
		Filter[] notWalkable = new Filter[7] {
		new Filter ("Category", "Walls"),
		new Filter ("Category", "Railings"),
		new Filter ("Category", "Columns"),
		new Filter ("Category", "Furniture"),
		new Filter ("Category", "Site"),
		new Filter ("Category", "Planting"),
		new Filter ("Category", "Roofs")
	};

		NavMeshData _navMeshData;
		AsyncOperation _asyncOperation;
		NavMeshDataInstance _NMDinstance;
		List<NavMeshBuildSource> _sources = new List<NavMeshBuildSource>();
		Bounds _bounds = new Bounds();

		MeshFilter[] _meshFilters;
		SyncManager _syncManager;

		static private ReflectNavMeshBuilder _instance;
		static public ReflectNavMeshBuilder instance
		{
			get { return _instance; }
		}

		public event System.Action onNavMeshUpdated;

		private void Awake()
		{
			if (_instance != null)
				Destroy(this);

			_instance = this;
			_syncManager = FindObjectOfType<SyncManager>();
		}
		private void OnEnable()
		{
			_navMeshData = new NavMeshData();
			_NMDinstance = NavMesh.AddNavMeshData(_navMeshData);

			ReflectEventsManager.Instance.onIsDoneInstantiating += Instance_onIsDoneInstantiating;
			ReflectEventsManager.Instance.onSyncUpdateEnd += Instance_onSyncUpdateEnd;
		}

		private void Instance_onIsDoneInstantiating(bool isDone)
		{
			if (isDone)
				UpdateNavMesh();
		}

		private void Instance_onSyncUpdateEnd()
		{
			if (ReflectEventsManager.Instance.IsDoneInstantiating)
				UpdateNavMesh();
		}

		private void OnDisable()
		{
			ReflectEventsManager.Instance.onIsDoneInstantiating -= Instance_onIsDoneInstantiating;
			ReflectEventsManager.Instance.onSyncUpdateEnd -= Instance_onSyncUpdateEnd;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/ReflectNavMeshBuilder/UpdateNavMesh")]
		static void ForceUpdateNavMesh(MenuCommand command)
		{
			var nvm = (ReflectNavMeshBuilder)command.context;
			nvm.UpdateNavMesh();
		}
#endif
		private void UpdateNavMesh()
		{
			_sources.Clear();

			// get all meshfilters in hierarchy
			_meshFilters = _syncManager.syncRoot.GetComponentsInChildren<MeshFilter>();

			// by pass if hierarchy contains no mesh
			if (_meshFilters.Length == 0)
				return;

			// initialize bounds with first renderer
			_bounds = (Bounds)_meshFilters[0].GetComponent<Renderer>()?.bounds;

			// for every mesh
			for (int i = 0; i < _meshFilters.Length; i++)
			{
				// find associated metadata
				//Metadata mData = _meshFilters[i].GetComponent<Metadata>();
				Metadata mData = _meshFilters[i].GetComponentInParent<Metadata>();

				// bypass filtered objects
				if (mData && ContainsData(mData, ignoreFilters))
					continue;

				// extending bounds
				Renderer renderer = _meshFilters[i].GetComponent<Renderer>();
				_bounds.Encapsulate(renderer.bounds.min);
				_bounds.Encapsulate(renderer.bounds.max);

				// add Mesh Colliders for interacting with objects
				if (addMeshColliders)
					_meshFilters[i].gameObject.AddComponent<MeshCollider>().isTrigger = isTrigger;

				Mesh m = _meshFilters[i].sharedMesh;
				if (m == null) continue;

				NavMeshBuildSource s = new NavMeshBuildSource();
				s.shape = NavMeshBuildSourceShape.Mesh;
				s.sourceObject = m;
				s.transform = _meshFilters[i].transform.localToWorldMatrix;
				s.area = (mData && ContainsData(mData, notWalkable)) ? 1 : 0;
				_sources.Add(s);
			}
			UpdateNavMeshMesh();
		}

		private void UpdateNavMeshMesh()
		{
			_asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(_navMeshData, NavMesh.GetSettingsByID(0), _sources, _bounds);
			_asyncOperation.completed += UpdateNavMeshDataAsync_completed;
		}

		private void UpdateNavMeshDataAsync_completed(AsyncOperation obj)
		{
			onNavMeshUpdated?.Invoke();
			foreach (INavMeshUpdate i in FindObjectsOfType<MonoBehaviour>().OfType<INavMeshUpdate>())
				i.OnNavMeshUpdate();
		}

		// returns true if MetaData contains any of the provided filters
		private bool ContainsData(Metadata mData, Filter[] filters)
		{
			if (filters.Length > 0)
			{
				for (int i = 0; i < filters.Length; i++)
				{
					if (mData.GetParameter(filters[i].key) == filters[i].value)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if a point was found.
		/// </summary>
		/// <param name="result">The World Space point on NavMesh.</param>
		/// <returns></returns>
		public bool RandomPoint(out Vector3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				Vector3 randomPoint = _bounds.center + Random.insideUnitSphere * _bounds.extents.magnitude;
				NavMeshHit hit;
				if (NavMesh.SamplePosition(randomPoint, out hit, 50.0f, NavMesh.AllAreas))
				{
					result = hit.position;
					return true;
				}
			}
			result = Vector3.zero;
			return false;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/Reflect.Populate/NavMeshBuilder", false, 10)]
		private static void CreateComponentHoldingGameObject(MenuCommand menuCommand)
		{
			var g = new GameObject("Reflect NavMesh Builder", new System.Type[1] { typeof(ReflectNavMeshBuilder) });
			GameObjectUtility.SetParentAndAlign(g, menuCommand.context as GameObject);
			Undo.RegisterCreatedObjectUndo(g, "Create Reflect NavMesh Builder");
		}
#endif
	}
}