using System.Collections.Generic;
using Unity.Reflect.Model;
using System.Linq;
using System;

namespace UnityEngine.Reflect.Extensions.Helpers
{
	[AddComponentMenu("Reflect/Helpers/Events Manager")]
	[DisallowMultipleComponent]
	public class ReflectEventsManager : MonoBehaviour
	{
		/// <summary>
		/// A new SyncObject is created. Passes the instantiated GameObject.
		/// </summary>
		public event Action<GameObject> onSyncObjectCreated;
		/// <summary>
		/// A SyncObject is destroyed. Passes the Identifier.
		/// </summary>
		public event Action<SyncObjectBinding.Identifier> onSyncObjectDestroyed;
		/// <summary>
		/// Returns true when all expected incoming objects are instantiated in the scene.
		/// </summary>
		public event Action<bool> onIsDoneInstantiating;
		/// <summary>
		/// 
		/// </summary>
		public event Action onSyncUpdateBegin;
		/// <summary>
		/// 
		/// </summary>
		public event Action onSyncUpdateEnd;

		SyncManager _syncManager;
		StreamingCamera _streamingCamera;

		Dictionary<SyncInstance, SyncPrefab> _allSyncPrefabs = new Dictionary<SyncInstance, SyncPrefab>();
		int _objectCountInSyncPrefabs, _objectCountInScene;
		bool _isDoneInstantiating, _streamingInUse;

		/// <summary>
		/// The number of gameObjects found in SyncPrefabs.
		/// </summary>
		public int ObjectCountInSyncPrefabs
		{
			get => _objectCountInSyncPrefabs;
			private set
			{
				if (_objectCountInSyncPrefabs == value)
					return;
				_objectCountInSyncPrefabs = value;

				UpdatedInDoneInstantiatingState();
			}
		}

		/// <summary>
		/// The number of gameObjects currently instantiated in the Scene.
		/// </summary>
		public int ObjectCountInScene
		{
			get => _objectCountInScene;
			private set
			{
				if (_objectCountInScene == value)
					return;
				_objectCountInScene = value;

				UpdatedInDoneInstantiatingState();
			}
		}

		private void UpdatedInDoneInstantiatingState ()
		{
			IsDoneInstantiating = ObjectCountInScene == ObjectCountInSyncPrefabs || (_streamingInUse && ObjectCountInScene == _streamingCamera.m_MaximumObjects);
		}

		/// <summary>
		/// IsDoneInstantiating. Returns true while the model is fully instanciated.
		/// </summary>
		public bool IsDoneInstantiating
		{
			get => _isDoneInstantiating;
			private set
			{
				if (_isDoneInstantiating == value)
					return;
				_isDoneInstantiating = value;

				onIsDoneInstantiating?.Invoke(_isDoneInstantiating);
			}
		}

		private static ReflectEventsManager _instance;
		public static ReflectEventsManager Instance
		{
			get
			{
				if (_instance == null)
					_instance = FindObjectOfType<ReflectEventsManager>();

				if (_instance == null)
					_instance = new GameObject("Reflect Events Manager", new Type[1] { typeof(ReflectEventsManager) }).GetComponent<ReflectEventsManager>();

				return _instance;
			}
			set => _instance = value;
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;

			_syncManager = FindObjectOfType<SyncManager>();
			_streamingCamera = FindObjectOfType<StreamingCamera>();
			_streamingInUse = _streamingCamera != null && _streamingCamera.enabled && _streamingCamera.m_MaximumObjects != 0;

			if (_syncManager == null)
				enabled = false;
		}

		private void OnEnable()
		{
			if (_syncManager == null)
			{
				enabled = false;
				return;
			}

			_syncManager.onProjectClosed += SyncManager_onProjectClosed;
			_syncManager.onSyncUpdateBegin += SyncManager_onSyncUpdateBegin;
			_syncManager.onSyncUpdateEnd += SyncManager_onSyncUpdateEnd;
			_syncManager.onInstanceAdded += SyncManager_onInstanceAdded;
		}

		private void OnDisable()
		{
			if (_syncManager == null)
				return;

			_syncManager.onProjectClosed -= SyncManager_onProjectClosed;
			_syncManager.onSyncUpdateBegin -= SyncManager_onSyncUpdateBegin;
			_syncManager.onSyncUpdateEnd -= SyncManager_onSyncUpdateEnd;
			_syncManager.onInstanceAdded -= SyncManager_onInstanceAdded;
		}

		private void SyncManager_onInstanceAdded(SyncInstance instance)
		{
			instance.onObjectCreated += Instance_onObjectCreated;
			instance.onObjectDestroyed += Instance_onObjectDestroyed;
			instance.onPrefabLoaded += Instance_onPrefabLoaded;
			instance.onPrefabChanged += Instance_onPrefabChanged;
		}

		private void Instance_onPrefabLoaded(SyncInstance instance, SyncPrefab prefab)
		{
			if (!_allSyncPrefabs.ContainsKey(instance))
				_allSyncPrefabs.Add(instance, prefab);
			UpdateSyncPrefabsObjectsCount();
		}

		private void UpdateSyncPrefabsObjectsCount()
		{
			ObjectCountInSyncPrefabs = _allSyncPrefabs.Sum(x => x.Value.Instances.Count);
		}

		private void Instance_onPrefabChanged(SyncInstance instance, SyncPrefab prefab)
		{
			_allSyncPrefabs[instance] = prefab;
			UpdateSyncPrefabsObjectsCount();
		}

		private void Instance_onObjectDestroyed(SyncObjectBinding obj)
		{
			ObjectCountInScene--;

			onSyncObjectDestroyed?.Invoke(obj.identifier);
		}

		private void Instance_onObjectCreated(SyncObjectBinding obj)
		{
			ObjectCountInScene++;

			onSyncObjectCreated?.Invoke(obj.gameObject);
		}

		private void SyncManager_onSyncUpdateEnd(bool hasChanged)
		{
			IsDoneInstantiating = ObjectCountInScene == ObjectCountInSyncPrefabs || (_streamingCamera && _streamingCamera.enabled && ObjectCountInScene == _streamingCamera.m_MaximumObjects);

			onSyncUpdateEnd?.Invoke();
		}

		private void SyncManager_onSyncUpdateBegin()
		{
			IsDoneInstantiating = false;

			onSyncUpdateBegin?.Invoke();
		}

		private void SyncManager_onProjectClosed()
		{
			_allSyncPrefabs.Clear();
			ObjectCountInScene = ObjectCountInSyncPrefabs = 0;
		}
	}
}