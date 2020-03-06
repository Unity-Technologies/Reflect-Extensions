using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions.Rules
{
    /// <summary>
    /// ReplaceObjects
    /// Replaces objects filtered by Metadata Key/Value Pair, with a Prefab.
    /// </summary>
    [AddComponentMenu("Reflect/Rules/Replace Objects")]
    public class ReplaceObjects : MonoBehaviour
    {
        [Tooltip("Child Prefab to add to objects.")]
        [SerializeField] private GameObject _childPrefab = default;
        [Tooltip("Metadata Key.")]
        [SerializeField] private string _key = "Category";
        [Tooltip("Metadata Value.")]
        [SerializeField] private string _value = "Planting";
        [Tooltip("Disable Original Renderers.")]
        [SerializeField] private bool _disableOriginalRenderers = default;

        SyncManager _syncManager;
        Dictionary<SyncObjectBinding.Identifier, SyncObjectBinding> _modifiedObjects = new Dictionary<SyncObjectBinding.Identifier, SyncObjectBinding>();
        Dictionary<GameObject, GameObject> _addedObjects = new Dictionary<GameObject, GameObject>();
        List<SyncInstance> _instances = new List<SyncInstance>(); // listing instances we subscribe to
        const string MODIFIED_TOKEN = "[modified]"; // name of a child object added to watch for object reset

        private void Awake()
        {
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

        private void SyncManager_ProjectClosed()
        {
            foreach (SyncInstance instance in _instances)
            {
                instance.onObjectCreated -= Instance_ObjectCreated;
                instance.onObjectDestroyed -= Instance_ObjectDestroyed;
            }
            _instances.Clear();
            _modifiedObjects.Clear();
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
            //Debug.LogFormat("<color=yellow>Sync Updated End {0}</color>", hasChanged ? "with changes." : "no change");
            if (hasChanged)
                StartCoroutine(ForceRespawn());
        }

        private IEnumerator ForceRespawn()
        {
            yield return null; // breathe

            foreach (KeyValuePair<SyncObjectBinding.Identifier, SyncObjectBinding> kvp in _modifiedObjects)
                if (kvp.Value.transform.Find(MODIFIED_TOKEN) == null) // if object was reset
                    SpawnChild(kvp.Value.transform); // respawn
        }

        private void SyncManager_InstanceAdded(SyncInstance instance)
        {
            //Debug.Log("<color=yellow>New instance added.</color>");
            instance.onObjectCreated += Instance_ObjectCreated;
            instance.onObjectDestroyed += Instance_ObjectDestroyed;
            _instances.Add(instance);
        }

        private void Instance_ObjectDestroyed(SyncObjectBinding obj)
        {
            if (_modifiedObjects.ContainsKey(obj.identifier))
            {
                if (_addedObjects.ContainsKey(obj.gameObject))
                {
                    DestroyImmediate(_addedObjects[obj.gameObject]); // destroying added object before Reflect collects children renderers
                    _addedObjects.Remove(obj.gameObject);
                }
                _modifiedObjects.Remove(obj.identifier);
            }
        }

        private void Instance_ObjectCreated(SyncObjectBinding obj)
        {
            var md = obj.gameObject.GetComponent<Metadata>();
            if (md != null)
            {
                if (md.GetParameter(_key) == _value)
                {
                    //Debug.LogFormat(obj.gameObject, "<color=green>Object {0} created.</color>", obj.name);
                    if (!_modifiedObjects.ContainsKey(obj.identifier)) //skipping duplicates
                    {
                        _modifiedObjects.Add(obj.identifier, obj);
                        SpawnChild(obj.transform);
                    }
                }
            }
        }

        private void SpawnChild(Transform parent)
        {
            // disabling existing renderers
            if (_disableOriginalRenderers)
                foreach (Renderer r in parent.GetComponentsInChildren<Renderer>())
                    r.enabled = false;

            // adding an empty subobject to track object reset
            new GameObject(MODIFIED_TOKEN).transform.SetParent(parent);

            // instantiating prefab as a child and putting the reference in the Dictionary
            _addedObjects[parent.gameObject] = Instantiate(_childPrefab, parent, false);
        }
    }
}