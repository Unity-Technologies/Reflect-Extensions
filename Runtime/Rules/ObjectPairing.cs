using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions.Rules
{
    /// <summary>
    /// Pairs objects filtered by Metadata Key/Value Pair, with a Prefab.
    /// </summary>
    [AddComponentMenu("Reflect/Rules/Object Pairing")]
    public class ObjectPairing : MonoBehaviour
    {
        [Tooltip("Prefab to instantiate.")]
        [SerializeField] GameObject prefab = default;
        [Tooltip("Metadata Key.")]
        [SerializeField] private string _key = "Category";
        [Tooltip("Metadata Value.")]
        [SerializeField] private string _value = "Planting";
        [Tooltip("Scale Prefab so that its height matches the Height parameter found in Metadata (if present)")]
        [SerializeField] private bool _matchHeight = default;

        SyncManager _syncManager;
        List<SyncObjectBinding.Identifier> _identifiers = new List<SyncObjectBinding.Identifier>();
        Dictionary<SyncObjectBinding, GameObject> _addedObjects = new Dictionary<SyncObjectBinding, GameObject>();
        List<SyncInstance> _instances = new List<SyncInstance>(); // listing instances we subscribe to
        Bounds prefabBounds = new Bounds();

        private void Awake()
        {
            if (prefab == null || _key.Length == 0 || _value.Length == 0)
            {
                enabled = false;
                return;
            }

            _syncManager = FindObjectOfType<SyncManager>();

            if (_syncManager == null)
            {
                enabled = false;
                return;
            }

            _syncManager.onInstanceAdded += SyncManager_InstanceAdded;
            _syncManager.onSyncUpdateEnd += SyncManager_SyncUpdateEnd;
            _syncManager.onProjectClosed += SyncManager_ProjectClosed;
        }

        private void Start()
        {
            if (_matchHeight)
            {
                foreach (MeshFilter m in prefab.GetComponentsInChildren<MeshFilter>())
                    prefabBounds.Encapsulate(m.sharedMesh.bounds);
            }
        }

        private void SyncManager_ProjectClosed()
        {
            foreach (GameObject g in _addedObjects.Values)
                Destroy(g);

            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }

            _instances.Clear();
            _addedObjects.Clear();
        }

        private void OnDestroy()
        {
            if (_syncManager == null)
                return;

            _syncManager.onInstanceAdded -= SyncManager_InstanceAdded;
            _syncManager.onSyncUpdateEnd -= SyncManager_SyncUpdateEnd;

            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }
        }

        private void SyncManager_SyncUpdateEnd(bool hasChanged)
        {
            if (hasChanged)
                StartCoroutine(UpdateTransforms());
        }

        private IEnumerator UpdateTransforms()
        {
            for (int i = 0; i < 2; i++)
                yield return null; // breathe, twice

            foreach (KeyValuePair<SyncObjectBinding, GameObject> kvp in _addedObjects)
            {
                kvp.Value.transform.position = kvp.Key.transform.position;
                kvp.Value.transform.rotation = kvp.Key.transform.rotation;
            }
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            instance.onObjectCreated += Instance_ObjectCreated;
            instance.onObjectDestroyed += Instance_ObjectDestroyed;
            _instances.Add(instance);
        }

        private void Instance_ObjectDestroyed(SyncObjectBinding obj)
        {
            if (_identifiers.Contains(obj.identifier))
            {
                if (_addedObjects.ContainsKey(obj))
                {
                    Destroy(_addedObjects[obj]);
                    _addedObjects.Remove(obj);
                }
                _identifiers.Remove(obj.identifier);
            }
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            if (_identifiers.Contains(obj.identifier)) // skipping duplicates
                return;

            var md = obj.GetComponent<Metadata>();
            if (md == null) // skipping objects with no Metadata
                return;

            if (md.GetParameter(_key) == _value)
            {
                _identifiers.Add(obj.identifier);
                _addedObjects[obj] = Instantiate(prefab, obj.transform.position, obj.transform.rotation, transform);
                if (_matchHeight && md.parameters.dictionary.ContainsKey("Height"))
                {
                    var height = float.Parse(md.GetParameter("Height")) * 0.001f;
                    _addedObjects[obj].transform.localScale = Vector3.one * (height / prefabBounds.size.y);
                }
            }
        }
    }
}