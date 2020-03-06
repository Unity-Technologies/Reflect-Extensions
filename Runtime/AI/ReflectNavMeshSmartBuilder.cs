using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Reflect.Extensions.Helpers;

namespace UnityEngine.Reflect.Extensions.AI
{
	/// <summary>
	/// ReflectNavMeshBuilder.
	/// Builds and Refresh a NavMesh on incoming geometry.
	/// </summary>
	[AddComponentMenu("Reflect/AI/ReflectNavMeshBuilder (Smart)")]
	[DisallowMultipleComponent]
	public class ReflectNavMeshSmartBuilder : MonoBehaviour
	{
		public event Action onNavMeshUpdated;

		ReflectEventsManager reflectEventsManager;

		NavMeshData _navMeshData;
		AsyncOperation _asyncOperation;
		NavMeshDataInstance _NMDinstance;
		List<NavMeshBuildSource> _sources = new List<NavMeshBuildSource>();
		Dictionary<GameObject, List<NavMeshBuildSource>> _sourceLookUp = new Dictionary<GameObject, List<NavMeshBuildSource>>();
		Bounds _bounds = new Bounds();

		MeshFilter[] _meshFilters;

		private void Awake()
		{
			reflectEventsManager = FindObjectOfType<ReflectEventsManager>();

			_navMeshData = new NavMeshData();
			_NMDinstance = NavMesh.AddNavMeshData(_navMeshData);

			if (reflectEventsManager == null)
				enabled = false;
		}

		private void OnEnable()
		{
			reflectEventsManager.onSyncObjectCreated += ReflectEventsManager_onGameObjectCreated;
			reflectEventsManager.onSyncObjectDestroyed += ReflectEventsManager_onGameObjectDestroyed;
			reflectEventsManager.onIsDoneInstantiating += ReflectEventsManager_onIsDoneInstantiating;
			reflectEventsManager.onSyncUpdateEnd += ReflectEventsManager_onSyncUpdateEnd;
			reflectEventsManager.onSyncUpdateBegin += ReflectEventsManager_onSyncUpdateBegin;
		}

		private void ReflectEventsManager_onSyncUpdateBegin()
		{

		}

		private void ReflectEventsManager_onSyncUpdateEnd()
		{
			//UpdateNavMesh();
			if (reflectEventsManager.IsDoneInstantiating)
				RebuildAllNavMesh_BruteForce();
		}

		private void ReflectEventsManager_onIsDoneInstantiating(bool isDone)
		{
			if (isDone)
			{
				//UpdateNavMesh();
				RebuildAllNavMesh_BruteForce();
			}
		}

		private void _asyncOperation_completed(AsyncOperation obj)
		{
			if (onNavMeshUpdated != null)
				onNavMeshUpdated.Invoke();
		}

		private void ReflectEventsManager_onGameObjectDestroyed(SyncObjectBinding.Identifier obj)
		{
			//RemoveFromNavMesh(obj);
		}

		private void ReflectEventsManager_onGameObjectCreated(GameObject obj)
		{
			//AddToNavMesh(obj);
		}

		private void OnDisable()
		{
			reflectEventsManager.onSyncObjectCreated -= ReflectEventsManager_onGameObjectCreated;
			reflectEventsManager.onSyncObjectDestroyed -= ReflectEventsManager_onGameObjectDestroyed;
			reflectEventsManager.onIsDoneInstantiating -= ReflectEventsManager_onIsDoneInstantiating;
		}

		private void UpdateNavMesh()
		{
			_asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(_navMeshData, NavMesh.GetSettingsByID(0), _sources, _bounds);
			_asyncOperation.completed += _asyncOperation_completed;
		}

		private void AddToNavMesh(GameObject root)
		{
			// get all meshfilters in hierarchy
			_meshFilters = root.GetComponentsInChildren<MeshFilter>();

			// by pass if hierarchy contains no mesh
			if (_meshFilters.Length == 0)
				return;

			// initialize bounds with first renderer
			_bounds = (Bounds)_meshFilters[0].GetComponent<Renderer>()?.bounds;

			// for every mesh
			for (int i = 0; i < _meshFilters.Length; i++)
			{
				// extending bounds
				Renderer renderer = _meshFilters[i].GetComponent<Renderer>();
				_bounds.Encapsulate(renderer.bounds.min);
				_bounds.Encapsulate(renderer.bounds.max);

				Mesh m = _meshFilters[i].sharedMesh;
				if (m == null) continue;

				NavMeshBuildSource s = new NavMeshBuildSource();
				s.shape = NavMeshBuildSourceShape.Mesh;
				s.sourceObject = m;
				s.transform = _meshFilters[i].transform.localToWorldMatrix;
				s.area = 0;
				_sources.Add(s);

				if (!_sourceLookUp.ContainsKey(root))
					_sourceLookUp.Add(root, new List<NavMeshBuildSource>());
				_sourceLookUp[root].Add(s);
			}
		}

		private void RemoveFromNavMesh(GameObject root)
		{
			if (_sourceLookUp.ContainsKey(root))
			{
				foreach (NavMeshBuildSource s in _sourceLookUp[root])
				{
					_sources.Remove(s);
				}
				_sourceLookUp.Remove(root);
			}
		}

		private void RebuildAllNavMesh_BruteForce()
		{
			var root = FindObjectOfType<SyncManager>().syncRoot;

			_sources.Clear();

			// get all meshfilters in hierarchy
			_meshFilters = root.GetComponentsInChildren<MeshFilter>();

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
				//if (mData && ContainsData(mData, ignoreFilters))
				//	continue;

				// extending bounds
				Renderer renderer = _meshFilters[i].GetComponent<Renderer>();
				_bounds.Encapsulate(renderer.bounds.min);
				_bounds.Encapsulate(renderer.bounds.max);

				// add Mesh Colliders for interacting with objects
				//if (addMeshColliders)
				//	_meshFilters[i].gameObject.AddComponent<MeshCollider>().isTrigger = isTrigger;

				Mesh m = _meshFilters[i].sharedMesh;
				if (m == null) continue;

				NavMeshBuildSource s = new NavMeshBuildSource();
				s.shape = NavMeshBuildSourceShape.Mesh;
				s.sourceObject = m;
				s.transform = _meshFilters[i].transform.localToWorldMatrix;
				//s.area = (mData && ContainsData(mData, notWalkable)) ? 1 : 0;
				s.area = 0;
				_sources.Add(s);
			}
			UpdateNavMesh();
		}
	}
}